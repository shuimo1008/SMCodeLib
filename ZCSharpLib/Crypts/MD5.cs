using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace ZCSharpLib.Crypts
{
    public class MD5
    {
        public static string Encode(string encryptString)
        {
            byte[] result = Encoding.Default.GetBytes(encryptString);
            byte[] output = new MD5CryptoServiceProvider().ComputeHash(result);
            return BitConverter.ToString(output).Replace("-", "");
        }
    }
}