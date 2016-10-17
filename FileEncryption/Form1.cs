using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FileEncryption.Models;
using System.Globalization;
using System.Reflection;

namespace FileEncryption
{
    public partial class Form1 : Form
    {
        

        private Debugging debug { get; set; }
        private DirectoryProcessor dirProc { get; set; }
        private EncryptionEngine encryption { get; set; }
        

        public Form1()
        {
            InitializeComponent();
            dirProc = new DirectoryProcessor();
            encryption = new EncryptionEngine();

            debug = Debugging.Instance;
            debug.SetForm(this);
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            string name = Assembly.GetExecutingAssembly().GetName().Name;
            string about = string.Format(CultureInfo.InvariantCulture, @"{0} Version {1}.{2}.{3} (r{4})", name, v.Major, v.Minor, v.Build, v.Revision);
            debug.WriteLine(about);

            // load settings
            bool needSave = false;
            if (Properties.Settings.Default.LastEncryptFilesPath == "")
            {
                Properties.Settings.Default.LastEncryptFilesPath = Environment.SpecialFolder.MyDocuments.ToString();
                needSave = true;
            }
            if (Properties.Settings.Default.LastEncryptSavePath == "")
            {
                Properties.Settings.Default.LastEncryptSavePath = Environment.SpecialFolder.MyDocuments.ToString();
                needSave = true;
            }
            if (Properties.Settings.Default.LastDecryptFilePath == "")
            {
                Properties.Settings.Default.LastDecryptFilePath = Environment.SpecialFolder.MyDocuments.ToString();
                needSave = true;
            }
            if (Properties.Settings.Default.LastDecryptSavePath == "")
            {
                Properties.Settings.Default.LastDecryptSavePath = Environment.SpecialFolder.MyDocuments.ToString();
                needSave = true;
            }
            if (needSave)
            {
                Properties.Settings.Default.Save();
            }
        }

        public void Test()
        {
            try
            {
                byte[] original = Encoding.ASCII.GetBytes("Here is some data to encrypt!");

                MemoryStream file = new MemoryStream(original);
                MemoryStream encryptStream = new MemoryStream();
                MemoryStream decryptStream = new MemoryStream();

                // Create a new instance of the Aes
                // class.  This generates a new key and initialization 
                // vector (IV).
                using (Aes myAes = Aes.Create())
                {
                    // Encrypt the string to an array of bytes.
                    AddDebugLine("Encrypting...");
                    //EncryptAES(encryptStream, file, myAes.Key, myAes.IV);

                    // Decrypt the bytes to a string.
                    AddDebugLine("Decrypting...");
                    //DecryptAES(decryptStream, new MemoryStream(encryptStream.ToArray()), myAes.Key, myAes.IV);

                    //Display the original data and the decrypted data.
                    AddDebugLine("Original:   " + Encoding.UTF8.GetString(original));
                    AddDebugLine("Round Trip: " + Encoding.UTF8.GetString(decryptStream.ToArray()));
                }
            }
            catch (Exception e)
            {
                AddDebugLine("Error: " + e.Message);
            }
        }

        public void AddDebugLine(string debug)
        {
            debugTextbox.Text += debug + Environment.NewLine;
        }


        private void testButton_Click(object sender, EventArgs e)
        {
            AddDebugLine("Running Test...");
            Test();
        }

