using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Core.Utilities
{

    // reference: https://stackoverflow.com/questions/10168240/encrypting-decrypting-a-string-in-c-sharp
    // reference: https://stackoverflow.com/questions/11743160/how-do-i-encode-and-decode-a-base64-string

    public static class Encryption
    {
        public static string Encrypt(string plainText, string passPhrase)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(plainText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(passPhrase, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    passPhrase = Convert.ToBase64String(ms.ToArray());
                }
            }
            return passPhrase;
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(passPhrase, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        public static string Encrypt(string plainText, string passPhrase, DateTime expiryDate)
        {
            var plainTextInBase64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
            var expiryBase64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(expiryDate.ToString()));

            var encryptedData = Encrypt($"{plainTextInBase64String}:{expiryBase64String}", passPhrase);
            return encryptedData;
        }

        public static string Decrypt(string cipherText, string passPhrase, DateTime currentDate)
        {
            const char separatorChar = ':';

            var decryptedData = Decrypt(cipherText, passPhrase);
            var plainText = Encoding.UTF8.GetString(Convert.FromBase64String(decryptedData.Split(separatorChar)[0]));
            var expiryDate = DateTime.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(decryptedData.Split(separatorChar)[1])));

            if (currentDate > expiryDate) throw new InvalidOperationException();

            return plainText;
        }
    }
}
