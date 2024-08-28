using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ClientePisee.Domain
{
    public static class XmlSecurity
    {
        public static XmlElement SignXmlFile(string xml, string certificate, string password)
        {
            X509Certificate2 combinedCertificate = new X509Certificate2(certificate, password);
            RSACryptoServiceProvider rsa = (RSACryptoServiceProvider)combinedCertificate.PrivateKey;

            // Create a new XML document.
            XmlDocument xmlDoc = new XmlDocument();

            // Format the document to ignore white spaces.
            xmlDoc.PreserveWhitespace = false;

            // Load the passed XML file using it's name.
            xmlDoc.LoadXml(xml);

            // Create a SignedXml object.
            SignedXml signedXml = new SignedXml(xmlDoc);

            // Add the key to the SignedXml document. 
            signedXml.SigningKey = rsa;

            // Specify a canonicalization method.
            signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigExcC14NTransformUrl;

            // Set the InclusiveNamespacesPrefixList property.        
            XmlDsigExcC14NTransform canMethod = (XmlDsigExcC14NTransform)signedXml.SignedInfo.CanonicalizationMethodObject;
            canMethod.InclusiveNamespacesPrefixList = "soapenv";

            // Create a reference to be signed.
            Reference reference = new Reference();
            reference.Uri = "";

            // Add an enveloped transformation to the reference.
            XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
            reference.AddTransform(env);

            // Add the reference to the SignedXml object.
            signedXml.AddReference(reference);

            // Add an RSAKeyValue KeyInfo (optional; helps recipient find key to validate).
            KeyInfo keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoX509Data(combinedCertificate.Export(X509ContentType.Cert)));
            signedXml.KeyInfo = keyInfo;

            signedXml.ComputeSignature();

            // Get the XML representation of the signature and save
            // it to an XmlElement object.
            XmlElement xmlDigitalSignature = signedXml.GetXml();

            if ( xmlDoc.FirstChild is XmlDeclaration )
            {
                xmlDoc.RemoveChild(xmlDoc.FirstChild);
            }


            //foreach (XmlNode node in xmlDigitalSignature.SelectNodes(
            //    "descendant-or-self::*[namespace-uri()='http://www.w3.org/2000/09/xmldsig#']"))
            //{
            //    node.Prefix = "ds";
            //}
            

            return xmlDigitalSignature;
        }


        public static Boolean VerifyXmlFile(String xml)
        {
            XmlDocument xmlDocument = new XmlDocument();

            // Format using white spaces.
            xmlDocument.PreserveWhitespace = true;

            // Load the passed XML file into the document. 
            xmlDocument.Load(xml);

            // Create a new SignedXml object and pass it
            // the XML document class.
            SignedXml signedXml = new SignedXml(xmlDocument);

            // Find the "Signature" node and create a new
            // XmlNodeList object.
            XmlNodeList nodeList = xmlDocument.GetElementsByTagName("Signature");

            // Load the signature node.
            signedXml.LoadXml((XmlElement)nodeList[0]);

            // Check the signature and return the result.
            return signedXml.CheckSignature();
        }

    }
}
