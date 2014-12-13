using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace RemoteBuildReview
{
    public partial class TestConnect : Form
    {
        public TestConnect()
        {
            InitializeComponent();
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents();
            string[] dirs;
            using (UNCAccessWithCredentials unc = new UNCAccessWithCredentials())
            {
                if (unc.NetUseWithCredentials(tbUNCPath.Text,
                                              tbUserName.Text,
                                              tbDomain.Text,
                                              tbPassword.Text))
                {
                    dirs = Directory.GetDirectories(tbUNCPath.Text);
                    foreach (string d in dirs)
                    {
                        tbDirList.Text += d + "\r\n";
                    }
                }
                else
                {
                    this.Cursor = Cursors.Default;
                    MessageBox.Show("Failed to connect to " + tbUNCPath.Text + "\r\nLastError = " + unc.LastError.ToString(),
                                    "Failed to connect",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                }

            }
            this.Cursor = Cursors.Default;
        }
    }
}