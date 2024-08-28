using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace ClientePisee.Domain
{
    public static class Srcei
    {
        private static readonly string certificate = ConfigurationManager.AppSettings["SRCEIPFXCertificatePath"];
        private static readonly string certificatePass = ConfigurationManager.AppSettings["SRCEIPFXCertificatePassword"];
        private static readonly string username = ConfigurationManager.AppSettings["SRCEIUsername"];
        private static readonly string password = ConfigurationManager.AppSettings["SRCEIPassword"];
        private static readonly bool signed = true;
        private static readonly X509Certificate2 combinedCertificate = new X509Certificate2(certificate, certificatePass);
        private static readonly RSACryptoServiceProvider RSA = (RSACryptoServiceProvider)combinedCertificate.PrivateKey;

        public static string getCertificadoNac(string run, string dv)
        {
            string url = ConfigurationManager.AppSettings["URLSRCEICertificadoNac"];
            string idSobre = Pisee.GetIdSobre("0001");
            string fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string plainTextData = "";

            DataStruct.SoapMessage soapMessage = new DataStruct.SoapMessage();
            DataStruct.ServiceDataLog serviceData = new DataStruct.ServiceDataLog
            {
                idSobre = idSobre,
                fecha = DateTime.Parse(fecha).ToString("yyyyMMdd HH:mm:ss.fff")
            };

            string encabezado = @"<encabezado><idSobre>" + idSobre + "</idSobre><fechaHora>" + fecha + "</fechaHora>" +
               "<proveedor><nombre>SRCeI</nombre><servicios><servicio>CERTIFICADO DE NACIMIENTO</servicio><respuestaServicio><estado>SI</estado><glosa>RESPUESTA OK</glosa></respuestaServicio></servicios></proveedor>" +
               @"<consumidor><nombre>CONADI</nombre><tramite>REGISTRO DE USUARIOS</tramite><certificado><ds:X509Data xmlns:ds=""http://www.w3.org/2000/09/xmldsig#""><ds:X509IssuerSerial><ds:X509IssuerName/><ds:X509SerialNumber>0</ds:X509SerialNumber></ds:X509IssuerSerial></ds:X509Data></certificado>" +
            "</consumidor><emisor>CONADI</emisor><metadataOperacional><estadoSobre>00</estadoSobre><glosaSobre>Transacción Exitosa</glosaSobre></metadataOperacional></encabezado>";

            string documento = "<documento><entradaSRCel><run><numero>" + run + "</numero><dv>" + dv + "</dv></run></entradaSRCel></documento>";
            soapMessage = Pisee.SendRequest(url, username, password, documento, encabezado, signed, certificate, certificatePass);

            while(soapMessage.response == null || !soapMessage.response.HasChildNodes)
            {
                Thread.Sleep(200);
                soapMessage = Pisee.SendRequest(url, username, password, documento, encabezado, signed, certificate, certificatePass);
            }

            serviceData.request = soapMessage.request.OuterXml;
            serviceData.response = soapMessage.response.OuterXml;

            XmlNode rsaCypherDataNode;
            XmlNode tripleDesCypherDataNode;
            var nsmgr = new XmlNamespaceManager(soapMessage.response.NameTable);
            nsmgr.AddNamespace("enc", "http://www.w3.org/2001/04/xmlenc#");
            nsmgr.AddNamespace("SOAP-ENV", "http://schemas.xmlsoap.org/soap/envelope/");

            XmlDocument sobre = new XmlDocument();

            try
            {
                rsaCypherDataNode = soapMessage.response.SelectSingleNode("//enc:EncryptedKey/enc:CipherData/enc:CipherValue", nsmgr);
                tripleDesCypherDataNode = soapMessage.response.SelectSingleNode("//enc:EncryptedData/enc:CipherData/enc:CipherValue", nsmgr);
                string tripleDesKey = EnCrypt.RSADecrypt(rsaCypherDataNode.InnerText, RSA, false);
                plainTextData = EnCrypt.TripleDesDecrypt(tripleDesCypherDataNode.InnerText, tripleDesKey, false);

            }
            catch (Exception ex)
            {
                Log.TextLog("ERROR - Srcei - getCertificadoNac soapMessage Response: " + soapMessage.response.OuterXml, ex.ToString());
            }

            sobre.LoadXml(soapMessage.response.SelectSingleNode("//SOAP-ENV:Body/*[1]", nsmgr).OuterXml);
            XElement xmlDocumentWithoutNs = XmlOperation.RemoveAllNamespaces(XElement.Parse(sobre.OuterXml));
            sobre.LoadXml(xmlDocumentWithoutNs.ToString());

            serviceData.proveedorNombre = sobre.SelectSingleNode("//proveedor/nombre", nsmgr)?.InnerText.Trim() ?? string.Empty;
            serviceData.proveedorServicio = sobre.SelectSingleNode("//proveedor/servicios/servicio")?.InnerText ?? string.Empty;
            serviceData.respuestaServicioEstado = sobre.SelectSingleNode("//respuestaServicio/estado")?.InnerText.Trim() ?? string.Empty;
            serviceData.respuestaServicioGlosa = sobre.SelectSingleNode("//respuestaServicio/glosa")?.InnerText.Trim() ?? string.Empty;
            serviceData.meOpEstadoSobre = sobre.SelectSingleNode("//metadataOperacional/estadoSobre")?.InnerText.Trim() ?? string.Empty;
            serviceData.meOpGlosaSobre = sobre.SelectSingleNode("//metadataOperacional/glosaSobre")?.InnerText.Trim() ?? string.Empty;
            serviceData.cuerpoDocumento = plainTextData.Replace("@", "Ñ").Replace("'", "");

            Log.ServiceDataLog(serviceData);

            return serviceData.cuerpoDocumento;
        }



        public static string GetCertificadoDisc(string run, string dv)
        {
            string url = ConfigurationManager.AppSettings["URLSRCEICertificadoDisc"];
            string idSobre = Pisee.GetIdSobre("0002");
            string fecha = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string plainTextData = "";

            DataStruct.SoapMessage soapMessage = new DataStruct.SoapMessage();
            DataStruct.ServiceDataLog serviceData = new DataStruct.ServiceDataLog
            {
                idSobre = idSobre,
                fecha = DateTime.Parse(fecha).ToString("yyyyMMdd HH:mm:ss.fff")
            };

            string encabezado = @"<encabezado><idSobre>" + idSobre + "</idSobre><fechaHora>" + fecha + "</fechaHora>" +
                           "<proveedor><nombre>SRCeI</nombre><servicios><servicio>INFORMACION PERSONAL AVANZADO DISCAPACIDAD</servicio><respuestaServicio><estado>SI</estado><glosa>RESPUESTA OK</glosa></respuestaServicio></servicios></proveedor>" +
                           @"<consumidor><nombre>CONADI</nombre><tramite>POSTULACION</tramite><certificado><ds:X509Data xmlns:ds=""http://www.w3.org/2000/09/xmldsig#""><ds:X509IssuerSerial><ds:X509IssuerName/><ds:X509SerialNumber>0</ds:X509SerialNumber></ds:X509IssuerSerial></ds:X509Data></certificado>" +
                        "</consumidor><emisor>SRCEI</emisor><metadataOperacional><estadoSobre>00</estadoSobre><glosaSobre>Transacción Exitosa</glosaSobre></metadataOperacional></encabezado>";


            string documento = "<documento><entradaSRCel><run><numero>" + run + "</numero><dv>" + dv + "</dv></run></entradaSRCel></documento>";

            soapMessage = Pisee.SendRequest(url, username, password, documento, encabezado, signed, certificate, certificatePass);

            while (soapMessage.response == null || !soapMessage.response.HasChildNodes)
            {
                Thread.Sleep(200);
                soapMessage = Pisee.SendRequest(url, username, password, documento, encabezado, signed, certificate, certificatePass);
            }

            serviceData.request = soapMessage.request.OuterXml;
            serviceData.response = soapMessage.response.OuterXml;

            XmlNode rsaCypherDataNode;
            XmlNode tripleDesCypherDataNode;
            var nsmgr = new XmlNamespaceManager(soapMessage.response.NameTable);
            nsmgr.AddNamespace("enc", "http://www.w3.org/2001/04/xmlenc#");
            nsmgr.AddNamespace("SOAP-ENV", "http://schemas.xmlsoap.org/soap/envelope/");

            XmlDocument sobre = new XmlDocument();

            try
            {
                rsaCypherDataNode = soapMessage.response.SelectSingleNode("//enc:EncryptedKey/enc:CipherData/enc:CipherValue", nsmgr);
                tripleDesCypherDataNode = soapMessage.response.SelectSingleNode("//enc:EncryptedData/enc:CipherData/enc:CipherValue", nsmgr);
                string tripleDesKey = EnCrypt.RSADecrypt(rsaCypherDataNode.InnerText, RSA,true);
                plainTextData = EnCrypt.TripleDesDecrypt(tripleDesCypherDataNode.InnerText, tripleDesKey, false);

            }
            catch (Exception ex)
            {
                Log.TextLog("ERROR - Srcei - getCertificadoDisc soapMessage Response :" + soapMessage.response.OuterXml, ex.ToString());
            }

            sobre.LoadXml(soapMessage.response.SelectSingleNode("//SOAP-ENV:Body/*[1]", nsmgr).OuterXml);
            XElement xmlDocumentWithoutNs = XmlOperation.RemoveAllNamespaces(XElement.Parse(sobre.OuterXml));
            sobre.LoadXml(xmlDocumentWithoutNs.ToString());

            serviceData.proveedorNombre = sobre.SelectSingleNode("//proveedor/nombre", nsmgr)?.InnerText.Trim() ?? string.Empty;
            serviceData.proveedorServicio = sobre.SelectSingleNode("//proveedor/servicios/servicio")?.InnerText ?? string.Empty;
            serviceData.respuestaServicioEstado = sobre.SelectSingleNode("//respuestaServicio/estado")?.InnerText.Trim() ?? string.Empty;
            serviceData.respuestaServicioGlosa = sobre.SelectSingleNode("//respuestaServicio/glosa")?.InnerText.Trim() ?? string.Empty;
            serviceData.meOpEstadoSobre = sobre.SelectSingleNode("//metadataOperacional/estadoSobre")?.InnerText.Trim() ?? string.Empty;
            serviceData.meOpGlosaSobre = sobre.SelectSingleNode("//metadataOperacional/glosaSobre")?.InnerText.Trim() ?? string.Empty;
            serviceData.cuerpoDocumento = plainTextData.Replace("@", "Ñ").Replace("'", "");

            Log.ServiceDataLog(serviceData);


            return serviceData.cuerpoDocumento;
        }
    }
}
