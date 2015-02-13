using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Configuration;
using System.Security.Cryptography;

namespace CleverDomeCommon.SSO
{
    public class SamlSignedXml : SignedXml
    {
        private string _referenceAttributeId = "";
        public SamlSignedXml(XmlDocument document, string referenceAttributeId)
            : base(document)
        {
            _referenceAttributeId = referenceAttributeId;
        }
        public override XmlElement GetIdElement(
            XmlDocument document, string idValue)
        {
            return (XmlElement)
                document.SelectSingleNode(
                    string.Format("//*[@{0}='{1}']",
                    _referenceAttributeId, idValue));

        }

    }

    public class SSOTools
    {
        public static string SamlVersion = "2.0";
        public static Dictionary<string, string> namespaces = new Dictionary<string, string>();
        static SSOTools()
        {
            namespaces.Add("samlp", "urn:oasis:names:tc:SAML:2.0:protocol");
            namespaces.Add("saml", "urn:oasis:names:tc:SAML:2.0:assertion");
            namespaces.Add("xenc", "http://www.w3.org/2001/04/xmlenc#");
            //namespaces.Add("ds", "http://www.w3.org/2000/09/xmldsig#");
        }


        public static XmlSerializerNamespaces GetSamlNamespaces()
        {
            XmlSerializerNamespaces xsn = new XmlSerializerNamespaces();
            foreach (var key in namespaces.Keys)
                xsn.Add(key, namespaces[key]);

            return xsn;
        }

        public static XmlNamespaceManager GetSamlNamespaceMgr()
        {
            XmlNameTable nt = new NameTable();
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(nt);
            foreach (var key in namespaces.Keys)
                nsmgr.AddNamespace(key, namespaces[key]);

            return nsmgr;
        }

        public static AuthnRequestType CreateAuthnRequest(string assertionSubject, string vendor, string ipAddress)
        {
            return new AuthnRequestType
            {
                ID = "_" + Guid.NewGuid().ToString(),
                Version = SamlVersion,
                IssueInstant = DateTime.UtcNow,
                ProtocolBinding = "urn:oasis:names:tc:SAML:2.0:bindings:SOAP",
                ProviderName = vendor, // a parameter
                IsPassive = false,
                IsPassiveSpecified = true,
                // AssertionConsumerServiceURL = "responseurl", // a parameter, i assume we don't need this
                Issuer = new NameIDType
                {
                    Value = vendor, // a parameter
                    Format = "urn:oasis:names:tc:SAML:2.0:nameidformat:transient"
                },
                NameIDPolicy = new NameIDPolicyType
                {
                    AllowCreate = true,
                    AllowCreateSpecified = true
                },
                Subject = new SubjectType
                {
                    Items = new List<object>()
					{
						new NameIDType 
						{
							Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:transient",
							Value = assertionSubject
						},
                        new NameIDType
                        {
                            Format = "urn:oasis:names:tc:SAML:2.0:nameid-format:transient",
							Value = ipAddress,
                            NameQualifier = "IPAddress"
                        }
					}
                }
            };
        }

        public static ResponseType CreateResponse(string vendor, string assertionSubject)
        {
            string Issuer = "unitedplanners.us";
            return new ResponseType
            {
                ID = "_" + Guid.NewGuid().ToString(),
                Version = SamlVersion,
                IssueInstant = DateTime.UtcNow,
                Issuer = new NameIDType
                {
                    Value = Issuer
                },
                Status = new StatusType
                {
                    StatusCode = new StatusCodeType
                    {
                        Value = "urn:oasis:names:tc:SAML:2.0:status:Success"
                    }
                },
                Items = new List<object>()
					{ 
						new AssertionType
						{
							ID = "_" + Guid.NewGuid().ToString(),
							Issuer = new NameIDType
							{
								Value = Issuer
							},
							Version = SamlVersion,
							IssueInstant = DateTime.UtcNow,
							Conditions = new ConditionsType
							{
								NotBefore = DateTime.UtcNow,
								NotBeforeSpecified = true,
								NotOnOrAfter = DateTime.UtcNow.AddMinutes(5),
								NotOnOrAfterSpecified = true,
								Items = new List<ConditionAbstractType>
								{
									new AudienceRestrictionType 
									{
										Audience = new List<string>() { vendor } // parameter
									}
								}
							},
							Items = new List<StatementAbstractType>()
							{
								new AuthnStatementType
								{
									AuthnInstant = DateTime.UtcNow,
									AuthnContext = new AuthnContextType 
									{
										ItemsElementName = new ItemsChoiceType5[] { ItemsChoiceType5.AuthnContextClassRef },
										Items = new object[] { "AuthnContextClassRef" }
									}
								}
							},
							Subject = new SubjectType
							{
								Items = new List<object>()
								{
									new NameIDType 
									{
										NameQualifier = vendor, 
										Value = assertionSubject
									},
									new SubjectConfirmationType
									{
										Method = "urn:oasis:names:tc:SAML:2.0:cm:bearer", // double check it!!!
										SubjectConfirmationData = new SubjectConfirmationDataType()
									}
								}
							}
						}
					}
            };
        }

