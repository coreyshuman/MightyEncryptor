using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileEncryption
{
    public partial class GetPasswordForm : Form
    {
        public string Password { get; set; }

        public GetPasswordForm()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Password = passwordTextbox.Text;
            string password2 = passwordRepeatTextbox.Text;

            if(Password != password2)
            {
                errorLabel.Text = "Error: Passwords do not match.";
            }
            else
            {
                errorLabel.Text = "";
                this.DialogResult = DialogResult.OK;
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
