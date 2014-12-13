using System;
using System.Management;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using RemoteBuildReview.Properties;
using System.Xml;
using CommonFunctions;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Security.Permissions;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.ServiceProcess;

namespace RemoteBuildReview
{

    public partial class RBR_Main : Form
    {
        public RBR_Main()
        {
            InitializeComponent();
            Application.EnableVisualStyles();

        }
        static DataSet dsfull = new DataSet();
        public DataSet1 testdataset = new DataSet1();
        List<Service> services = new List<Service>();
        public static ArrayList filesfolders = new ArrayList();

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        public string cacls;
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofdfull = new OpenFileDialog();
            ofdfull.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            ofdfull.InitialDirectory = @"C:\";
            ofdfull.Title = "Please select the rbr-full.xml file to open.";
            if (ofdfull.ShowDialog() == DialogResult.OK)
            {
                dsfull.ReadXml(ofdfull.FileName);
            } 
            
            OpenFileDialog ofdsam = new OpenFileDialog();
            //open file code
            ofdsam.Filter = "All files (*.*)|*.*";
            ofdsam.InitialDirectory = @"C:\";
            ofdsam.Title = "Please select the SAM file to open.";
            if (ofdsam.ShowDialog() == DialogResult.OK)
            { 
            
            }
            OpenFileDialog ofdsys = new OpenFileDialog();
            //open file code
            ofdsys.Filter = "All files (*.*)|*.*";
            ofdsys.InitialDirectory = @"C:\";
            ofdsys.Title = "Please select the SYSTEM file to open.";
            if (ofdsys.ShowDialog() == DialogResult.OK)
            {

                ProcessStartInfo psi3 = new ProcessStartInfo("c:\\samdump2.exe", ofdsys.FileName + " " + ofdsam.FileName);

                psi3.RedirectStandardOutput = true;
                psi3.RedirectStandardError = true;
                psi3.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                psi3.UseShellExecute = false;
                System.Diagnostics.Process reg3;
                reg3 = System.Diagnostics.Process.Start(psi3);
                using (System.IO.StreamReader myOutput = reg3.StandardOutput)
                {
                    hashes.Text = hashes.Text + myOutput.ReadToEnd();
                }
            }

            foreach (DataRow row in dsfull.Tables["software"].Rows) // Loop over the rows.
            {
                string name = row["name"].ToString();
                string version = row["version"].ToString();

                output.Text = output.Text + name + ": " + version + "\n";

            }

            foreach (DataRow row in dsfull.Tables["systeminfo"].Rows) // Loop over the rows.
            {
                string sysinfo = row["sysinfo"].ToString();
                string ipconfig = row["ipconfig"].ToString();
                string routes = row["routes"].ToString();

                userstxt.Text = userstxt.Text + ipconfig + "\n" + routes + "\n" + sysinfo + "\n";

            }

            foreach (DataRow row in dsfull.Tables["userandgroups"].Rows) // Loop over the rows.
            {
                string users = row["users"].ToString();
                string groups = row["groups"].ToString();
                string account = row["account"].ToString();

                userstxt.Text = userstxt.Text + users + "\n" + groups + "\n" + account + "\n";

            }





            string currentDir = Environment.CurrentDirectory;
            DirectoryInfo directory = new DirectoryInfo(currentDir);
            FileInfo file = new FileInfo("KB2MS.xml");
            string fullDirectory = directory.FullName;
            string fullFile = file.FullName;
            if (!fullFile.StartsWith(fullDirectory))
            {
               MessageBox.Show("Unable to make relative path" + fullDirectory + " " + fullFile);
            }

