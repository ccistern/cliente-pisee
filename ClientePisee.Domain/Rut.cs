using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ClientePisee.Domain
{
    public static class Rut
    {

        /// <summary>
        /// Metodo de validación de rut con digito verificador
        /// dentro de la cadena
        /// </summary>
        /// <param name="rut">string</param>
        /// <returns>booleano</returns>
        public static bool ValidaRut(string rut)
        {
            rut = rut.Replace(".", "").ToUpper();
            rut = rut.Replace("-", "").ToUpper();
            Regex expresion = new System.Text.RegularExpressions.Regex("^([0-9]+[0-9K])$");
            string dv = rut.Substring(rut.Length - 1, 1);
            int mantisa;
            Int32.TryParse(rut.Substring(0, rut.Length - 1), out mantisa);

            if ( !expresion.IsMatch(rut) )
            {
                return false;
            }


            if ( dv != Digito(mantisa) )
            {
                return false;
            }
            return true;
        }




        /// <summary>
        /// método que calcula el digito verificador a partir
        /// de la mantisa del rut
        /// </summary>
        /// <param name="rut"></param>
        /// <returns></returns>
        public static string Digito(int rut)
        {
            int suma = 0;
            int multiplicador = 1;
            while ( rut != 0 )
            {
                multiplicador++;
                if ( multiplicador == 8 )
                    multiplicador = 2;
                suma += (rut % 10) * multiplicador;
                rut = rut / 10;
            }
            suma = 11 - (suma % 11);
            if ( suma == 11 )
            {
                return "0";
            }
            else if ( suma == 10 )
            {
                return "K";
            }
            else
            {
                return suma.ToString();
            }
        }

    }

}
