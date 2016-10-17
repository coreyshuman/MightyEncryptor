using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileEncryption
{
    public class AES
    {
        const int KeySize = 256;
        const int BlockSize = 128;
        const int Iterations = 10000;

        public byte[] Encrypt(Stream file, string password)
        {
            byte[] dataOut;
            // Check arguments.
            if (file == null || !file.CanRead)
                throw new ArgumentNullException("File");

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = KeySize;
                aes.BlockSize = BlockSize;

                aes.GenerateIV();
                byte[] salt = new byte[aes.BlockSize / 8];
                Buffer.BlockCopy(aes.IV, 0, salt, 0, salt.Length);

                var key = new Rfc2898DeriveBytes(password, salt, Iterations);
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);

                aes.Mode = CipherMode.CBC;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream encryptStream = new MemoryStream())
                {
                    // write salt to beginning of output stream
                    encryptStream.Write(salt, 0, salt.Length);
                    // Create the streams used for encryption.
                    using (CryptoStream csEncrypt = new CryptoStream(encryptStream, encryptor, CryptoStreamMode.Write))
                    {
                        file.Position = 0;
                        file.CopyTo(csEncrypt);
                        csEncrypt.FlushFinalBlock();
                        
                    }
                    dataOut = encryptStream.ToArray();
                }
            }
            return dataOut;
        }

        public void Decrypt(Stream decryptStream, Stream cipherText, string password)
        {

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = KeySize;
                aes.BlockSize = BlockSize;

                byte[] salt = new byte[aes.BlockSize / 8];
                cipherText.Position = 0;
                cipherText.Read(salt, 0, salt.Length);

                var key = new Rfc2898DeriveBytes(password, salt, Iterations);
                aes.Key = key.GetBytes(aes.KeySize / 8);
                aes.IV = key.GetBytes(aes.BlockSize / 8);

                aes.Mode = CipherMode.CBC;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (CryptoStream csDecrypt = new CryptoStream(cipherText, decryptor, CryptoStreamMode.Read))
                {
                    var buffer = new byte[512];
                    var bytesRead = default(int);
                    while ((bytesRead = csDecrypt.Read(buffer, 0, buffer.Length)) > 0)
                        decryptStream.Write(buffer, 0, bytesRead);

                }
            }
        }
    }
}
