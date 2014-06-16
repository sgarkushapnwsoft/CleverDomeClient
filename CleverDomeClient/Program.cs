using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using CleverDomeCommon;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using CleverDomeCommon.SSO;
using CleverDomeDocumentManagement.Data;

namespace CleverDomeClient
{
    static class Program
    {
        static string userID = "4";
        static string vendorName = "EverNote";
        static int applicationID = 9;
        static int templateID = 0;
        static int descriptionID = 79;
        static string certPath = @"C:\Cert\CleverDomeTestCert.pfx";
        static string certPassword = "passw";

        static void Main()
        {
            X509Certificate2 cert = GetCertificate();
            string allowedIPs = ""; //" + HttpContext.Current.UserHostAddress; // If you want access our service directly from browser.
            Guid? sessionID = SSORequester.GetSessionID(userID, cert, vendorName, allowedIPs);
            if (sessionID.HasValue)
            {
                Console.WriteLine("SAML request completed. SessionID = {0}.", sessionID);
                ChannelFactory<IWidgets> channelFactory = new ChannelFactory<IWidgets>("BasicHttpBinding_IWidgets");
                IWidgets widgets = channelFactory.CreateChannel();
                Guid documentGuid = UploadFile(widgets, sessionID.Value, @"C:\TestFile.jpg");
                Console.WriteLine("Uploaded file guid: {0}", documentGuid);
                SetTestMetadata(widgets, sessionID.Value, documentGuid);
                channelFactory.Close();
            }
            else
            {
                Console.WriteLine("SAML request not completed.");
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadLine();
        }

        static Guid UploadFile(IWidgets widgetsClient, Guid sessionID, string fileName)
        {
            Stream postStream;
            using (FileStream file = new FileStream(fileName, FileMode.Open))
            {
                Dictionary<string, string> parameters = new Dictionary<string, string>();
                parameters.Add("sessionID", sessionID.ToString());
                parameters.Add("applicationID", applicationID.ToString());
                parameters.Add("templateID", templateID.ToString());
                parameters.Add("descriptionID", descriptionID.ToString());
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

        static X509Certificate2 GetCertificate()
        {
            X509Certificate2 cert = SSOTools.GetCertificate(certPath, certPassword);
            return cert;
        }

        static bool CreateUser(string userID, string firstName, string lastName, string email, string phone)
        {
            var channelFactory = new ChannelFactory<IVendorManagement>("WSHttpBinding_IVendorManagement");

            channelFactory.Credentials.ClientCertificate.Certificate = GetCertificate();

            IVendorManagement vendorMgmt = channelFactory.CreateChannel();

            return vendorMgmt.CreateUser(userID, vendorName, firstName, lastName, email, phone);
        }

    }
}
