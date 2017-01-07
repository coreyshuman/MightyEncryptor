using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FileEncryption
{
    public static class HMAC
    {
        /// <summary>
        /// Calculate SHA256 for data using given key. If key is null, random key will be generated.
        /// </summary>
        /// <param name="data">Data to be hashed</param>
        /// <param name="key">Hash key</param>
        /// <param name="hash">Calculated hash value</param>
        /// <returns>key</returns>
        private static byte[] HashSHA256(byte[] data, byte[] key, out byte[] hash)
        {
            if(key == null)
            {
                key = AES.GenerateSalt();
            }

            var sha = new HMACSHA256(key);
            hash = sha.ComputeHash(data);
            return key;
        }
    }
}
