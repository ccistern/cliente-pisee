using ClientePisee.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;



namespace ClientePisee.Web
{
    public partial class Servicios : System.Web.UI.Page
    {

        [WebMethod(EnableSession = true)]
        public static string CertificadoNacimiento(string run, string dv)
        {
            if ( !InputValidation.IsRutValid(run + "-" + dv) )
            {
                Log.TextLog("ERROR: Servicios.aspx - CertificadoNacimiento : Rut invalido : " + run + "-" + dv, "");
                return "";
            }
            else
            {
                string response = Srcei.getCertificadoNac(run, dv);
                return response;
            }
        }


        [WebMethod(EnableSession = true)]
        public static string CertificadoDiscapacidad(string run, string dv)
        {

            if ( !InputValidation.IsRutValid(run + "-" + dv) )
            {
                Log.TextLog("ERROR: Servicios.aspx - CertificadoDiscapacidad : Rut invalido : " + run + "-" + dv, "");
                return "";
            }
            else
            {
                string response = Srcei.GetCertificadoDisc(run, dv);
                return response;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}