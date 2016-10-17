namespace FileEncryption
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.debugTextbox = new System.Windows.Forms.TextBox();
            this.selectFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.encryptFolderButton = new System.Windows.Forms.Button();
            this.decryptButton = new System.Windows.Forms.Button();
            this.selectFolderDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.encryptFilesButton = new System.Windows.Forms.Button();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // debugTextbox
            // 
            this.debugTextbox.Location = new System.Drawing.Point(12, 81);
            this.debugTextbox.Multiline = true;
            this.debugTextbox.Name = "debugTextbox";
            this.debugTextbox.ReadOnly = true;
            this.debugTextbox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.debugTextbox.Size = new System.Drawing.Size(563, 215);
            this.debugTextbox.TabIndex = 1;
            // 
            // selectFileDialog
            // 
            this.selectFileDialog.FileName = "openFileDialog1";
            this.selectFileDialog.Multiselect = true;
            // 
            // encryptFolderButton
            // 
            this.encryptFolderButton.Location = new System.Drawing.Point(12, 12);
            this.encryptFolderButton.Name = "encryptFolderButton";
            this.encryptFolderButton.Size = new System.Drawing.Size(87, 40);
            this.encryptFolderButton.TabIndex = 8;
            this.encryptFolderButton.Text = "Encrypt Folder";
            this.encryptFolderButton.UseVisualStyleBackColor = true;
            this.encryptFolderButton.Click += new System.EventHandler(this.encryptFolderButton_Click);
            // 
            // decryptButton
            // 
            this.decryptButton.Location = new System.Drawing.Point(198, 12);
            this.decryptButton.Name = "decryptButton";
            this.decryptButton.Size = new System.Drawing.Size(87, 40);
            this.decryptButton.TabIndex = 9;
            this.decryptButton.Text = "Decrypt";
            this.decryptButton.UseVisualStyleBackColor = true;
            this.decryptButton.Click += new System.EventHandler(this.decryptButton_Click);
            // 
            // encryptFilesButton
            // 
            this.encryptFilesButton.Location = new System.Drawing.Point(105, 12);
            this.encryptFilesButton.Name = "encryptFilesButton";
            this.encryptFilesButton.Size = new System.Drawing.Size(87, 40);
            this.encryptFilesButton.TabIndex = 10;
            this.encryptFilesButton.Text = "Encrypt Files";
            this.encryptFilesButton.UseVisualStyleBackColor = true;
            this.encryptFilesButton.Click += new System.EventHandler(this.encryptFilesButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(587, 308);
            this.Controls.Add(this.encryptFilesButton);
            this.Controls.Add(this.decryptButton);
            this.Controls.Add(this.encryptFolderButton);
            this.Controls.Add(this.debugTextbox);
            this.Name = "Form1";
            this.Text = "File Encryption";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox debugTextbox;
        private System.Windows.Forms.OpenFileDialog selectFileDialog;
        private System.Windows.Forms.Button encryptFolderButton;
        private System.Windows.Forms.Button decryptButton;
        private System.Windows.Forms.FolderBrowserDialog selectFolderDialog;
        private System.Windows.Forms.Button encryptFilesButton;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
    }
}

