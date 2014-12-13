using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Collections;

namespace RemoteBuildReview
{
    public partial class Targetslist : Form
    {
        public Targetslist()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string kb = "";
            string ms = "MS Number Not Found!";

            string currentDir = Environment.CurrentDirectory;
            DirectoryInfo directory = new DirectoryInfo(currentDir);
            FileInfo file = new FileInfo("KB2MS.xml");
            string fullDirectory = directory.FullName;
            string fullFile = file.FullName;
            if (!fullFile.StartsWith(fullDirectory))
            {
                //kbs.Text = kbs.Text + fullDirectory + "\r\n";
                //kbs.Text = kbs.Text + fullFile + "\r\n";
                kbs.Text = kbs.Text + "Unable to make relative path";
            }
            ArrayList patches = new ArrayList();
            foreach (string line in kbs.Lines)
            {
                kb = line.ToString();
                XmlReader reader = XmlReader.Create(fullFile);
                while (reader.Read())
                {
                    if ((reader.NodeType == XmlNodeType.Element) && (reader.Name == "patch"))
                    {
                        if (reader.HasAttributes)
                        {
                            if (reader.GetAttribute("kbnumber") == kb)
                            {
                                ms = reader.GetAttribute("msnumber").ToString();
                                patches.Add(ms);
                            }

                        }
                    }
                }
                //kbs.Text = kbs.Text + kb + "\r\n";
                ms = "MS Number Not Found!";
            }

            patches.Sort();
            patches.Reverse();
            kbs.Text = kbs.Text + "Corresponsding MS Numbers\r\n";
            kbs.Text = kbs.Text + "==========================\r\n";
            foreach (string s in patches)
            {
                kbs.Text = kbs.Text + s + "\r\n";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
