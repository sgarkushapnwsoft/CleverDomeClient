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
                Console.WriteLine("SAML request completed");
                Console.WriteLine("SessionID = {0}.", sessionID);
                Console.WriteLine();

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

            TestUserManagement(sessionID.Value, documentGuid);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void PrintTemplatesAndDescriptions(IWidgets widgets, Guid sessionID)
        {
            Console.WriteLine("Document templates:");
            var templates = widgets.GetDocumentTemplates(sessionID, applicationID).ReturnValue;
            foreach (var template in templates)
            {
                PrintDocumentTemplate(template);   
                var documentTypes = widgets.GetDocumentTypes(sessionID, template.ID, applicationID).ReturnValue;

                Console.WriteLine("Document types for template '{0}'", template.Name);
                PrintDocumentTypes(documentTypes);
            }

            Console.WriteLine();
        }

        public static void PrintDocumentTemplate(DocumentTemplate template)
        {
            Console.WriteLine("Template ID: {0}, Name: {1}", template.ID, template.Name);
        }

        public static void PrintDocumentTypes(IEnumerable<DocumentType> types)
        {
            foreach (var type in types)
            {
                Console.WriteLine("  ID: {0}, Name: {1}", type.ID, type.Name);
            }
        }

        private static void SetTestPermission(IWidgets widgets, Guid sessionID, int userID, Guid documentGuid)
        {
            var res = widgets.SetPermissionsForUser(sessionID, new Guid[] { documentGuid }, userID, (int)PermissionLevel.Share);

            if (res.Result == ResultType.Success)
            {
                Console.WriteLine("Permission has been added successfully");

                var userPermissions = widgets
                    .GetDocumentsSharingInfo(sessionID, new Guid[] { documentGuid }).ReturnValue
                    .DocumentUsersSharing.Where(u => u.UserID == userID).Single()
                    .Permissions;

                PrintPermissions(userPermissions.ID);
            }
            else
            {
                Console.WriteLine("Error occured while adding permission");
            }
        }

        private static void PrintPermissions(int permissionLevel = (int)PermissionLevel.Admin)
        {
            Console.WriteLine("List of user permissions to the document:");

            foreach (var permission in GetEnumValues<PermissionLevel>().Where(p => (int)p <= permissionLevel))
            {
                Console.WriteLine("{0}: {1}", (int)permission, permission.ToString());
            }

            Console.WriteLine();
        }

        private static IEnumerable<T> GetEnumValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        private static int CreateUser(string externalUserID)
        {
            var channelFactory = new ChannelFactory<IVendorManagement>("MaxClockSkewBinding");
            channelFactory.Credentials.ClientCertificate.Certificate = GetClientCertificate();
            IVendorManagement vendorMgmt = channelFactory.CreateChannel();

            int userID = 0;

            try
            {
                userID = CreateUser(vendorMgmt, externalUserID, "TestFirstName", "TestLastName", string.Format("{0}@testemail.com", externalUserID), "0123456789");
                Console.WriteLine("User has been created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Sorry, but user has not been created.");
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
                parameters.Add("applicationID", applicationID.ToString());
                postStream = StreamBuilder.Build(file, Path.GetFileName(fileName), parameters);
            }

            Guid documentID = widgetsClient.UploadFile(postStream);
            postStream.Close();
            return documentID;
        }

        static void SetTestMetadata(IWidgets widgets, Guid sessionID, Guid documentGuid)
        {
            Console.WriteLine();
            Console.WriteLine("Initial document metadata:");
            PrintMetadata(widgets, sessionID, documentGuid);

            Console.WriteLine("Adding tag to the document...");
            DocumentMetadataValueBase[] values = new DocumentMetadataValueBase [1]
            {
                new DocumentMetadataValueBase{FieldID= 78, FieldValue = "Test Tag" }
            };

            var result = widgets.SetMetadataValues(sessionID, documentGuid, values, new int[0]);
            if (result.Result != ResultType.Success)
            {
                Console.WriteLine("Error! {0}", result.Message);
            }

            Console.WriteLine();
            Console.WriteLine("Document metadata after adding tag:");
            PrintMetadata(widgets, sessionID, documentGuid);

            Console.WriteLine("Removing all metadata values from the document...");
            RemoveAllMetadata(widgets, sessionID, documentGuid);

            Console.WriteLine();
            Console.WriteLine("Document metadata:");
            PrintMetadata(widgets, sessionID, documentGuid);

        }

        static void PrintMetadata(IWidgets widgets, Guid sessionID, Guid documentGuid)
        {
            var metadataValues = widgets.GetDocumentMetadataBase(sessionID, documentGuid).ReturnValue;
            if (!metadataValues.Any())
            {
                Console.WriteLine("Document metadata is empty");
            }

            foreach (var metadataValue in metadataValues)
	        {
                Console.WriteLine("TypeID: {0}, TypeName: {1}, ValueID: {2}, Value: {3}", metadataValue.FieldID, 
                    metadataValue.FieldName, metadataValue.FieldValueID, metadataValue.FieldValue);
	        }

            Console.WriteLine();
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
            Console.WriteLine("Creating security group...");
            var createGroupResult = widgets.CreateSecurityGroup(sessionID, "Test 1", "Test Security Group 1", 1, GetInternalUserID(Program.userID), applicationID);
            int securityGroupID = createGroupResult.ReturnValue.ID.Value;

            Console.WriteLine();
            Console.WriteLine("Adding users to security group...");
            List<int> usersToAdd = new List<int> { 1, 2, 3 };
            foreach (int userID in usersToAdd)
            {
                widgets.AddUserToSecurityGroup(sessionID, securityGroupID, userID);
            }

            PrintSecurityGroup(widgets, sessionID, securityGroupID);

            Console.WriteLine();
            Console.WriteLine("Removing all users from security group...");
            RemoveAllUsersFromSecurityGroup(widgets, sessionID, securityGroupID);
            PrintSecurityGroup(widgets, sessionID, securityGroupID);

            Console.WriteLine();
            Console.WriteLine("Attaching security group to uploaded document...");
            widgets.AttachSecurityGroupsToDocument(sessionID, documentGuid, new int[] { securityGroupID }, (int)PermissionLevel.Modify);
            Console.WriteLine("Security group attached");

            var groupsBeforeDeletion = widgets.GetSecurityGroups(sessionID, documentGuid).ReturnValue;
            Console.WriteLine("Document security groups count: {0}", groupsBeforeDeletion.DocumentSecurityData.Length);

            Console.WriteLine();
            Console.WriteLine("Removing security group from the document...");
            widgets.RemoveSecurityGroupFromDocument(sessionID, documentGuid, securityGroupID);
            var groupsAfterDeletion = widgets.GetSecurityGroups(sessionID, documentGuid).ReturnValue;
            Console.WriteLine("Document security groups count: {0}", groupsAfterDeletion.DocumentSecurityData.Length);

            Console.WriteLine();
            Console.WriteLine("Deleting security group...");
            widgets.RemoveSecurityGroup(sessionID, securityGroupID);
            Console.WriteLine("Security group has been successfully deleted");
        }

        #endregion

        public static void TestUserManagement(Guid sessionID, Guid documentGuid)
        {
            Console.WriteLine();
            Console.WriteLine("Creating new user..");

            string externalUserID = Guid.NewGuid().ToString();

            int createdUserID = CreateUser(externalUserID);

            if (createdUserID != 0)
            {
                ChannelFactory<IWidgets> channelFactory = new ChannelFactory<IWidgets>("BasicHttpBinding_IWidgets");
                IWidgets widgets = channelFactory.CreateChannel();

                Console.WriteLine();
                Console.WriteLine("Adding permission to uploaded document for newly created user...");

                SetTestPermission(widgets, sessionID, createdUserID, documentGuid);
                channelFactory.Close();

                UserEmailManaging(externalUserID);
            }

        }

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
            
            var response = widgets.CreateSecurityGroup(sessionID, securityGroupName, securityGroupDescription, (int)CleverDomeCommon.SecurityGroupType.Client, clientID, applicationID);
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

            var response = widgets.CreateSecurityGroup(sessionID, securityGroupName, securityGroupDescription, (int)CleverDomeCommon.SecurityGroupType.Owner, advisorID, applicationID);
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

        private static void UserEmailManaging(string externalUserID)
        {
            var channelFactory = new ChannelFactory<IVendorManagement>("MaxClockSkewBinding");
            channelFactory.Credentials.ClientCertificate.Certificate = GetClientCertificate();
            IVendorManagement vendorMgmt = channelFactory.CreateChannel();

            Console.WriteLine("User emails:");
            ListUserEmails(vendorMgmt, externalUserID);

            Console.WriteLine();
            Console.WriteLine("Adding new primary email...");
            var newPrimaryEmail = string.Format("{0}new-primary@email.com", externalUserID);
            vendorMgmt.AddUserEmail(vendorName, externalUserID, newPrimaryEmail, true);

            ListUserEmails(vendorMgmt, externalUserID);

            Console.WriteLine();
            Console.WriteLine("Adding new not primary email...");
            var newNotPrimaryEmail = string.Format("{0}new-not-primary@email.com", externalUserID);
            var notPrimaryEmailID = vendorMgmt.AddUserEmail(vendorName, externalUserID, newNotPrimaryEmail, false);

            ListUserEmails(vendorMgmt, externalUserID);

            Console.WriteLine();
            Console.WriteLine("Setting last added email primary...");
            vendorMgmt.SetUserPrimaryEmail(vendorName, externalUserID, notPrimaryEmailID);

            ListUserEmails(vendorMgmt, externalUserID);

            Console.WriteLine();
            Console.WriteLine("Removing first email...");
            var removedEmailID = vendorMgmt.GetUserEmails(vendorName, externalUserID).First().ID;
            vendorMgmt.RemoveUserEmail(vendorName, externalUserID, removedEmailID);

            ListUserEmails(vendorMgmt, externalUserID);
        }

        private static void ListUserEmails(IVendorManagement vendorMgmt, string externalUserID)
        {
            var emails = vendorMgmt.GetUserEmails(vendorName, externalUserID);
            foreach (var email in emails)
            {
                Console.WriteLine(string.Format("ID: {0}, Email: {1}, Active: {2}, Primary: {3}", email.ID, email.Email, email.Active, email.IsPrimary));
            }
        }
    }
}
