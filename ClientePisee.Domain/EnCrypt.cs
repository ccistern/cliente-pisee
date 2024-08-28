using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;

namespace ClientePisee.Domain
{
    public static class EnCrypt
    {
        static public string RSADecrypt(string CypherString, RSACryptoServiceProvider RSA, bool DoOAEPPadding)
        {
            byte[] DataToDecrypt = System.Convert.FromBase64String(CypherString);

            try
            {
                byte[] decryptedData;
                decryptedData = RSA.Decrypt(DataToDecrypt, DoOAEPPadding);

                return Convert.ToBase64String(decryptedData);
            }
            catch ( CryptographicException e )
            {
                Console.WriteLine(e.ToString());

                return "";
            }
        }

        public static string TripleDesDecrypt(string cipherString, string keyString, bool useHashing)
        {
            byte[] MyDecryptArray = Convert.FromBase64String(cipherString);
            byte[] b = Convert.FromBase64String(cipherString);
            TripleDES des = new TripleDESCryptoServiceProvider();

            des.Key = Convert.FromBase64String(keyString);
            des.Mode = CipherMode.CBC;
     
            des.IV = new byte[8];

            ICryptoTransform ct = des.CreateDecryptor();
            byte[] output = ct.TransformFinalBlock(b, 0, b.Length);

            return Encoding.UTF8.GetString(output.Skip(8).ToArray());

        }
    }
}

