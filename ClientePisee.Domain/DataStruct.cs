using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using static ClientePisee.Domain.DataStruct;

namespace ClientePisee.Domain
{
    public class DataStruct
    {
        public struct ServiceDataLog
        {
            public string idSobre;
            public string fecha;
            public string proveedorNombre;
            public string proveedorServicio;
            public string request;
            public string response;
            public string respuestaServicioEstado;
            public string respuestaServicioGlosa;
            public string meOpEstadoSobre;
            public string meOpGlosaSobre;
            public string cuerpoDocumento;
        }

        public struct SoapMessage
        {
            public XmlDocument request;
            public XmlDocument response;
        }

    }
}
