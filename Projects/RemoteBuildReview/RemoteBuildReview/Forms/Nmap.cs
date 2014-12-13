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


namespace RemoteBuildReview
{
    public partial class Nmap : Form
    {
        public Nmap()
        {
            InitializeComponent();
            //this.dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        }

        List<Vulnerability> vulns = new List<Vulnerability>();

        private void button2_Click(object sender, EventArgs e)
        {



        }
        private void PrintValues(DataTable table)
        {

        }
        private void button5_Click(object sender, EventArgs e)
        {
            //open file code
            openFileDialog1.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            openFileDialog1.InitialDirectory = @"C:\";
            openFileDialog1.Title = "Please select an XML file to open.";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;

                XDocument xml = XDocument.Load(textBox1.Text);

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
                        string state = record.Element("state").Attribute("state").Value.ToString();
                        string name = record.Element("service").Attribute("name").Value.ToString();
                        //host, port, protocol, service
                        dataGridView2.Rows.Add(hostname, port + " (" + state + ")", protocol, name);

                    }
                }
            }


        }

        private void webbutton_Click(object sender, EventArgs e)
        {
        }

        private void findtxtbox_TextChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            DataSet1 testdataset = new DataSet1();

            foreach (var test in vulns)
            {

                testdataset.Tables["Vulnerabilities"].Rows.Add(
                test.VulnName,
                test.Severity,
                test.Host,
                test.Port,
                test.Evidence,
                test.Hostport
                );

            }

            testdataset.Tables["Vulnerabilities"].WriteXml("C:\\vulns.xml");

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {


        }
        private void ToCsV(DataGridView dGV, string filename)
        {
            string stOutput = "";
            // Export titles:
            string sHeaders = "";

            for (int j = 0; j < dGV.Columns.Count; j++)
                sHeaders = sHeaders.ToString() + Convert.ToString(dGV.Columns[j].HeaderText) + "\t";
            stOutput += sHeaders + "\r\n";
            // Export data.
            for (int i = 0; i < dGV.RowCount - 1; i++)
            {
                string stLine = "";
                for (int j = 0; j < dGV.Rows[i].Cells.Count; j++)
                    stLine = stLine.ToString() + Convert.ToString(dGV.Rows[i].Cells[j].Value) + "\t";
                stOutput += stLine + "\r\n";
            }
            Encoding utf16 = Encoding.GetEncoding(1254);
            byte[] output = utf16.GetBytes(stOutput);
            FileStream fs = new FileStream(filename, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(output, 0, output.Length); //write the encoded file
            bw.Flush();
            bw.Close();
            fs.Close();
        }

        private void copyAlltoClipboard()
        {
                   }

        private void button1_Click_1(object sender, EventArgs e)
        {
        }

        private void SelectAll_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            Listtargets tst = new Listtargets();
            tst.Show();
        }

    }

}

//var testme = testdataset.Tables["Vulnerabilities"].Select("VulnName like '%" + vulnname + "%'");
////testme is now a row in the table or Multiple rows remember
//foreach (DataRow item in testme){

//    foreach (var item2 in item.ItemArray) { 
//    MessageBox.Show(item2.ToString());
//    }
////
//}
////MessageBox.Show(testme.ToString());
//// then you need to do some mapping 
////MessageBox.Show("woop");