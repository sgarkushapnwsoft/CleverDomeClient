using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using CleverDomeCommon;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using CleverDomeSSO.Metadata;
using CleverDomeDocumentManagement.Data;
using System.Configuration;
using System.Net;

namespace CleverDomeClient
{
    static class Program
    {
        static string userID = ConfigurationManager.AppSettings["TestUserID"];
        static string vendorName = ConfigurationManager.AppSettings["TestVendorName"];
        static int applicationID = int.Parse(ConfigurationManager.AppSettings["TestApplicationID"]);
		static string certPath = ConfigurationManager.AppSettings["VendorCertificatePath"];
        static int templateID = 0;
        static int descriptionID = 79;
        static string certPassword = ConfigurationManager.AppSettings["CertPassword"];
        static string cleverCertPath = ConfigurationManager.AppSettings["CleverDomeCertPath"];
        static string testFilePath = "TestFile.pdf";

        static void Main()
        {
            string allowedIPs = ""; //" + HttpContext.Current.UserHostAddress; // If you want access our service directly from browser.
            var ssoRequester = new SSORequester(GetClientCertificate(), GetServerCertificate());
            Guid? sessionID = ssoRequester.GetSessionID(userID, vendorName, allowedIPs);
            Guid documentGuid = default(Guid);
            if (sessionID.HasValue)
            {
                Console.WriteLine("SAML request completed. SessionID = {0}.", sessionID);
                ChannelFactory<IWidgets> channelFactory = new ChannelFactory<IWidgets>("BasicHttpBinding_IWidgets");
                IWidgets widgets = channelFactory.CreateChannel();
                PrintTemplatesAndDescriptions(widgets, sessionID.Value);
                documentGuid = UploadFile(widgets, sessionID.Value, testFilePath);
                Console.WriteLine("Uploaded file guid: {0}", documentGuid);
                SetTestMetadata(widgets, sessionID.Value, documentGuid);
                TestSecurityGroups(widgets, sessionID.Value, documentGuid);
                channelFactory.Close();
            }
            else
            {
                Console.WriteLine("SAML request not completed.");
            }

            Console.WriteLine("Press 'Y' if you want to add user.");
            string line = Console.ReadLine();
            int userCreatedID = default(int);
            if (line.ToLower()[0] == 'y')
            {
                userCreatedID = UserCreation();
            }

            if (userCreatedID > 0)
            {
                Console.WriteLine("Press 'Y' if you want to add permission to created user.");
                line = Console.ReadLine();
                if (line.ToLower()[0] == 'y')
                {
                    Console.WriteLine("SAML request completed. SessionID = {0}.", sessionID);
                    ChannelFactory<IWidgets> channelFactory = new ChannelFactory<IWidgets>("BasicHttpBinding_IWidgets");
                    IWidgets widgets = channelFactory.CreateChannel();
                    SetTestPermission(widgets, sessionID.Value, userCreatedID, documentGuid);
                    channelFactory.Close();
                }
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        static void PrintTemplatesAndDescriptions(IWidgets widgets, Guid sessionID)
        {
            foreach (int templateID in GetTemplatesByApplication(widgets, sessionID, applicationID))
            {
                GetDescriptionsByApplication(widgets, sessionID, applicationID, templateID);
            }
        }

        static List<int> GetTemplatesByApplication(IWidgets widgetsClient, Guid sessionID, int applicationID)
        {
            List<int> templatesID = new List<int>();
            foreach (var template in widgetsClient.GetDocumentTemplates(sessionID, applicationID).ReturnValue)
            {
                Console.WriteLine("templateID={0}, Name={1}", template.ID, template.Name);
                templatesID.Add(template.ID);
            }
            return templatesID;
        }

        static List<int?> GetDescriptionsByApplication(IWidgets widgetsClient, Guid sessionID, int applicationID, int templateID)
        {
            List<int?> descriptionsID = new List<int?>();
            foreach (var type in widgetsClient.GetDocumentTypes(sessionID, templateID, applicationID).ReturnValue)
            {
                Console.WriteLine("         templateID={0}, descriptionID={1}, Name={2}", templateID, type.ID, type.Name);
                descriptionsID.Add(type.ID);
            }
            return descriptionsID;
        }

        private static void SetTestPermission(IWidgets widgets, Guid sessionID, int userID, Guid documentGuid)
        {
            PrintPermissions();

            Console.WriteLine("Enter permission level:");
            string strPermissionID = Console.ReadLine();
            int permissionID = int.Parse(strPermissionID);

            var res = widgets.SetPermissionsForUser(sessionID, new Guid[] { documentGuid }, userID, permissionID);

            if (res.Result == ResultType.Success)
            {
                Console.WriteLine("Successfully added permission.");

                var userPermissions = widgets
                    .GetDocumentSharingInfo(sessionID, documentGuid).ReturnValue
                    .DocumentUsersSharing.Where(u => u.UserID == userID).Single()
                    .Permissions;

                PrintPermissions(userPermissions.ID);
            }
        }

        private static void PrintPermissions(int permissionLevel = (int)PermissionLevel.Admin)
        {
            Console.WriteLine("List of all permissions:");

            foreach (var permission in
                GetEnumValues<PermissionLevel>().Where(p => (int)p <= permissionLevel))
            {
                Console.WriteLine("{0}: {1}", (int)permission, permission.ToString());
            }

            Console.WriteLine();
        }

        private static IEnumerable<T> GetEnumValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        private static int UserCreation()
        {
			var channelFactory = new ChannelFactory<IVendorManagement>("MaxClockSkewBinding");
            channelFactory.Credentials.ClientCertificate.Certificate = GetClientCertificate();
            IVendorManagement vendorMgmt = channelFactory.CreateChannel();

            Console.WriteLine("Please enter UserID from your system to verify that this person doesn't exist in our database.");
            string externalUserID = Console.ReadLine();

            int userID = 0;
            bool userExists = CheckUser(vendorMgmt, externalUserID, ref userID);
            if (userExists)
            {
                Console.WriteLine("Sorry, but this user already exists.");
            }
            else
            {
                Console.WriteLine("User is not present in our database. Please enter necessary information to create a new user.");

                Console.WriteLine("First Name:");
                string firstName = Console.ReadLine();

                Console.WriteLine("Last Name:");
                string lastName = Console.ReadLine();

                Console.WriteLine("Email:");
                string email = Console.ReadLine();

                Console.WriteLine("Phone:");
                string phone = Console.ReadLine();

                try
                {
                    userID = CreateUser(vendorMgmt, externalUserID, firstName, lastName, email, phone);
                    Console.WriteLine("User has been created successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Sorry, but user has not been created.");
                }
            }

            return userID;
        }

        static Guid UploadFile(IWidgets widgetsClient, Guid sessionID, string fileName)
        {
            Stream postStream;
            using (FileStream file = new FileStream(fileName, FileMode.Open))
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("sessionID", sessionID.ToString());
                parameters.Add("templateID", templateID.ToString());
                parameters.Add("descriptionID", descriptionID.ToString());
                parameters.Add("applicationID", applicationID.ToString());
                postStream = StreamBuilder.Build(file, Path.GetFileName(fileName), parameters);
            }

            Guid documentID = widgetsClient.UploadFile(postStream);
            postStream.Close();
            return documentID;
        }

        static void SetTestMetadata(IWidgets widgets, Guid sessionID, Guid documentGuid)
        {
            Console.WriteLine("--- Metadata before ---");
            PrintMetadata(widgets, sessionID, documentGuid);
            DocumentMetadataValueBase[] values = new DocumentMetadataValueBase [1]
            {
                new DocumentMetadataValueBase{FieldID= 99, FieldValue = "https://google.com" }
            };

            var result = widgets.SetMetadataValues(sessionID, documentGuid, values, new int[0]);
            if (result.Result != ResultType.Success)
            {
                Console.WriteLine("Error! {0}", result.Message);
            }

            Console.WriteLine();
            Console.WriteLine("--- Metadata after ---");
            PrintMetadata(widgets, sessionID, documentGuid);

            RemoveAllMetadata(widgets, sessionID, documentGuid);

            Console.WriteLine();
            Console.WriteLine("--- Metadata after removing all values ---");
            PrintMetadata(widgets, sessionID, documentGuid);
        }

        static void PrintMetadata(IWidgets widgets, Guid sessionID, Guid documentGuid)
        {
            foreach (var metadataValue in widgets.GetDocumentMetadataBase(sessionID, documentGuid).ReturnValue)
	        {
                Console.WriteLine("TypeID={0}, TypeName={1}, ValueID={2}, Value={3}", metadataValue.FieldID, 
                    metadataValue.FieldName, metadataValue.FieldValueID, metadataValue.FieldValue);
	        } 
        }

        static void RemoveAllMetadata(IWidgets widgets, Guid sessionID, Guid documentGuid)
        {
            int[] idsToRemove = widgets.GetDocumentMetadataBase(sessionID, documentGuid).ReturnValue.Select(x => x.FieldValueID.Value).ToArray();
            var result = widgets.SetMetadataValues(sessionID, documentGuid, new DocumentMetadataValueBase[0], idsToRemove);
            if (result.Result != ResultType.Success)
            {
                Console.WriteLine("Error! {0}", result.Message);
            }
        }

        static X509Certificate2 GetClientCertificate()
        {
            return SSOTools.GetCertificate(certPath, certPassword);
        }

        static X509Certificate2 GetServerCertificate()
        {
            return new X509Certificate2(cleverCertPath);
        }

        static int CreateUser(IVendorManagement vendorMgmt, string userID, string firstName, string lastName, string email, string phone)
        {
            return vendorMgmt.CreateUser(userID, vendorName, firstName, lastName, email, phone);
        }

        static bool CheckUser(IVendorManagement vendorMgmt, string externalUserID, ref int userID)
        {
            var nullUserID = vendorMgmt.CheckUser(externalUserID, vendorName);
            if (nullUserID.HasValue)
            {
                userID = nullUserID.Value;
            }
            return nullUserID.HasValue;
        }

        #region Security Group Test

        private static void PrintSecurityGroup(IWidgets widgets, Guid sessionID, int securityGroupID)
        {
            UserData[] users = widgets.GetUsersForGroup(sessionID, securityGroupID).ReturnValue;
            if (users.Length == 0)
            {
                Console.WriteLine("No users in the security group.");
            }
            else
            {
                Console.WriteLine("Security group users:");
                foreach (UserData user in users)
                {
                    Console.WriteLine("\tID: {0}, Name: {1}.", user.ID, user.FullName);
                }
            }
        }

        private static void RemoveAllUsersFromSecurityGroup(IWidgets widgets, Guid sessionID, int securityGroupID)
        {
            UserData[] users = widgets.GetUsersForGroup(sessionID, securityGroupID).ReturnValue;
            foreach (UserData user in users)
            {
                widgets.RemoveUserFromSecurityGroup(sessionID, securityGroupID, user.ID);
            }

            Console.WriteLine("Remove users from the security group");
        }

        private static int GetInternalUserID(string externalUserID)
        {
			var channelFactory = new ChannelFactory<IVendorManagement>("MaxClockSkewBinding");
            channelFactory.Credentials.ClientCertificate.Certificate = GetClientCertificate();
            IVendorManagement vendorMgmt = channelFactory.CreateChannel();
            int internalUserID = vendorMgmt.CheckUser(externalUserID, vendorName).Value;
            channelFactory.Close();
            return internalUserID;
        }

        private static void TestSecurityGroups(IWidgets widgets, Guid sessionID, Guid documentGuid)
        {
            var createGroupResult = widgets.CreateSecurityGroup(sessionID, "Test 1", "Test Security Group 1", 1, GetInternalUserID(Program.userID));
            int securityGroupID = createGroupResult.ReturnValue.ID.Value;
            List<int> usersToAdd = new List<int> { 1, 2, 3 };
            foreach (int userID in usersToAdd)
            {
                widgets.AddUserToSecurityGroup(sessionID, securityGroupID, userID);
            }

            PrintSecurityGroup(widgets, sessionID, securityGroupID);
            RemoveAllUsersFromSecurityGroup(widgets, sessionID, securityGroupID);
            PrintSecurityGroup(widgets, sessionID, securityGroupID);

            widgets.AttachSecurityGroupsToDocument(sessionID, documentGuid, new int[] { securityGroupID }, (int)PermissionLevel.Modify);
            var groupsBeforeDeletion = widgets.GetSecurityGroups(sessionID, documentGuid).ReturnValue;
            Console.WriteLine("Groups count before removing: {0}.", groupsBeforeDeletion.DocumentSecurityData.Length);

            widgets.RemoveSecurityGroupFromDocument(sessionID, documentGuid, securityGroupID);
            var groupsAfterDeletion = widgets.GetSecurityGroups(sessionID, documentGuid).ReturnValue;
            Console.WriteLine("Groups count after removing: {0}.", groupsAfterDeletion.DocumentSecurityData.Length);

            widgets.RemoveSecurityGroup(sessionID, securityGroupID);
        }

        #endregion

        static void CreateClient()
        {
            string allowedIPs = "";
            var ssoRequester = new SSORequester(GetClientCertificate(), GetServerCertificate());
            Guid? sessionID = ssoRequester.GetSessionID(userID, vendorName, allowedIPs);
            var sessionGuid = sessionID.Value;

            ChannelFactory<IWidgets> channelFactory = new ChannelFactory<IWidgets>("BasicHttpBinding_IWidgets");
            IWidgets widgets = channelFactory.CreateChannel();

            var channelFactory2 = new ChannelFactory<IVendorManagement>("MaxClockSkewBinding");
            channelFactory2.Credentials.ClientCertificate.Certificate = GetClientCertificate();
            IVendorManagement vendorMgmt = channelFactory2.CreateChannel();

            Console.WriteLine("Please enter UserID from your system to verify that this person doesn't exist in our database.");
            string externalUserID = Console.ReadLine();

            Console.WriteLine("First Name:");
            string firstName = Console.ReadLine();

            Console.WriteLine("Last Name:");
            string lastName = Console.ReadLine();

            Console.WriteLine("Email:");
            string email = Console.ReadLine();

            Console.WriteLine("Phone:");
            string phone = Console.ReadLine();

            Console.WriteLine("SecurityGroup Name:");
            string securityGroupName = Console.ReadLine();

            Console.WriteLine("SecurityGroup Description:");
            string securityGroupDescription = Console.ReadLine();

            int clientID, securityGroupID;
            CreateClient(vendorMgmt, widgets, sessionGuid,
                externalUserID, firstName, lastName, email, phone,
                securityGroupName, securityGroupDescription,
                out clientID, out securityGroupID);

            Console.WriteLine("ClientID: {0}", clientID);
            Console.WriteLine("SecurityGroupID: {0}", securityGroupID);

            channelFactory2.Close();
            channelFactory.Close();
        }

        static void DeleteUser()
        {
            string allowedIPs = "";
            var ssoRequester = new SSORequester(GetClientCertificate(), GetServerCertificate());
            Guid? sessionID = ssoRequester.GetSessionID(userID, vendorName, allowedIPs);
            var sessionGuid = sessionID.Value;

            ChannelFactory<IWidgets> channelFactory = new ChannelFactory<IWidgets>("BasicHttpBinding_IWidgets");
            IWidgets widgets = channelFactory.CreateChannel();

            var channelFactory2 = new ChannelFactory<IVendorManagement>("MaxClockSkewBinding");
            channelFactory2.Credentials.ClientCertificate.Certificate = GetClientCertificate();
            IVendorManagement vendorMgmt = channelFactory2.CreateChannel();

            Console.WriteLine("Please enter UserID from your system to delete the user.");
            string externalUserID = Console.ReadLine();

            DeleteUser(vendorMgmt, externalUserID);
            Console.WriteLine("User is deleted sucessfully.");

            channelFactory2.Close();
            channelFactory.Close();
        }

        static void AddClientToAdvisor()
        {
            string allowedIPs = "";
            var ssoRequester = new SSORequester(GetClientCertificate(), GetServerCertificate());
            Guid? sessionID = ssoRequester.GetSessionID(userID, vendorName, allowedIPs);
            var sessionGuid = sessionID.Value;

            ChannelFactory<IWidgets> channelFactory = new ChannelFactory<IWidgets>("BasicHttpBinding_IWidgets");
            IWidgets widgets = channelFactory.CreateChannel();

            var channelFactory2 = new ChannelFactory<IVendorManagement>("MaxClockSkewBinding");
            channelFactory2.Credentials.ClientCertificate.Certificate = GetClientCertificate();
            IVendorManagement vendorMgmt = channelFactory2.CreateChannel();

            Console.WriteLine("Client User ID:");
            int clientID = int.Parse(Console.ReadLine());

            Console.WriteLine("Advisor User ID:");
            int advisorID = int.Parse(Console.ReadLine());

            AddClientToAdvisor(clientID, advisorID,
                vendorMgmt, widgets, sessionGuid);

            channelFactory2.Close();
            channelFactory.Close();
        }

        static void RemoveClientFromAdvisor()
        {
            string allowedIPs = "";
            var ssoRequester = new SSORequester(GetClientCertificate(), GetServerCertificate());
            Guid? sessionID = ssoRequester.GetSessionID(userID, vendorName, allowedIPs);
            var sessionGuid = sessionID.Value;

            ChannelFactory<IWidgets> channelFactory = new ChannelFactory<IWidgets>("BasicHttpBinding_IWidgets");
            IWidgets widgets = channelFactory.CreateChannel();

            var channelFactory2 = new ChannelFactory<IVendorManagement>("MaxClockSkewBinding");
            channelFactory2.Credentials.ClientCertificate.Certificate = GetClientCertificate();
            IVendorManagement vendorMgmt = channelFactory2.CreateChannel();

            Console.WriteLine("Client User ID:");
            int clientID = int.Parse(Console.ReadLine());

            Console.WriteLine("Advisor User ID:");
            int advisorID = int.Parse(Console.ReadLine());

            RemoveClientFromAdvisor(clientID, advisorID,
                vendorMgmt, widgets, sessionGuid);

            channelFactory2.Close();
            channelFactory.Close();
        }

        static void CreateClient(IVendorManagement vendorMgmt, IWidgets widgets, Guid sessionID,
            string externalUserID, string firstName, string lastName, string email, string phone,
            string securityGroupName, string securityGroupDescription,
            out int clientID, out int securityGroupID)
        {
            clientID = vendorMgmt.CreateUser(externalUserID, vendorName, firstName, lastName, email, phone);
            
            var response = widgets.CreateSecurityGroup(sessionID, securityGroupName, securityGroupDescription, (int)CleverDomeCommon.SecurityGroupType.Client, clientID);
            if (response.Result != ResultType.Success)
            {
                throw new Exception(response.Message);
            }

            securityGroupID = response.ReturnValue.ID.Value;
        }

        static void CreateAdvisor(IVendorManagement vendorMgmt, IWidgets widgets, Guid sessionID,
            string externalUserID, string firstName, string lastName, string email, string phone,
            string securityGroupName, string securityGroupDescription,
            out int advisorID, out int securityGroupID)
        {
            advisorID = vendorMgmt.CreateUser(externalUserID, vendorName, firstName, lastName, email, phone);

            var response = widgets.CreateSecurityGroup(sessionID, securityGroupName, securityGroupDescription, (int)CleverDomeCommon.SecurityGroupType.Owner, advisorID);
            if (response.Result != ResultType.Success)
            {
                throw new Exception(response.Message);
            }

            securityGroupID = response.ReturnValue.ID.Value;
        }

        static void DeleteUser(IVendorManagement vendorMgmt, string externalUserID)
        {
            vendorMgmt.DeleteUser(externalUserID, vendorName);
        }

        static void AddClientToAdvisor(int clientID, int advisorID,
            IVendorManagement vendorMgmt, IWidgets widgets, Guid sessionID)
        {
            var response = widgets.GetUserSecurityGroups(sessionID, clientID, true, (int)CleverDomeCommon.SecurityGroupType.Client);
            if (response.Result != ResultType.Success)
            {
                throw new Exception(response.Message);
            }

            var securityGroupID = response.ReturnValue.Single().ID.Value;

            widgets.AddUserToSecurityGroup(sessionID, securityGroupID, advisorID);

            vendorMgmt.AllowAccessToUserTagsHierarchy(vendorName, advisorID, clientID);
        }

        static void RemoveClientFromAdvisor(int clientID, int advisorID,
            IVendorManagement vendorMgmt, IWidgets widgets, Guid sessionID)
        {
            var response = widgets.GetUserSecurityGroups(sessionID, clientID, true, (int)CleverDomeCommon.SecurityGroupType.Client);
            if (response.Result != ResultType.Success)
            {
                throw new Exception(response.Message);
            }

            var securityGroupID = response.ReturnValue.Single().ID.Value;

            widgets.RemoveUserFromSecurityGroup(sessionID, securityGroupID, advisorID);
        }

    }
}
