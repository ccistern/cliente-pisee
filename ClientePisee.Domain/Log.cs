using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using static ClientePisee.Domain.DataStruct;

namespace ClientePisee.Domain
{
    public static class Log
    {
        private static readonly string connectionString = ConfigurationManager.AppSettings["ConnectionString"];

        public static void ServiceDataLog(ServiceDataLog log)
        {

            string sql = "INSERT INTO conadiclientepisee.dbo.log (idsobre,fecha,proveedornombre,proveedorservicio,request,response,respuestaservicioestado,respuestaservicioglosa,meopestadosobre,meopglosasobre,cuerpodocumento) values('"
                + log.idSobre + "','"
                + log.fecha + "','"
                + log.proveedorNombre + "','"
                + log.proveedorServicio + "',"
                + "@request,@response,'"
                + log.respuestaServicioEstado + "','"
                + log.respuestaServicioGlosa + "','"
                + log.meOpEstadoSobre + "','"
                + log.meOpGlosaSobre + "','"
                + log.cuerpoDocumento + "')";

            SqlCommand cmd = new SqlCommand(sql);
            cmd.Parameters.AddWithValue("@request", log.request);
            cmd.Parameters.AddWithValue("@response", log.response);

            DataSource.ExecuteNonQuery(cmd, connectionString);

        }

        public static void TextLog(string Descripcion, string Exception)
        {
            string path = @"c:\temp\Log\ConadiClientePisee\" + DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-" + DateTime.Now.Day.ToString() + ".txt";

            if ( !Directory.Exists(path) )
            {
                try
                {
                    Directory.GetParent(path).Create();
                }
                catch ( Exception e )
                {
                    //Error("Error al Crear el Directorio de Log ", ex.ToString());
                }
            }
            try
            {
                StreamWriter sw = System.IO.File.AppendText(path);
                sw.WriteLine(DateTime.Now.ToString() + "-----------------------------------------------------------------------------------------------");
                sw.WriteLine(Descripcion + "//" + Exception);

                sw.Flush();
                sw.Close();
            }
            catch ( Exception  e)
            {
                //Error("Error al escribir en el archivo de Log: "+ path.ToString() + " ", ex.ToString());  
            }

        }
    }

}