using FileEncryption.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileEncryption
{
    class BlockFileProcessor
    {
        const UInt16 MaxBlockVersion = 1;

        private DirectoryProcessor directoryProcessor { get; set; }

        public BlockFileProcessor(DirectoryProcessor dp)
        {
            directoryProcessor = dp;
        }

        /// <summary>
        /// Decode the header section of an unencrypted block file an populate the folders and files lists.
        /// </summary>
        /// <param name="dataStream">Data stream of unencrypted block file.</param>
        /// <param name="folders"></param>
        /// <param name="files"></param>
        public void DecodeHeader(Stream dataStream, IList<string> folders, IList<FileHeaderV01> files)
        {
            int res;
            UInt16 version;
            dataStream.Position = 0;
            byte[] buffer = new byte[4];

            // verify header tag
            res = dataStream.Read(buffer, 0, 4);
            if (res != 1)
            {
                throw new Exception("Unexpected EOF");
            }

            if(buffer[0] != 'M' || buffer[1] != 'E' || buffer[2] != 'B' || buffer[3] != 'F')
            {
                throw new Exception("Invalid file.");
            }

            res = dataStream.Read(buffer, 0, 1);
            if (res != 1)
            {
                throw new Exception("Unexpected EOF");
            }

            // get file version
            version = 0;
            res = dataStream.Read(buffer, 0, 1);
            if (res != 1)
            {
                throw new Exception("Unexpected EOF");
            }
            version = (UInt16)(buffer[0] * 256);
            res = dataStream.Read(buffer, 0, 1);
            if (res != 1)
            {
                throw new Exception("Unexpected EOF");
            }
            version += buffer[0];
            if(version < 1 || version > MaxBlockVersion)
            {
                throw new Exception("Invalid file version.");
            }

            switch (version)
            {
                case 1:
                    DecodeHeaderV01(dataStream, folders, files);
                    break;
            }
        }

        /// <summary>
        /// Generate header section of the block file.
        /// </summary>
        /// <param name="folders"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        public Stream GenerateHeader(IEnumerable<string> folders, IEnumerable<FileHeaderV01> files)
        {
            MemoryStream headerStream = new MemoryStream(1024);
            try
            {
                headerStream.SetLength(0);

                // write header tag and version number
                headerStream.Write(Encoding.ASCII.GetBytes("MEBF"), 0, 4);
                headerStream.Write(BitConverter.GetBytes(MaxBlockVersion), 0, 2);

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
            catch (Exception e)
            {
                throw e; // pass error up to next handler
            }

            return headerStream;
        }

        #region DecodeHeader versions
        /// <summary>
        /// Decode the header section of an unencrypted block file an populate the folders and files lists.
        /// </summary>
        /// <param name="dataStream">Data stream of unencrypted block file.</param>
        /// <param name="folders"></param>
        /// <param name="files"></param>
        private void DecodeHeaderV01(Stream dataStream, IList<string> folders, IList<FileHeaderV01> files)
        {
            int res;
            byte[] buffer = new byte[0xFFFF];
            

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
                FileHeaderV01 file = new FileHeaderV01();
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
        #endregion

        /// <summary>
        /// Combines file content of listed files into a single data stream. Files are compressed individually using GZip compression.
        /// </summary>
        /// <param name="folders"></param>
        /// <param name="files"></param>
        /// <returns>Stream</returns>
        public Stream PackageFiles(IEnumerable<string> folders, IEnumerable<Models.FileHeaderV01> files)
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

                    fileSize = directoryProcessor.GetFile(dataStream, file.fullPath);
                    file.filesize = fileSize;
                }
            }
            catch (Exception e)
            {
                throw e; // pass error up to next handler
            }
            finally
            {
                if (curFileStream != null)
                {
                    curFileStream.Dispose();
                }
            }
            return dataStream;
        }
    }
}
