using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using CleverDomeSSO.Metadata;
using System.ServiceModel;
using System.Configuration;

namespace CleverDomeCommon
{
    public class SSORequester
    {
        private X509Certificate2 _clientCertificate;
        private X509Certificate2 _serverCertificate;

        public SSORequester(X509Certificate2 clientCertificate, X509Certificate2 serverCertificate)
        {
            _clientCertificate = clientCertificate;
            _serverCertificate = serverCertificate;
        }

        public Guid? GetSessionID(string userID, string vendor, string ipAddress)
        {
            AuthnRequestType auth = SSOTools.CreateAuthnRequest(userID, vendor, ipAddress);
            auth.Signature = SSOTools.Sign(auth, auth.ID, _clientCertificate);

            ChannelFactory<ISSOService> channelFactory = new ChannelFactory<ISSOService>("BasicHttpBinding_ISSOService");
            ISSOService ssoClient = channelFactory.CreateChannel();
            ResponseType r = ssoClient.GetSSO(new GetSSORequest { AuthnRequest = auth }).Response;
            channelFactory.Close();

            if (SSOTools.VerifySignature(r, _serverCertificate))
            {
                Dictionary<string, string> attributes = SSOTools.GetAttributes(r);
                Guid cleverDomeSessionGuid = new Guid(attributes["SessionID"]);
                //int timeOut = int.Parse(attributes["SessionTimeOut"]); // Time Out of the session.
                return cleverDomeSessionGuid;
            }
            else
            {
                return null;
            }
        }
    }
}
