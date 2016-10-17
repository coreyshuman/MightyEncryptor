using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FileEncryption.Models;
using System.IO.Compression;

namespace FileEncryption
{
    public class DirectoryProcessor
    {
        public DirectoryProcessor()
        {
 
        }

        public void ProcessPaths(string[] args, IList<string> folders, IList<FileHeader> files)
        {
            string currentFolder = "";
            foreach (string path in args)
            {
                if (File.Exists(path))
                {
                    var pathParts = path.Split(Path.DirectorySeparatorChar);
                    currentFolder = pathParts[pathParts.Length - 1];
                    // This path is a file
                    ProcessFile(path, currentFolder, files);
                }
                else if (Directory.Exists(path))
                {
                    var pathParts = path.Split(Path.DirectorySeparatorChar);
                    currentFolder = pathParts[pathParts.Length - 1];
                    // This path is a directory
                    ProcessDirectory(path, currentFolder, folders, files);
                }
                else
                {
                    Console.WriteLine("{0} is not a valid file or directory.", path);
                }
            }
        }


        // Process all files in the directory passed in, recurse on any directories 
        // that are found, and process the files they contain.
        private void ProcessDirectory(string targetDirectory, string topDirectory, IList<string> folders, IList<FileHeader> files)
        {
            folders.Add(GetRelativePath(targetDirectory, topDirectory));
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessFile(fileName, topDirectory, files);

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory, topDirectory, folders, files);
        }

        // Insert logic for processing found files here.
        private void ProcessFile(string path, string topDirectory, IList<FileHeader> files)
        {
            ulong size = (ulong)new FileInfo(path).Length;
            files.Add(new FileHeader { fullPath = path, filename = GetRelativePath(path, topDirectory), filesize = size });
        }

        private string GetRelativePath(string path, string currentFolder)
        {
            // use the correct seperator for the environment
            var pathParts = path.Split(Path.DirectorySeparatorChar);

            int startAfter = Array.IndexOf(pathParts, currentFolder);

            if (startAfter == -1)
            {
                // path not found
                return path;
            }

            return string.Join(
                Path.DirectorySeparatorChar.ToString(),
                pathParts, startAfter,
                pathParts.Length - startAfter);
        }

        private string GetFullPath(string path, string currentFolder)
        {
            return currentFolder + Path.DirectorySeparatorChar.ToString() + path;
        }

        public void CreateFolders(string outputLocation, IEnumerable<string> folders)
        {
            foreach(var folder in folders)
            {
                Directory.CreateDirectory(GetFullPath(folder, outputLocation));
            }
        }

        public void CreateFiles(string outputLocation, Stream dataStream, IEnumerable<FileHeader> files)
        {
            byte[] buffer = new byte[0xFFFF];
            ulong remainingBytes = 0;
            int readCount = 0;

            foreach (var file in files)
            {
                using (MemoryStream tempStream = new MemoryStream((int)file.filesize))
                {
                    remainingBytes = file.filesize;
                    while (remainingBytes > 0)
                    {
                        if (remainingBytes > 0xFFFF)
                        {
                            readCount = 0xFFFF;
                        }
                        else
                        {
                            readCount = (int)remainingBytes;
                        }
                        dataStream.Read(buffer, 0, readCount);
                        tempStream.Write(buffer, 0, readCount);
                        remainingBytes -= (ulong)readCount;
                        
                    }
                    using (FileStream newFileStream = new FileStream(GetFullPath(file.filename, outputLocation), FileMode.Create, FileAccess.Write))
                    {
                        tempStream.Position = 0;
                        using (GZipStream compressionStream = new GZipStream(tempStream, CompressionMode.Decompress))
                        {
                            compressionStream.CopyTo(newFileStream);
                        }
                    }
                }
            }
        }

        public void SaveFile(Stream dataStream, string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                dataStream.Position = 0;
                dataStream.CopyTo(fileStream);
            }
        }

        public ulong GetFile(Stream dataStream, string filePath)
        {
            ulong fileSize = (ulong)dataStream.Position;
            using (var curFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (GZipStream compressionStream = new GZipStream(dataStream, CompressionMode.Compress, true))
                {
                    curFileStream.CopyTo(compressionStream);
                }
            }
            fileSize = (ulong)dataStream.Position - fileSize;
            return fileSize; // update file size
        }
    }
}
