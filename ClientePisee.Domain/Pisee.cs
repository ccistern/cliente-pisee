using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ClientePisee.Domain
{
    public static class Pisee
    {

        public static DataStruct.SoapMessage SendRequest(string url, string username, string password, string documento, string encabezado, bool signature, string certificate, string certificatepass)
        {
            XmlDocument xmlCuerpo = CreateSoapCuerpo(documento);
            XmlDocument xmlEncabezado = CreateSoapEncabezado(encabezado);
            XmlDocument xmlHeader = CreateSoapHeader();
            XmlDocument xmlBody = CreateSoapBody();
            XmlDocument xmlSobre = CreateSoapSobre();
            XmlDocument xmlEnvelope = CreateSoapEnvelope();
            XmlDocument xmlSignature = CreateSignatureElement();
            XmlElement SignatureElement;

            DataStruct.SoapMessage soapMessage = new DataStruct.SoapMessage();
            soapMessage.request = new XmlDocument();
            soapMessage.response = new XmlDocument();


            xmlSobre.DocumentElement.AppendChild(xmlSobre.ImportNode(xmlEncabezado.DocumentElement, true));
            xmlSobre.DocumentElement.AppendChild(xmlSobre.ImportNode(xmlCuerpo.DocumentElement, true));

            string xmldoc = XmlOperation.RemoveAllEmptyNameSpaces(xmlSobre.OuterXml);

            //string xmldoc = xmlSobre.OuterXml;


            xmlSobre.LoadXml(xmldoc);

            var nsmgr = new XmlNamespaceManager(xmlEnvelope.NameTable);
            nsmgr.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
            nsmgr.AddNamespace("wsse", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd");

            xmlBody.DocumentElement.AppendChild(xmlBody.ImportNode(xmlSobre.DocumentElement, true));
            xmlEnvelope.DocumentElement.AppendChild(xmlEnvelope.ImportNode(xmlHeader.DocumentElement, true));
            xmlEnvelope.DocumentElement.AppendChild(xmlEnvelope.ImportNode(xmlBody.DocumentElement, true));
            string wsse = @"<wsse:Security xmlns:wsse=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"" xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"" ></wsse:Security>";
            XmlDocument soapWSSE = new XmlDocument();
            soapWSSE.LoadXml(wsse);
            XmlNode parentNode = xmlEnvelope.SelectSingleNode("soapenv:Envelope/soapenv:Header", nsmgr);
            parentNode.InsertBefore(xmlEnvelope.ImportNode((XmlNode)soapWSSE.DocumentElement, true), parentNode.LastChild);

            if (signature)
            {

                SignatureElement = XmlSecurity.SignXmlFile(xmlEnvelope.OuterXml, certificate, certificatepass);

                parentNode = xmlEnvelope.SelectSingleNode("soapenv:Envelope/soapenv:Header/wsse:Security", nsmgr);
                parentNode.InsertBefore(xmlEnvelope.ImportNode((XmlNode)SignatureElement, true), parentNode.LastChild);

            }

            soapMessage.request = xmlEnvelope;


            HttpWebRequest webRequest = CreateWebRequest(url, "", username, password);

            try
            {
                using (Stream stm = webRequest.GetRequestStream())
                {
                    using (StreamWriter stmw = new StreamWriter(stm))
                    {
                        stmw.Write(xmlEnvelope.OuterXml);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.TextLog("Pisee - SendRequest - ERROR", ex.ToString());
            }


            IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);
            asyncResult.AsyncWaitHandle.WaitOne();


            string response;
            try
            {
                using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
                {
                    using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                    {
                        response = rd.ReadToEnd();
                    }
                }

                soapMessage.response.LoadXml(response);
            }
            catch (Exception ex)
            {
                Log.TextLog("Pisee - SendRequest - ERROR", ex.ToString());

            }

            return soapMessage;
        }

        private static HttpWebRequest CreateWebRequest(string url, string action, string username, string password)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Headers.Add("SOAPAction", action);
            webRequest.ContentType = "application/soap+xml";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            webRequest.SendChunked = true;

            if (username != "" && password != "")
            {
                String encoded = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));
                webRequest.Headers.Add("Authorization", "Basic " + encoded);
            }
            return webRequest;
        }

        private static XmlDocument CreateSignatureElement()
        {
            string signature = @"<Signature xmlns=""http://www.w3.org/2000/09/xmldsig#""></Signature>";

            XmlDocument soapSignature = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings { NameTable = new NameTable() };
            XmlNamespaceManager xmlns = new XmlNamespaceManager(settings.NameTable);
            xmlns.AddNamespace("", "http://www.w3.org/2000/09/xmldsig#");
            XmlParserContext context = new XmlParserContext(null, xmlns, "", XmlSpace.Default);
            var strReader = new StringReader(signature);
            XmlReader reader = XmlReader.Create(strReader, settings, context);
            soapSignature.Load(reader);

            return soapSignature;
        }

        private static XmlDocument CreateSoapBody()
        {
            //string body = @"<soapenv:Body xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""></soapenv:Body>";

            string body = @"<soapenv:Body xmlns:wsu=""http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""></soapenv:Body>";

            XmlDocument soapBody = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings { NameTable = new NameTable() };
            XmlNamespaceManager xmlns = new XmlNamespaceManager(settings.NameTable);
            xmlns.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
            XmlParserContext context = new XmlParserContext(null, xmlns, "", XmlSpace.Default);
            var strReader = new StringReader(body);
            XmlReader reader = XmlReader.Create(strReader, settings, context);
            soapBody.Load(reader);

            return soapBody;
        }

        private static XmlDocument CreateSoapHeader()
        {
            string header = @"<soapenv:Header></soapenv:Header>";

            XmlDocument soapHeader = new XmlDocument();
            XmlReaderSettings settings = new XmlReaderSettings { NameTable = new NameTable() };
            XmlNamespaceManager xmlns = new XmlNamespaceManager(settings.NameTable);
            xmlns.AddNamespace("soapenv", "http://schemas.xmlsoap.org/soap/envelope/");
            XmlParserContext context = new XmlParserContext(null, xmlns, "", XmlSpace.Default);
            var strReader = new StringReader(header);
            XmlReader reader = XmlReader.Create(strReader, settings, context);
            soapHeader.Load(reader);

            return soapHeader;
        }

        private static XmlDocument CreateSoapEnvelope()
        {
            string envelope = @"<soapenv:Envelope xmlns:soapenv = ""http://schemas.xmlsoap.org/soap/envelope/"" ></soapenv:Envelope>";
            XmlDocument soapEnvelopeDocument = new XmlDocument();
            soapEnvelopeDocument.LoadXml(envelope);
            return soapEnvelopeDocument;
        }

        private static XmlDocument CreateSoapSobre()
        {
            string body = @"<sobre xsi:schemaLocation=""http://valida.aem.gob.cl http://valida.aem.gob.cl/documentales/AEM/sobre.xsd http://valida.aem.gob.cl http://valida.aem.gob.cl/documentales/entradas/entradaSRCeI.xsd"" xmlns=""http://valida.aem.gob.cl""></sobre>";
            XmlDocument soapBodyDocument = new XmlDocument();
            XmlSchema schema = new XmlSchema();

            XmlReaderSettings settings = new XmlReaderSettings { NameTable = new NameTable() };
            XmlNamespaceManager xmlns = new XmlNamespaceManager(settings.NameTable);

            xmlns.AddNamespace("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            XmlParserContext context = new XmlParserContext(null, xmlns, "", XmlSpace.Default);
            var strReader = new StringReader(body);
            XmlReader reader = XmlReader.Create(strReader, settings, context);
            soapBodyDocument.Load(reader);


            return soapBodyDocument;
        }

        private static XmlDocument CreateSoapCuerpo(string documento)
        {
            string cuerpo = @"<cuerpo>" + documento + "</cuerpo>";
            XmlDocument soapCuerpoDocument = new XmlDocument();
            soapCuerpoDocument.LoadXml(cuerpo);
            return soapCuerpoDocument;
        }

        private static XmlDocument CreateSoapEncabezado(string encabezado)
        {
            XmlDocument soapEncabezadoDocument = new XmlDocument();
            soapEncabezadoDocument.LoadXml(encabezado);
            return soapEncabezadoDocument;
        }

        public static string GetIdSobre(string codigoTramite)
        {
            string codigoInstitucion = "210601";
            string fechaActual = DateTime.Now.ToString("yyyyMMdd");
            string numeroTransaccion = Pisee.GetNumeroTransaccion();
            string codigoSecuencial = "00";
            string idSobre = codigoInstitucion + codigoTramite + fechaActual + numeroTransaccion + codigoSecuencial;

            return idSobre;
        }

        private static string GetNumeroTransaccion()
        {
            string connectionString = ConfigurationManager.AppSettings["ConnectionString"];
            string sql = "select * from conadiclientepisee.dbo.transaccion";
            int transaccion;
            string fechainicio = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string fechareset = DateTime.Now.ToString("yyyy-MM-dd 23:59:59.000");

            DataTable numeroTable = DataSource.ExecuteReader(sql, connectionString);


            if (numeroTable.Rows.Count > 0)
            {
                DateTime fechaResetDb = numeroTable.Rows[0].Field<DateTime>("fechareset");

                if (DateTime.Now > fechaResetDb)
                {
                    transaccion = 1;
                    Log.TextLog("Pisee - getNumeroTransaccion - Numero Transaccion Updated/Reset : 1", "");
                }
                else
                {
                    transaccion = numeroTable.Rows[0].Field<int>("numero") + 1;
                }

                sql = "update conadiclientepisee.dbo.transaccion set " +
                       "numero ='" + transaccion + "'," +
                       "fechainicio = convert(datetime, '" + fechainicio + "', 121) ," +
                       "fechareset = convert(datetime, '" + fechareset + "', 121) ";
            }
            else
            {
                transaccion = 1;
                sql = "insert into conadiclientepisee.dbo.transaccion (numero,fechainicio,fechareset) values("
                 + "'" + transaccion + "',"
                 + "convert(datetime, '" + fechainicio + "', 121),"
                 + "convert(datetime, '" + fechareset + "', 121) )";

                Log.TextLog("Pisee - getNumeroTransaccion - Numero Transaccion Insertado : 1", "");
            }

            DataSource.ExecuteNonQuery(sql, connectionString);

            return transaccion.ToString().PadLeft(7, '0');
        }
    }
}
