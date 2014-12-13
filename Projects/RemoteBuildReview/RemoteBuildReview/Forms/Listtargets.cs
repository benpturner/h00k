using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;
using System.Windows.Forms;
using System.Security.Permissions;
using System.Security;
using Novacode;
using System.Text.RegularExpressions;


namespace RemoteBuildReview
{
    public partial class Listtargets : Form
    {
        public Listtargets()
        {
            InitializeComponent();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            DataSet1 testdataset = new DataSet1();

            //open file code
            openFileDialog1.Filter = "nessus files (*.nessus)|*.nessus|All files (*.*)|*.*";
            openFileDialog1.InitialDirectory = @"C:\";
            openFileDialog1.Title = "Please select a nessus file to open.";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (String file in openFileDialog1.FileNames)
                {
                    // Create a PictureBox.
                    try
                    {
                        //open nessus file
                        XDocument xml = XDocument.Load(file);

                        //for each report host that is high
                        var queryhosts = from p in xml.Elements("NessusClientData_v2").Elements("Report").Elements("ReportHost")
                                         select p;
                        foreach (var record in queryhosts)
                        {
                            string hostname = record.Attribute("name").Value;
                            if (!testedtargets.Items.Contains(hostname))
                            {
                                testedtargets.Items.Add(hostname);
                            }

                        }
                        string totalcount = testedtargets.Items.Count.ToString();
                        totaltargetstested.Text = totalcount;

                    }
                    catch (SecurityException ex)
                    {
                        // The user lacks appropriate permissions to read files, discover paths, etc.
                        MessageBox.Show("Security error. Please contact your administrator for details.\n\n" +
                            "Error message: " + ex.Message + "\n\n" +
                            "Details (send to Support):\n\n" + ex.StackTrace
                        );
                    }
                    catch (Exception ex)
                    {
                        // Could not load the image - probably related to Windows file system permissions.
                        MessageBox.Show("Cannot display the image: " + file.Substring(file.LastIndexOf('\\'))
                            + ". You may not have permission to read the file, or " +
                            "it may be corrupt.\n\nReported error: " + ex.Message);
                    }
                }

            }

            else
            {
                MessageBox.Show("No file selected");
                return;
            }





            foreach (string o in testedtargets.Items)
            {

                Match match2 = Regex.Match(o, @"\d{1,3}\.\d{1,3}\.\d{1,3}");

                if (match2.Success)
                {
                    if (!subnets.Items.Contains(match2 + ".0/24"))
                    {
                        subnets.Items.Add(match2 + ".0/24");
                    }
                }
            }


        }



        private void button2_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < testedtargets.Items.Count; i++)
            {
                testedtargets.SetSelected(i, true);
            }
            string s = "";
            foreach (object o in testedtargets.SelectedItems)
            {
                s += o.ToString() + "  ";
            }
            Clipboard.SetText(s);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < subnets.Items.Count; i++)
            {
                subnets.SetSelected(i, true);
            }
            string s = "";
            foreach (object o in subnets.SelectedItems)
            {
                s += o.ToString() + "  ";
            }
            Clipboard.SetText(s);
        }
    }
}
