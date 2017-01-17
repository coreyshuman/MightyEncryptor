using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileEncryption
{
    /// <summary>
    /// Static methods for AES encryption and decryption of file streams.
    /// </summary>
    public static class AES
    {
        const int KeySize = 256;
        const int BlockSize = 128;
        const int Iterations = 10000;

        /// <summary>
        /// Take a file stream and encrypt using AES and the given password.
        /// Salt and verification code are appended to beginning of output byte array.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="password"></param>
        /// <returns>Returns encypted byte array.</returns>
        public static byte[] Encrypt(Stream file, string password)
        {
            byte[] dataOut;
            // Check arguments.
            if (file == null || !file.CanRead)
                throw new ArgumentNullException("File");

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
                    // write two byte short verification code
                    encryptStream.Write(key.GetBytes(2), 0, 2);
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

        /// <summary>
        /// Takes an AES-encrypted file stream and decodes using the given password.
        /// </summary>
        /// <param name="decryptStream">Return path for decrypted stream.</param>
        /// <param name="cipherText">Encrypted stream.</param>
        /// <param name="password"></param>
        /// <returns>Returns FALSE if verification code fails, otherwise TRUE.</returns>
        public static bool Decrypt(Stream decryptStream, Stream cipherText, string password)
        {
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

                byte[] verification = key.GetBytes(2);

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (CryptoStream csDecrypt = new CryptoStream(cipherText, decryptor, CryptoStreamMode.Read))
                {
                    var buffer = new byte[512];
                    var bytesRead = default(int);
                    // check verification code
                    bytesRead = csDecrypt.Read(buffer, 0, 2);
                    if (bytesRead != 2 || buffer[0] != verification[0] || buffer[1] != verification[1])
                        return false;

                    while ((bytesRead = csDecrypt.Read(buffer, 0, buffer.Length)) > 0)
                        decryptStream.Write(buffer, 0, bytesRead);
                }
            }
            return true;
        }

        /// <summary>
        /// Generate random salt bytes. Length is determined by BlockSize.
        /// </summary>
        /// <returns></returns>
        public static byte[] GenerateSalt()
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = KeySize;
                aes.BlockSize = BlockSize;

                aes.GenerateIV();
                byte[] salt = new byte[aes.BlockSize / 8];
                Buffer.BlockCopy(aes.IV, 0, salt, 0, salt.Length);
                return salt;
            }
        }
    }
}
