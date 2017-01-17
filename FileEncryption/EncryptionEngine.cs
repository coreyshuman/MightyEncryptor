using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileEncryption.Models;
using System.IO.Compression;

namespace FileEncryption
{
    /// <summary>
    /// Main controller for encoding and decoding encrypted ME files.
    /// </summary>
    class EncryptionEngine
    {
        public const UInt16 MaxEEVersion = 1;

        private Debugging debug { get; set; }
        private DirectoryProcessor dirProc { get; set; }
        private BlockFileProcessor blockFileProcessor { get; set; }

        public EncryptionEngine()
        {
            debug = Debugging.Instance;
            dirProc = new DirectoryProcessor();
            blockFileProcessor = new BlockFileProcessor(dirProc);
        }

        

        /// <summary>
        /// Process folders and files using compression, creates block file header and file package, and encrypts data stream.
        /// </summary>
        /// <param name="folders"></param>
        /// <param name="files"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public Stream Encrypt(IEnumerable<string> folders, IEnumerable<FileHeaderV01> files, string password)
        {
            MemoryStream dataStream = new MemoryStream();
            MemoryStream encryptedStream = null;
            Stream packageStream = null;
            Stream headerStream = null;
            try
            {
                debug.WriteLine("Beginning Encryption Engine...");
                byte[] hmacKey = AES.GenerateSalt();
                // package files first to generate compressed file sizes
                debug.WriteLine("Packaging Files.");
                packageStream = blockFileProcessor.PackageFiles(folders, files, hmacKey);
                debug.WriteLine("Generating Header.");
                headerStream = blockFileProcessor.GenerateHeader(folders, files, hmacKey);

                // combine streams
                dataStream.Capacity = (int)(headerStream.Length + packageStream.Length);
                headerStream.Position = 0;
                headerStream.CopyTo(dataStream);
                packageStream.Position = 0;
                packageStream.CopyTo(dataStream);
                headerStream.Dispose();
                headerStream = null;
                packageStream.Dispose();
                packageStream = null;
                byte[] encryptedData = AES.Encrypt(dataStream, password);
                //encryptedStream = new MemoryStream(encryptedData);
                encryptedStream = new MemoryStream(encryptedData.Length + 32);
                // write header tag
                encryptedStream.Write(Encoding.ASCII.GetBytes("MEBF"), 0, 4);
                // write version number
                byte[] version = BitConverter.GetBytes(MaxEEVersion);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(version);
                headerStream.Write(version, 0, 2);
                // append encrypted string (containing salt and verification code)
                headerStream.Write(encryptedData, 0, encryptedData.Length);
            }
            catch(Exception e)
            {
                debug.WriteLine("Error occured: " + e.Message);
            }
            finally
            {
                if(packageStream != null)
                {
                    packageStream.Dispose();
                }
                if(headerStream != null)
                {
                    headerStream.Dispose();
                }
                dataStream.Dispose();
            }
            return encryptedStream;
        }

        
        /// <summary>
        /// Decrpyts file stream using password and parses header block to determine file list.
        /// </summary>
        /// <param name="blockStream"></param>
        /// <param name="folders"></param>
        /// <param name="files"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public Stream Decrypt(Stream blockStream, IList<string> folders, IList<FileHeaderV01> files, string password)
        {
            MemoryStream decryptedStream = new MemoryStream();
            try
            {
                debug.WriteLine("Beginning Decryption Engine...");
                debug.WriteLine("Decrypting.");
                AES.Decrypt(decryptedStream, blockStream, password);
                debug.WriteLine("Decode Header.");
                blockFileProcessor.DecodeHeader(decryptedStream, folders, files);
            }
            catch (Exception e)
            {
                debug.WriteLine("Error occured: " + e.Message);
            }
            finally
            {
                
            }

            return decryptedStream;
        }
    }
}
