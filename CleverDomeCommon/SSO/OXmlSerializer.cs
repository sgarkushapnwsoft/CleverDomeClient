using System;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace CleverDomeCommon.SSO
{
    public class OXmlSerializer
    {
        /// <summary>
        /// Serializes this object including the entire heirarchy objects to xml.
        /// </summary>
        /// <param name="serializableObject">object to serialize to xml</param>
        /// <param name="omitDeclaration">true to remove xml declaration</param>
        /// <param name="omitNamespaces">true to remove all default namespaces</param>
        /// <param name="namespaces">namespaces to use for serializer</param>
        /// <returns>xml string</returns>
        public static string Serialize(object serializableObject, bool omitDeclaration, bool omitNamespaces, XmlSerializerNamespaces namespaces, Type[] derivedTypes)
        {
            string objectXml = string.Empty;

            try
            {
                //create xml serializer
                XmlSerializer xmlSerializer = new XmlSerializer(serializableObject.GetType(), derivedTypes);
                //set xml writer settings
                XmlWriterSettings writerSettings = new XmlWriterSettings();
                writerSettings.OmitXmlDeclaration = omitDeclaration;
                writerSettings.Encoding = Encoding.UTF8;
                StringWriter stringWriter = new StringWriter();
                //create xml writer with settings
                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter, writerSettings))
                {
                    //check if we want to omit the xml declaration and namespaces
                    if (omitNamespaces || namespaces != null)
                    {
                        if (namespaces == null)
                        {
                            namespaces = new XmlSerializerNamespaces();
                            namespaces.Add(string.Empty, string.Empty);
                        }
                        xmlSerializer.Serialize(xmlWriter, serializableObject, namespaces);
                    }
                    else
                    {
                        xmlSerializer.Serialize(xmlWriter, serializableObject);
                    }
                    //set xml string
                    objectXml = stringWriter.ToString();
                }
            }
            catch (Exception ex)
            {

            }

            return objectXml;
        }

        /// <summary>
        /// Serializes this object including the entire heirarchy objects to xml.
        /// </summary>
        /// <param name="serializableObject">object to serialize to xml</param>
        /// <param name="omitDeclaration">true to remove xml declaration</param>
        /// <param name="omitNamespaces">true to remove all default namespaces</param>
        /// <returns>xml string</returns>
        public static string Serialize(object serializableObject, bool omitDeclaration, bool omitNamespaces)
        {
            return Serialize(serializableObject, omitDeclaration, omitNamespaces, null, null);
        }
    }
}
