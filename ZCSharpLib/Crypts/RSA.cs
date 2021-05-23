using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace ZCSharpLib.Crypts
{
    /// <summary>
    /// RSA加密算法
    /// </summary>
    public class RSA
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static RSACryptoServiceProvider GetRSACrypto(string privateKey)
        {
            CspParameters csp = new CspParameters();

            csp.Flags = CspProviderFlags.UseMachineKeyStore;

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(csp);

            rsa.FromXmlString(privateKey);

            return rsa;
        }

        public static string RsaEncrypt(string privateKey, string src)
        {
            RSACryptoServiceProvider rsa = GetRSACrypto(privateKey);
            return RsaEncrypt(rsa, src);
        }

        public static string RsaEncrypt(RSACryptoServiceProvider rsa, string src)
        {
            byte[] rsaData = Encoding.UTF8.GetBytes(src);
            byte[] destData = rsa.Encrypt(rsaData, false);
            return Convert.ToBase64String(destData);
        }

        /// <summary>
        ///  用RSA解密基于Base64编码的字符串，返回UTF-8编码的字符串
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="src"></param>
        /// <returns></returns>
        public static string RsaDecrypt(string privateKey, string src)
        {
            CspParameters csp = new CspParameters();
            csp.Flags = CspProviderFlags.UseMachineKeyStore;
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(csp);
            rsa.FromXmlString(privateKey);
            return RsaDecrypt(rsa, src);
        }
        public static string RsaDecrypt(RSACryptoServiceProvider rsa, string src)
        {
            byte[] srcData = Convert.FromBase64String(src);
            byte[] destData = rsa.Decrypt(srcData, false);
            return Encoding.UTF8.GetString(destData);
        }

        public static byte[] RsaDecryt2(RSACryptoServiceProvider rsa, string src)
        {
            byte[] srcData = Convert.FromBase64String(src);
            return rsa.Decrypt(srcData, false);
        }
    }
}