        private void encryptFolderButton_Click(object sender, EventArgs e)
        {
            selectFolderDialog.SelectedPath = Properties.Settings.Default.LastEncryptFilesPath;
            saveFileDialog.FileName = Properties.Settings.Default.LastEncryptSavePath;

            // get password value
            string password = "";
            bool cont = false;
            GetPasswordForm pwf = new GetPasswordForm();
            if(pwf.ShowDialog(this) == DialogResult.OK)
            {
                password = pwf.Password;
                cont = true;
            }

            pwf.Dispose();

            if (cont)
            {
                var result = selectFolderDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    result = saveFileDialog.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        EncryptBlockFile(new string[] { selectFolderDialog.SelectedPath }, saveFileDialog.FileName, password);
                        Properties.Settings.Default.LastEncryptFilesPath = selectFolderDialog.SelectedPath;
                        Properties.Settings.Default.LastEncryptSavePath = saveFileDialog.FileName;
                        Properties.Settings.Default.Save();
                    }
                }
            }
        }

        private void encryptFilesButton_Click(object sender, EventArgs e)
        {
            selectFileDialog.Multiselect = true;
            selectFileDialog.FileName = Properties.Settings.Default.LastEncryptFilesPath;
            saveFileDialog.FileName = Properties.Settings.Default.LastEncryptSavePath;

            // get password value
            string password = "";
            bool cont = false;
            GetPasswordForm pwf = new GetPasswordForm();
            if (pwf.ShowDialog(this) == DialogResult.OK)
            {
                password = pwf.Password;
                cont = true;
            }

            pwf.Dispose();

            if (cont)
            {

                var result = selectFileDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    result = saveFileDialog.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        EncryptBlockFile(selectFileDialog.FileNames, saveFileDialog.FileName, password);
                        Properties.Settings.Default.LastEncryptFilesPath = Path.GetDirectoryName(selectFileDialog.FileName);
                        Properties.Settings.Default.LastEncryptSavePath = saveFileDialog.FileName;
                        Properties.Settings.Default.Save();
                    }
                }
            }
        }

        private void decryptButton_Click(object sender, EventArgs e)
        {
            selectFileDialog.Multiselect = false;
            selectFileDialog.FileName = Properties.Settings.Default.LastDecryptFilePath;
            selectFolderDialog.SelectedPath = Properties.Settings.Default.LastDecryptSavePath;
            var result = selectFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                // get password value
                string password = "";
                bool cont = false;
                GetPasswordForm pwf = new GetPasswordForm();
                if (pwf.ShowDialog(this) == DialogResult.OK)
                {
                    password = pwf.Password;
                    cont = true;
                }

                pwf.Dispose();

                if (cont)
                {
                    result = selectFolderDialog.ShowDialog();

                    if (result == DialogResult.OK)
                    {
                        DecryptBlockFile(selectFileDialog.FileName, selectFolderDialog.SelectedPath, password);
                        Properties.Settings.Default.LastDecryptFilePath = selectFileDialog.FileName;
                        Properties.Settings.Default.LastDecryptSavePath = selectFolderDialog.SelectedPath;
                        Properties.Settings.Default.Save();
                    }
                } 
            }
        }



        private void DecryptBlockFile(string filePath, string saveFilePath, string password)
        {
            List<string> folders = new List<string>();
            List<FileHeader> files = new List<FileHeader>();
            // open block file
            FileStream blockFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            // decrypt file
            Stream decryptedStream = encryption.Decrypt(blockFileStream, folders, files, password);
            // generate output
            debug.WriteLine("Create Folders.");
            dirProc.CreateFolders(saveFilePath, folders);
            debug.WriteLine("Create Files.");
            dirProc.CreateFiles(saveFilePath, decryptedStream, files);
            blockFileStream.Dispose();
        }

        private void EncryptBlockFile(string[] paths, string saveFilePath, string password)
        {
            List<string> folders = new List<string>();
            List<FileHeader> files = new List<FileHeader>();

            AddDebugLine("Processing files...");
            dirProc.ProcessPaths(paths, folders, files);
            AddDebugLine("Folders:");

            foreach (var folder in folders)
            {
                AddDebugLine("- " + folder);
            }
            AddDebugLine("Files:");

            foreach (var file in files)
            {
                AddDebugLine("- " + file.filename);
            }
            AddDebugLine("Done.");

            // encrypt files
            Stream blockData = encryption.Encrypt(folders, files, password);

            // save encryption block
            dirProc.SaveFile(blockData, saveFilePath);
            blockData.Dispose();
        }

        
    }

}