        public static Dictionary<string, string> GetAttributes(ResponseType rsp)
        {
            Dictionary<string, string> attributes = new Dictionary<string, string>();

            foreach (var item in rsp.Items)
            {
                AssertionType at = item as AssertionType;
                if (at != null)
                {

                    foreach (object subj in at.Subject.Items)
                    {
                        if (subj is NameIDType)
                        {
                            attributes.Add("Username", ((NameIDType)subj).Value);
                            break;
                        }
                    }

                    foreach (StatementAbstractType ast in at.Items)
                    {
                        if (ast is AttributeStatementType)
                        {
                            AttributeStatementType AttributeStatement = (AttributeStatementType)ast;

                            if (AttributeStatement != null && AttributeStatement.Items != null)
                            {
                                //get all attributes
                                foreach (object attributeItem in AttributeStatement.Items)
                                {
                                    if (attributeItem is AttributeType)
                                    {
                                        AttributeType Attribute = (AttributeType)attributeItem;
                                        if (!attributes.ContainsKey(Attribute.Name))
                                        {
                                            attributes.Add(Attribute.Name, Attribute.AttributeValue[0].ToString());
                                        }
                                    }
                                }
                            }
                        }
                    }

                    break;
                }
            }

            return attributes;
        }


        public static void SetAttributes(Dictionary<string, string> attributes, ResponseType rsp)
        {
            AttributeStatementType attrStatement = new AttributeStatementType
            {
                Items = new List<object>()
            };

            if (attributes != null)
            {
                foreach (KeyValuePair<string, string> attribute in attributes)
                {
                    attrStatement.Items.Add(new AttributeType
                    {
                        Name = attribute.Key,
                        NameFormat = "urn:oasis:names:tc:SAML:2.0:attrname-format:basic",
                        AttributeValue = new List<object> { attribute.Value }
                    });
                }
            }

            if (attrStatement.Items.Count > 0)
            {
                foreach (var item in rsp.Items)
                {
                    AssertionType at = item as AssertionType;
                    if (at != null)
                    {
                        at.Items.Add(attrStatement);
                        break;
                    }
                }
            }

        }

        public static X509Certificate2 GetCertificate(string path, string pwd)
        {
            X509Certificate2 cert = null;
            if (System.IO.File.Exists(path))
            {
                cert = new X509Certificate2(path, pwd);
            }
            return cert;
        }

        public static X509Certificate2 GetCertificate(StoreLocation storeLocation, StoreName storeName, X509FindType findType, object findValue)
        {
            X509Certificate2 certificate = null;

            X509Store store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadWrite);

            X509Certificate2Collection coll = store.Certificates.Find(findType, findValue, false);

            if (coll.Count < 1)
            {
                throw new ArgumentException("Unable to locate certificate");
            }

            certificate = coll[0];
            store.Close();

            return certificate;
        }

        public static XmlElement SignDoc(XmlDocument doc, X509Certificate2 cert2, string referenceId, string referenceValue)
        {
            SamlSignedXml sig = new SamlSignedXml(doc, referenceId);
            // Add the key to the SignedXml xmlDocument. 
            sig.SigningKey = cert2.PrivateKey;

            // Create a reference to be signed. 
            Reference reference = new Reference();

            reference.Uri = String.Empty;
            reference.Uri = "#" + referenceValue;

            // Add an enveloped transformation to the reference. 
            XmlDsigEnvelopedSignatureTransform env = new
                XmlDsigEnvelopedSignatureTransform();
            XmlDsigC14NTransform env2 = new XmlDsigC14NTransform();

            reference.AddTransform(env);
            reference.AddTransform(env2);

            // Add the reference to the SignedXml object. 
            sig.AddReference(reference);

            // Add an RSAKeyValue KeyInfo (optional; helps recipient find key to validate). 
            KeyInfo keyInfo = new KeyInfo();
            KeyInfoX509Data keyData = new KeyInfoX509Data(cert2);

            keyInfo.AddClause(keyData);

            sig.KeyInfo = keyInfo;

            // Compute the signature. 
            sig.ComputeSignature();

            // Get the XML representation of the signature and save it to an XmlElement object. 
            XmlElement xmlDigitalSignature = sig.GetXml();
            //xmlDigitalSignature.Prefix = "ds";

            return xmlDigitalSignature;
        }

