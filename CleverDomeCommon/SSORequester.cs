using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using CleverDomeCommon.SSO;
using System.ServiceModel;

namespace CleverDomeCommon
{
    public class SSORequester
    {
        public static Guid? GetSessionID(string userID, X509Certificate2 cert, string vendor, string ipAddress)
        {
            AuthnRequestType auth = SSOTools.CreateAuthnRequest(userID, vendor, ipAddress);
            auth.Signature = SSOTools.Sign(auth, auth.ID, cert);

            ChannelFactory<ISSOService> channelFactory = new ChannelFactory<ISSOService>("BasicHttpBinding_ISSOService");
            ISSOService ssoClient = channelFactory.CreateChannel();
            ResponseType r = ssoClient.GetSSO(new GetSSORequest { AuthnRequest = auth }).Response;

            channelFactory.Close();
            if (SSOTools.VerifySignature(r, cert))
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
