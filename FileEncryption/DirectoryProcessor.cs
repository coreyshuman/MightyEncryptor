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
    /// <summary>
    /// Manage reading and writing of files and folders.
    /// </summary>
    public class DirectoryProcessor
    {
        public DirectoryProcessor()
        {
 
        }

        /// <summary>
        /// Process all file and folder strings in args and populate folders and files lists.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="folders"></param>
        /// <param name="files"></param>
        public void ProcessPaths(string[] args, IList<string> folders, IList<FileHeaderV01> files)
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


        /// <summary>
        /// Process all files in the directory passed in, recurse in on any directories 
        /// that are found, and process the files they contain.
        /// </summary>
        /// <param name="targetDirectory"></param>
        /// <param name="topDirectory"></param>
        /// <param name="folders"></param>
        /// <param name="files"></param>
        private void ProcessDirectory(string targetDirectory, string topDirectory, IList<string> folders, IList<FileHeaderV01> files)
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

        /// <summary>
        /// Process file in 'path' and populate into 'files' list. File path is relative to 'topDirectory'.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="topDirectory"></param>
        /// <param name="files"></param>
        private void ProcessFile(string path, string topDirectory, IList<FileHeaderV01> files)
        {
            ulong size = (ulong)new FileInfo(path).Length;
            files.Add(new FileHeaderV01 { fullPath = path, filename = GetRelativePath(path, topDirectory), filesize = size });
        }

        /// <summary>
        /// Processes full path in 'fullPath' and returns relative path based on 'currentFolder'.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="currentFolder"></param>
        /// <returns></returns>
        private string GetRelativePath(string fullPath, string currentFolder)
        {
            // use the correct seperator for the environment
            var pathParts = fullPath.Split(Path.DirectorySeparatorChar);

            int startAfter = Array.IndexOf(pathParts, currentFolder);

            if (startAfter == -1)
            {
                // path not found
                return fullPath;
            }

            return string.Join(
                Path.DirectorySeparatorChar.ToString(),
                pathParts, startAfter,
                pathParts.Length - startAfter);
        }

        /// <summary>
        /// Appends relative path in 'path' to 'currentFolder'.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="currentFolder"></param>
        /// <returns></returns>
        private string GetFullPath(string path, string currentFolder)
        {
            return currentFolder + Path.DirectorySeparatorChar.ToString() + path;
        }

        /// <summary>
        /// Creates directories in 'folders' list at 'outputLocation'.
        /// </summary>
        /// <param name="outputLocation"></param>
        /// <param name="folders"></param>
        public void CreateFolders(string outputLocation, IEnumerable<string> folders)
        {
            foreach(var folder in folders)
            {
                Directory.CreateDirectory(GetFullPath(folder, outputLocation));
            }
        }

        /// <summary>
        /// Creates files in 'files' list using data from 'dataStream' at 'outputLocation'.
        /// </summary>
        /// <param name="outputLocation"></param>
        /// <param name="dataStream"></param>
        /// <param name="files"></param>
        public void CreateFiles(string outputLocation, Stream dataStream, IEnumerable<FileHeaderV01> files)
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

        /// <summary>
        /// Save data in 'dataStream' to new file created at 'filePath'.
        /// </summary>
        /// <param name="dataStream"></param>
        /// <param name="filePath"></param>
        public void SaveFile(Stream dataStream, string filePath)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                dataStream.Position = 0;
                dataStream.CopyTo(fileStream);
            }
        }

        /// <summary>
        /// Read file at 'fileStream' into 'dataStream' using GZip compression.
        /// </summary>
        /// <param name="dataStream"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
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