            else
            {
                ObjectQuery query2 = new ObjectQuery("SELECT * FROM Win32_QuickFixEngineering WHERE HotFixID LIKE 'KB%'");
                ManagementObjectSearcher searcher2 = new ManagementObjectSearcher(query2);

                string kb = "";
                string ms = "MS Number Not Found!";
                ArrayList patches = new ArrayList();

                foreach (DataRow row in dsfull.Tables["patches"].Rows) // Loop over the rows.
                {
                    string kb1 = row["kbnumber"].ToString();


                    XmlReader reader = XmlReader.Create(fullFile);
                    while (reader.Read())
                    {
                        if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "patch"))
                        {
                            if (reader.HasAttributes)
                            {
                                if (reader.GetAttribute("kbnumber") == kb1)
                                {
                                    ms = reader.GetAttribute("msnumber").ToString();
                                    patches.Add(ms);
                                }

                            }
                        }
                    }

                    kbnumbers.Text = kbnumbers.Text + kb1 + "\r\n";
                    ms = "MS Number Not Found!";
                }

                patches.Sort();
                patches.Reverse();
                foreach (string s in patches)
                {
                    msnumbers.Text = msnumbers.Text + s + "\r\n";
                }
            }


            //MessageBox.Show("Run");
            foreach (DataRow row in dsfull.Tables["services"].Rows) // Loop over the rows.
            {
                string sn = row["servicename"].ToString();
                string unquoted = row["unquoted"].ToString();
                string imagep = row["imagepath"].ToString();
                string perm = row["perms"].ToString();


                servicesimagepath.Text = servicesimagepath.Text + sn + "\n" + imagep + "\nIsUnquoted=" + unquoted + "\n";

                string[] values = perm.Split('|');
                foreach (string perm2 in values)
                {
                    if (!perm2.ToString().Contains("SYSTEM"))
                    {
                        if (!perm2.ToString().Contains("Administrators"))
                        {
                            servicesimagepath.Text = servicesimagepath.Text + perm2 + "\n";
                        }
                    }
                }

                servicesimagepath.Text = servicesimagepath.Text + "\n";

            }

        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About formAbout = new About();
            formAbout.Show();
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // When user clicks button, show the dialog.
            //saveFileDialog1.ShowDialog();

            string Saved_File = "";
            saveFileDialog1.InitialDirectory = "c:\\";
            saveFileDialog1.InitialDirectory = "C:\\";
            saveFileDialog1.Title = "Save a Text File";
            saveFileDialog1.FileName = "";
            saveFileDialog1.Filter = "Text Files|*.txt|All Files|*.*";

            if (saveFileDialog1.ShowDialog() != DialogResult.Cancel)
            {
                Saved_File = saveFileDialog1.FileName;
                output.SaveFile(Saved_File, RichTextBoxStreamType.PlainText);
            }

        }
        private void kBToMSConverterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //KB2MS_Form newKB2MS = new KB2MS_Form();
            //newKB2MS.Show();
        }

        private void saveConnectionDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            Nessus NessusViewer = new Nessus();
            // Create a new instance of the Form2 class

            // Show the settings form
            NessusViewer.Show();

        }

        private void clearbutton_Click(object sender, EventArgs e)
        {
            output.Text = "";
            servicesimagepath.Text = "";
            userstxt.Text = "";
            kbnumbers.Text = "";
            msnumbers.Text = "";
        }

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            

        }

        private void RBR_Main_Load(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            TestConnect TC = new TestConnect();
            TC.Show();
        }

        private void button7_Click(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {


        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            Form1 RR = new Form1();
            // Create a new instance of the Form2 class

            // Show the settings form
            RR.Show();
        }

        private void importServicesxmlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            



                    
                
            

        }

        private void toolsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {

            OpenFileDialog ofd4 = new OpenFileDialog();
            ofd4.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            ofd4.InitialDirectory = @"C:\";
            ofd4.Title = "Please select the rbr-full.xml file to open.";
            if (ofd4.ShowDialog() == DialogResult.OK)
            {
                dsfull.ReadXml(ofd4.FileName);
            }
            foreach (DataRow row in dsfull.Tables["systeminfo"].Rows) // Loop over the rows.
            {
                string sysinfo = row["sysinfo"].ToString();
                string ipconfig = row["ipconfig"].ToString();
                string routes = row["routes"].ToString();

                userstxt.Text = userstxt.Text + ipconfig + "\n" + routes + "\n" + sysinfo + "\n";

            }

        }
    }
}