        public static SignatureType Sign(Object o, string ID, X509Certificate2 cert)
        {
            XmlSerializerNamespaces samlNamespaces = GetSamlNamespaces();
            string assertionXml = OXmlSerializer.Serialize(o, true, false, samlNamespaces, new Type[] { });

            XmlDocument assertionXmlDoc = new XmlDocument();
            assertionXmlDoc.PreserveWhitespace = true;
            assertionXmlDoc.LoadXml(assertionXml);
            XmlElement signature = SignDoc(assertionXmlDoc, cert, "ID", ID);


            XmlAttributes myXmlAttributes = new XmlAttributes();

            // Create an XmlRootAttribute and set its element name and namespace.
            XmlRootAttribute myXmlRootAttribute = new XmlRootAttribute();
            myXmlRootAttribute.ElementName = "Signature";
            myXmlRootAttribute.Namespace = "http://www.w3.org/2000/09/xmldsig#";

            // Set the XmlRoot property to the XmlRoot object.
            myXmlAttributes.XmlRoot = myXmlRootAttribute;
            XmlAttributeOverrides myXmlAttributeOverrides =
                                          new XmlAttributeOverrides();

            /* Add the XmlAttributes object to the 
            XmlAttributeOverrides object. */
            myXmlAttributeOverrides.Add(typeof(SignatureType), myXmlAttributes);


            XmlSerializer xmlSerializer = new XmlSerializer(typeof(SignatureType), myXmlAttributeOverrides);
            //set xml writer settings
            XmlWriterSettings writerSettings = new XmlWriterSettings();
            writerSettings.OmitXmlDeclaration = true;
            writerSettings.Encoding = Encoding.UTF8;
            StringReader sr = new StringReader(signature.OuterXml);

            return (SignatureType)xmlSerializer.Deserialize(XmlReader.Create(sr));

        }

        public static bool VerifySignature(Object o, X509Certificate2 cert)
        {
            XmlSerializerNamespaces samlNamespaces = GetSamlNamespaces();
            string assertionXml = OXmlSerializer.Serialize(o, true, false, samlNamespaces, new Type[] { });

            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            doc.LoadXml(assertionXml);

            // Create a new SignedXml object and pass it the xml doc
            SignedXml signedXml = new SignedXml(doc);
		    string path = ConfigurationManager.AppSettings["CleverDomeCertPath"];
            // Get signature
            var signs = doc.GetElementsByTagName("Signature");
            if (signs.Count != 1)
            {
                return false;
            }
            XmlElement signatureNode = (XmlElement)signs[0];
			//X509Certificate2 asd = GetCleverDomeCert();
			X509Certificate2 asd = null;
			if (System.IO.File.Exists(path))
			{
				asd = new X509Certificate2(path);
			}
		    
			signedXml.LoadXml(signatureNode);
			var xcv = signedXml.CheckSignature(asd, true);
            //return signatureNode != null;
			return xcv;
			//return signedXml.CheckSignature(Key.FromXmlString());
        }

		public static X509Certificate2 GetCleverDomeCert()
		{
			var _cleverDomeCertKey = "CleverDomeCert";

			try
			{
				return GetCert(ConfigurationManager.AppSettings[_cleverDomeCertKey]);
			}
			catch (Exception e)
			{
				throw new Exception("Error retrieving cleverDome cert", e);
			}
		}

		private static X509Certificate2 GetCert(string thumbprint)
		{
			X509Certificate2 certificate = null;

			X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
			store.Open(OpenFlags.ReadOnly);

			X509Certificate2Collection coll = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);

			if (coll.Count < 1)
			{
				throw new ArgumentException("Unable to locate certificate with thumbprint = " + thumbprint);
			}

			certificate = coll[0];
			store.Close();

			return certificate;
		}
    }
}
