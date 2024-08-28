using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ClientePisee.Domain
{
    public static class InputValidation
    {

        public static bool IsGuid(string s)
        {

            if ( string.IsNullOrEmpty(s) )
                return false;
            else
            {
                var regex = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$");
                return regex.IsMatch(s);
            }
        }

        public static bool IsNumeric(string s)
        {
            if ( string.IsNullOrEmpty(s) )
                return false;
            else
            {
                var regex = new Regex(@"^[0-9]+$");
                return regex.IsMatch(s);
            }
        }

        public static bool IsLetters(string s)
        {
            if ( string.IsNullOrEmpty(s) )
                return false;
            else
            {
                var regex = new Regex(@"^[a-zA-Z]+$");
                return regex.IsMatch(s);
            }
        }

        public static bool IsAlfaNumeric(string s)
        {

            if ( string.IsNullOrEmpty(s) )
                return false;
            else
            {
                var regex = new Regex(@"^[a-zA-Z0-9]+$");
                return regex.IsMatch(s);
            }

        }
        public static bool IsB64(string s)
        {
            if ( string.IsNullOrEmpty(s) )
                return false;
            else
            {
                var regex = new Regex(@"^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$");
                return regex.IsMatch(s);
            }
        }

        public static bool IsValidEmailAddress(string s)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(s);
                return addr.Address == s;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsRutValid(string s)
        {
            if ( string.IsNullOrEmpty(s) )
                return false;
            else
            {

                return Rut.ValidaRut(s);
            }

        }


        public static bool IsGcaptchaValid(string s)
        {
            if ( string.IsNullOrEmpty(s) )
                return false;
            else
            {
                return true;
            }
        }

    }
}
