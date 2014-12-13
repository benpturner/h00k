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
//using Novacode;

namespace NmapParser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void parse_Click(object sender, EventArgs e)
        {

            openFileDialog1.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            openFileDialog1.InitialDirectory = @"C:\";
            openFileDialog1.Title = "Please select an XML file to open.";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (String file in openFileDialog1.FileNames)
                {
                    XDocument xml = XDocument.Load(openFileDialog1.FileName);

                    //for each report host
                    var query1 = from p in xml.Elements("nmaprun").Elements("host")
                                 select p;

                    foreach (var record1 in query1)
                    {

                        string address = record1.Element("address").Attribute("addr").Value;

                        //for each reportitem in host
                        var query2 = from p in xml.Elements("nmaprun").Elements("host").Elements("ports").Elements("port")
                                     where (string)p.Parent.Parent.Element("address").Attribute("addr") == address
                                     select p;

                        foreach (var record in query2)
                        {
                            string hostname = record.Parent.Parent.Element("address").Attribute("addr").Value.ToString();
                            string protocol = record.Attribute("protocol").Value.ToString();
                            string port = record.Attribute("portid").Value.ToString();


                            string state = null;
                            try
                            { state = record.Element("state").Attribute("state").Value.ToString(); }
                            catch { }

                            string name = null;
                            try
                            { name = record.Element("service").Attribute("name").Value.ToString(); } 
                            catch { }
                            
                            //host, port, protocol, service
                            dataGridView1.Rows.Add(hostname, port + " (" + state + ")", protocol, name);

                        }
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
