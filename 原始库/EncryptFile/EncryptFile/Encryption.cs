using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace EncryptFile
{
    public class Encryption
    {
        //Declare a transformer of type Rijndael Managed  
        RijndaelManaged rj = new RijndaelManaged();
        //Create a file stream template to read and write to the file  
        FileStream fs;

        public Encryption(string key, string iv)
        {
            //Set the key  
            //If the key is not set then a key will be randomnly generated   
            //every time it is used  
            //With default settings, the key should be 32 bytes long  
            rj.Key = Encoding.UTF8.GetBytes(key);
            //Set the IV  
            //If the IV is not set then an IV will be randomnly generated   
            //every time it is used  
            //With default settings, the IV should be 16 bytes long  
            rj.IV = Encoding.UTF8.GetBytes(iv);
        }

        //Encrypts text and writes it to a file  
        //It takes two parameters--file, which is the file name to write the text to  
        //and text, which is the text to encrypt  
        public string EncryptFile(string file, string text)
        {
            //Open a filestream in Append mode and create a CryptoStream to encrypt the text  
            fs = new FileStream(file, FileMode.Append);
            CryptoStream cs = new CryptoStream(fs, rj.CreateEncryptor(rj.Key, rj.IV), CryptoStreamMode.Write);
            //use a streamwriter for easy writing  
            using (StreamWriter sw = new StreamWriter(cs))
            {
                try
                {
                    //write (encrypt) the text  
                    sw.Write(text);
                }
                catch (Exception ex)
                {
                    //something went wrong...  
                    return ("Unable to encrypt text; the error was " + ex.ToString());
                }
            }
            //close the cryptostream and filestream  
            cs.Close();
            fs.Close();
            return ("Text successfully encrypted in file " + file);
        }

        //Decrypts a file and displays it on screen  
        //It takes one parameter, file, which is the filename of the file to decrypt  
        public string DecryptFile(string file)
        {
            //variable that the text will be held in  
            string intxt = "";
            try
            {
                //try and open the file in Open mode  
                fs = new FileStream(file, FileMode.Open);
            }
            catch (Exception ex)
            {
                //something went wrong, usually because file doesn't exits  
                return ("Unable to open file..does it exist?");
            }
            //create a cryptostream to decrypt the text  
            CryptoStream cs = new CryptoStream(fs, rj.CreateDecryptor(rj.Key, rj.IV), CryptoStreamMode.Read);
            //use a streamreader for easy reading  
            using (StreamReader sr = new StreamReader(cs))
            {
                intxt = sr.ReadToEnd();
            }
            //close the cryptostream and filestream  
            cs.Close();
            fs.Close();
            //show the decrypted text  
            return ("The decrypted text is...\n" + intxt);
        }
    }
}
