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
    class EncryptionEngine
    {
        private Debugging debug { get; set; }
        private DirectoryProcessor dirProc { get; set; }
        private AES aes { get; set; }

        public EncryptionEngine()
        {
            debug = Debugging.Instance;
            dirProc = new DirectoryProcessor();
            aes = new AES();

            
        }

        public Stream GenerateHeader(IEnumerable<string> folders, IEnumerable<FileHeader> files)
        {
            MemoryStream headerStream = new MemoryStream(1024);
            try
            {
                headerStream.SetLength(0);

                foreach (var folder in folders)
                {
                    if (folder.Length > 0xFFFF || folder.Length == 0)
                    {
                        continue;
                    }
                    headerStream.Append((byte)(folder.Length >> 8), 1);
                    headerStream.Append((byte)(folder.Length & 0xFF), 1);
                    headerStream.Write(Encoding.ASCII.GetBytes(folder), 0, folder.Length);
                }
                headerStream.Append(0x00, 2);
                foreach (var file in files)
                {
                    if (file.filename.Length > 0xFFFF || file.filename.Length == 0)
                    {
                        continue;
                    }
                    if (file.filesize > 0xFFFFFFFF)
                    {
                        continue;
                    }
                    headerStream.Append((byte)(file.filename.Length >> 8), 1);
                    headerStream.Append((byte)(file.filename.Length & 0xFF), 1);
                    headerStream.Write(Encoding.ASCII.GetBytes(file.filename), 0, file.filename.Length);
                    headerStream.Append((byte)((file.filesize >> 24) & 0xFF), 1);
                    headerStream.Append((byte)((file.filesize >> 16) & 0xFF), 1);
                    headerStream.Append((byte)((file.filesize >> 8) & 0xFF), 1);
                    headerStream.Append((byte)(file.filesize & 0xFF), 1);
                }
                headerStream.Append(0x00, 2);
                
            }
            catch(Exception e)
            {
                throw e; // pass error up to next handler
            }

            return headerStream;
        }

        private void DecodeHeader(Stream dataStream, IList<string> folders, IList<FileHeader> files)
        {
            int res;
            byte[] buffer = new byte[0xFFFF];
            byte[] iv = new byte[16];
            dataStream.Position = 0;
            
            // loop through folders
            while (true)
            {
                // read size of folder path/name
                int size = 0;
                res = dataStream.Read(buffer, 0, 1);
                if (res != 1)
                {
                    throw new Exception("Unexpected EOF");
                }
                size = buffer[0] * 256;
                res = dataStream.Read(buffer, 0, 1);
                if (res != 1)
                {
                    throw new Exception("Unexpected EOF");
                }
                size += buffer[0];
                // if size == 0 then we are finished with folders
                if (size == 0)
                {
                    break;
                }
                // read folder path/name
                res = dataStream.Read(buffer, 0, size);
                if (res != size)
                {
                    throw new Exception("Unexpected EOF");
                }
                folders.Add(Encoding.UTF8.GetString(buffer, 0, size));
            }

            // loop through files
            while (true)
            {
                // read size of file path/name
                int pathSize = 0;
                ulong fileSize = 0;
                FileHeader file = new FileHeader();
                res = dataStream.Read(buffer, 0, 1);
                if (res != 1)
                {
                    throw new Exception("Unexpected EOF");
                }
                pathSize = buffer[0] * 256;
                res = dataStream.Read(buffer, 0, 1);
                if (res != 1)
                {
                    throw new Exception("Unexpected EOF");
                }
                pathSize += buffer[0];
                // if size == 0 then we are finished with files
                if (pathSize == 0)
                {
                    break;
                }
                // read folder path/name
                res = dataStream.Read(buffer, 0, pathSize);
                if (res != pathSize)
                {
                    throw new Exception("Unexpected EOF");
                }
                file.filename = Encoding.UTF8.GetString(buffer, 0, pathSize);
                // read file size
                res = dataStream.Read(buffer, 0, 1);
                if (res != 1)
                {
                    throw new Exception("Unexpected EOF");
                }
                fileSize = (ulong)(buffer[0] * 0x1000000);
                res = dataStream.Read(buffer, 0, 1);
                if (res != 1)
                {
                    throw new Exception("Unexpected EOF");
                }
                fileSize += (ulong)(buffer[0] * 0x10000);
                res = dataStream.Read(buffer, 0, 1);
                if (res != 1)
                {
                    throw new Exception("Unexpected EOF");
                }
                fileSize += (ulong)(buffer[0] * 0x100);
                res = dataStream.Read(buffer, 0, 1);
                if (res != 1)
                {
                    throw new Exception("Unexpected EOF");
                }
                fileSize += buffer[0];
                file.filesize = fileSize;
                // save file info
                files.Add(file);
            }
        }

        private Stream PackageFiles(IEnumerable<string> folders, IEnumerable<FileHeader> files)
        {
            FileStream curFileStream = null;
            MemoryStream dataStream = new MemoryStream();
            ulong fileSize = 0;
            try
            {
                // sum file sizes to calculate file stream capacity
                foreach (var file in files)
                {
                    if (file.filename.Length > 0xFFFF || file.filename.Length == 0)
                    {
                        continue;
                    }
                    if (file.filesize > 0xFFFFFFFF)
                    {
                        continue;
                    }
                    fileSize += file.filesize;
                }
                dataStream.Capacity = (int)fileSize;
                foreach (var file in files)
                {
                    if (file.filename.Length > 0xFFFF || file.filename.Length == 0)
                    {
                        continue;
                    }
                    if (file.filesize > 0xFFFFFFFF)
                    {
                        continue;
                    }

                    fileSize = dirProc.GetFile(dataStream, file.fullPath);
                    file.filesize = fileSize;
                }
            }
            catch (Exception e)
            {
                throw e; // pass error up to next handler
            }
            finally
            {
                if(curFileStream != null)
                {
                    curFileStream.Dispose();
                }
            }
            return dataStream;
        }

        public Stream Encrypt(IEnumerable<string> folders, IEnumerable<FileHeader> files, string password)
        {
            MemoryStream dataStream = new MemoryStream();
            MemoryStream encryptedStream = null;
            Stream packageStream = null;
            Stream headerStream = null;
            try
            {
                debug.WriteLine("Beginning Encryption Engine...");
                // package files first to generate compressed file sizes
                debug.WriteLine("Packaging Files.");
                packageStream = PackageFiles(folders, files);
                debug.WriteLine("Generating Header.");
                headerStream = GenerateHeader(folders, files);

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
                byte[] encryptedData = aes.Encrypt(dataStream, password);
                encryptedStream = new MemoryStream(encryptedData);
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

        

        public Stream Decrypt(Stream blockStream, IList<string> folders, IList<FileHeader> files, string password)
        {
            MemoryStream decryptedStream = new MemoryStream();
            try
            {
                debug.WriteLine("Beginning Decryption Engine...");
                debug.WriteLine("Decrypting.");
                aes.Decrypt(decryptedStream, blockStream, password);
                debug.WriteLine("Decode Header.");
                DecodeHeader(decryptedStream, folders, files);
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
