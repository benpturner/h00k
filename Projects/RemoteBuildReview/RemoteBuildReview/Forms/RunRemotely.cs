using RemoteBuildReview.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Windows.Forms;

namespace RemoteBuildReview
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            string currentDir = Environment.CurrentDirectory;
            DirectoryInfo directory = new DirectoryInfo(currentDir);
            outputdirtxt.Text = currentDir;

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void copyDirectory(string strSource, string strDestination)
        {
            if (!Directory.Exists(strDestination))
            {
                Directory.CreateDirectory(strDestination);
            }
            DirectoryInfo dirInfo = new DirectoryInfo(strSource);
            FileInfo[] files = dirInfo.GetFiles();
            foreach (FileInfo tempfile in files)
            {
                tempfile.CopyTo(Path.Combine(strDestination, tempfile.Name));
            }
            DirectoryInfo[] dirctororys = dirInfo.GetDirectories();
            foreach (DirectoryInfo tempdir in dirctororys)
            {
                copyDirectory(Path.Combine(strSource, tempdir.Name), Path.Combine(strDestination, tempdir.Name));
            }

        }
        private void button1_Click(object sender, EventArgs e)
        {
            string currentDir = Environment.CurrentDirectory;
            DirectoryInfo directory = new DirectoryInfo(currentDir);
            FileInfo file = new FileInfo("rbr.exe");
            string fullDirectory = directory.FullName;
            string fullFile = file.FullName;

            foreach (string item in targetslist.Items)
            {
                string loadexe = fullFile;
                string outputfile = outputdirtxt.Text;
                string filenew = "\\\\" + item + "\\admin$\\temp\\rbr.exe";
                string uncpath = "\\\\" + item + "\\admin$";

                //if computer name is localhost
                if (item.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                {

                    try
                    {
                        ManagementScope scope = new ManagementScope("\\\\" + item + "\\root\\CIMV2", null);
                        scope.Connect();

                        ObjectGetOptions objectGetOptions = new ObjectGetOptions();
                        ManagementPath managementPath = new ManagementPath("Win32_Process");
                        ManagementClass processClass = new ManagementClass(scope, managementPath, objectGetOptions);
                        ManagementBaseObject inParams = processClass.GetMethodParameters("Create");
                        inParams["CommandLine"] = @"cmd.exe /c " + loadexe;
                        ManagementBaseObject outParams = processClass.InvokeMethod("Create", inParams, null);
                        // MessageBox.Show("Creation of the process returned: " + outParams["returnValue"]);
                        // MessageBox.Show("Process ID: " + outParams["processId"]);

                        // now check the process has finished

                        int i = 0;
                        while (i != 0)
                        {
                            ObjectQuery Query = new ObjectQuery("SELECT * FROM Win32_Process Where Name='rbr.exe'");
                            ManagementObjectSearcher Searcher = new ManagementObjectSearcher(scope, Query);
                            if (Searcher.Get() == null)
                            {
                                i = 11;
                            }

                        }
                        string subPath = outputdirtxt.Text;
                        bool isExists = System.IO.Directory.Exists(subPath);
                        try
                        {
                            if (!isExists)
                            {
                                System.IO.Directory.CreateDirectory(subPath);
                            }
                            else
                            {
                                //MessageBox.Show("Could not create local di");
                            }
                        }
                        catch
                        {
                            MessageBox.Show("Could not create local directory on C:\rbr");
                        }

                        string subDir2 = subPath + "\\" + item;
                        bool isExists2 = System.IO.Directory.Exists(subDir2);
                        try
                        {
                            if (!isExists2)
                            {
                                System.IO.Directory.CreateDirectory(subDir2);
                            }
                            else
                            {
                                // MessageBox.Show("Could not crea");
                            }
                        }
                        catch
                        {
                            MessageBox.Show("Could not create host directory in C:\rbr");
                        }

                        string outputdir = subDir2;
                        string dir = "C:\\windows\\temp\\rbr";

                        try
                        {
                            copyDirectory(dir, outputdir);
                        }
                        catch
                        {
                            MessageBox.Show("Error: Could not copy file");
                        }

                    }
                    catch
                    {
                        MessageBox.Show("Error: Could not copy file");
                    }
                        MessageBox.Show("Successfully ran against:" + item);
                }

                //do it remotely


                using (UNCAccessWithCredentials unc = new UNCAccessWithCredentials())
                {
                    if (unc.NetUseWithCredentials(uncpath,
                                                  username.Text,
                                                  domain.Text,
                                                  password.Text))
                    {
                        try
                        {
                            File.Copy(loadexe, filenew, true);
                        }
                        catch
                        {
                            MessageBox.Show("Error: Could not copy file");
                        }
                        try
                        {
                            ConnectionOptions connection = new ConnectionOptions();
                            connection.Impersonation = ImpersonationLevel.Impersonate;
                            connection.EnablePrivileges = true;
                            connection.Username = username.Text;
                            connection.Password = password.Text;
                            connection.Authority = "ntlmdomain:" + domain.Text;

                            ManagementScope scope = new ManagementScope("\\\\" + item + "\\root\\CIMV2", connection);
                            scope.Connect();

                            ObjectGetOptions objectGetOptions = new ObjectGetOptions();
                            ManagementPath managementPath = new ManagementPath("Win32_Process");
                            ManagementClass processClass = new ManagementClass(scope, managementPath, objectGetOptions);
                            ManagementBaseObject inParams = processClass.GetMethodParameters("Create");
                            inParams["CommandLine"] = @"cmd.exe /c c:\windows\temp\rbr.exe";
                            ManagementBaseObject outParams = processClass.InvokeMethod("Create", inParams, null);
                            // MessageBox.Show("Creation of the process returned: " + outParams["returnValue"]);
                            // MessageBox.Show("Process ID: " + outParams["processId"]);

                            // now check the process has finished

                            int i = 0;
                            while (i != 0)
                            {
                                ObjectQuery Query = new ObjectQuery("SELECT * FROM Win32_Process Where Name='rbr.exe'");
                                ManagementObjectSearcher Searcher = new ManagementObjectSearcher(scope, Query);
                                if (Searcher.Get() == null)
                                {
                                    i = 11;
                                }

                            }

                            //get the files back
                            string subPath = outputdirtxt.Text;


                            bool isExists = System.IO.Directory.Exists(subPath);
                            try
                            {
                                if (!isExists)
                                {
                                    System.IO.Directory.CreateDirectory(subPath);
                                }
                                else
                                {
                                    //MessageBox.Show("Could not create local di");
                                }
                            }
                            catch
                            {
                                MessageBox.Show("Could not create local directory on C:\rbr");
                            }

                            string subDir2 = subPath + "\\" + item;
                            bool isExists2 = System.IO.Directory.Exists(subDir2);
                            try
                            {
                                if (!isExists2)
                                {
                                    System.IO.Directory.CreateDirectory(subDir2);
                                }
                                else
                                {
                                    // MessageBox.Show("Could not crea");
                                }
                            }
                            catch
                            {
                                MessageBox.Show("Could not create host directory in C:\rbr");
                            }


                            string outputdir = subDir2;
                            string dir = "\\\\" + item + "\\admin$\\temp\\rbr";

                            using (UNCAccessWithCredentials unc2 = new UNCAccessWithCredentials())
                            {
                                if (unc2.NetUseWithCredentials(uncpath,
                                                              username.Text,
                                                              domain.Text,
                                                              password.Text))
                                {
                                    try
                                    {
                                        copyDirectory(dir, outputdir);
                                    }
                                    catch
                                    {
                                        MessageBox.Show("Error: Could not copy file");
                                    }

                                }
                                else
                                {
                                    this.Cursor = Cursors.Default;
                                    MessageBox.Show("Failed to connect to UNC " + uncpath + "\r\nLastError = " + unc.LastError.ToString(),
                                                    "Failed to connect",
                                                    MessageBoxButtons.OK,
                                                    MessageBoxIcon.Error);

                                }

                            }
                            //get em back end
                            MessageBox.Show("Successfully ran against:" + item);


                        }
                        catch
                        {
                            this.Cursor = Cursors.Default;
                            MessageBox.Show("Failed to connect to WMI " + uncpath + "\r\nLastError = ",
                                            "Failed to connect",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Error);
                        }



                    }
                    else
                    {
                        this.Cursor = Cursors.Default;
                        MessageBox.Show("Failed to connect to UNC " + uncpath + "\r\nLastError = " + unc.LastError.ToString(),
                                        "Failed to connect",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);

                    }

                }
            }

        }

        private void username_TextChanged(object sender, EventArgs e)
        {

        }

        private void domain_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void password_TextChanged(object sender, EventArgs e)
        {

        }

        private void domainLabel_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void ip_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }


        private void button2_Click(object sender, EventArgs e)
        {
            //save file code
            FolderBrowserDialog dialog = new FolderBrowserDialog();

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                outputdirtxt.Text = dialog.SelectedPath;
            }

            else
            {
                MessageBox.Show("No file selected");
                return;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            targetslist.Items.Add(ip.Text);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            targetslist.Items.Remove(targetslist.SelectedItem);
        }


        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About formAbout = new About();
            formAbout.Show();
        }

        private void kBToMSConverterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //KB2MS_Form newKB2MS = new KB2MS_Form();
            //newKB2MS.Show();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //// When user clicks button, show the dialog.
            ////saveFileDialog1.ShowDialog();

            //string Saved_File = "";
            //saveFileDialog1.InitialDirectory = "c:\\";
            //saveFileDialog1.InitialDirectory = "C:\\";
            //saveFileDialog1.Title = "Save a Text File";
            //saveFileDialog1.FileName = "";
            //saveFileDialog1.Filter = "Text Files|*.txt|All Files|*.*";

            //if (saveFileDialog1.ShowDialog() != DialogResult.Cancel)
            //{
            //    Saved_File = saveFileDialog1.FileName;
            //    output.SaveFile(Saved_File, RichTextBoxStreamType.PlainText);
            //}
        }

        private void saveConnectionDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Copy window location to app settings
            Settings.Default.WindowSize = this.Size;
            Settings.Default.WindowLocation = this.Location;
            Settings.Default.LastUsedIP = ip.Text;
            Settings.Default.Username = username.Text;

            // Save settings
            Settings.Default.Save();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Nessus NessusF = new Nessus();
            NessusF.Show();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            RBR_Main rbrnew = new RBR_Main();
            rbrnew.Show();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Nmap NmapF = new Nmap();
            NmapF.Show();
        }
    }
}
