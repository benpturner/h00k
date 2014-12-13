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
    public partial class Nessus : Form
    {
        public Nessus()
        {
            InitializeComponent();
            //this.dataGridView1.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        }

        List<Vulnerability> vulns = new List<Vulnerability>();

        private void button2_Click(object sender, EventArgs e)
        {
            // Modify to suit your machine:
            string fileName = @"C:\Results.docx";

            // Create a document in memory:
            var doc = DocX.Create(fileName);

            foreach (DataGridViewRow row in this.dataGridView1.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value == null || cell.Value.Equals(""))
                    {
                        continue;
                    }
                    doc.InsertParagraph(cell.Value.ToString());
                }
                doc.InsertParagraph("");
            }

            // Save to the output directory:
            doc.Save();

            // Open in Word:
            Process.Start("WINWORD.EXE", fileName);


        }
        private void PrintValues(DataTable table)
        {
            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn column in table.Columns)
                {
                    MessageBox.Show(row[column].ToString());
                }
            }
        }
        private void button5_Click(object sender, EventArgs e)
        {
            
            
            DataSet1 testdataset = new DataSet1();

            //open file code
            openFileDialog1.Filter = "nessus files (*.nessus)|*.nessus|All files (*.*)|*.*";
            openFileDialog1.InitialDirectory = @"C:\";
            openFileDialog1.Title = "Please select a nessus file to open.";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
                // Read the files
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

                        //get high vulns out
                        if (chk_critical.Checked)
                        {
                            //for each report host that is high
                            var query1 = from p in xml.Elements("NessusClientData_v2").Elements("Report").Elements("ReportHost")
                                         select p;
                            foreach (var record1 in query1)
                            {
                                string hostname = record1.Attribute("name").Value;
                                //for each reportitem in the nessus report for that host
                                var query2 = from p in xml.Elements("NessusClientData_v2").Elements("Report").Elements("ReportHost").Elements("ReportItem")
                                             where (int)p.Attribute("severity") == 4 && (string)p.Parent.Attribute("name") == hostname
                                             select p;


                                foreach (var record in query2)
                                {
                                    string isvalid = "1";
                                    string severity = "4 Critical";
                                    string vulnname = record.Element("plugin_name").Value.ToString();
                                    string port = record.Attribute("port").Value.ToString();
                                    string pluginoutput = record.Element("plugin_output").ElementValueNull();
                                    string solutions = record.Element("solution").ElementValueNull();

                                    foreach (var item in vulns)
                                    {

                                        if (item.VulnName == vulnname)
                                        {
                                            isvalid = "2";
                                            item.Evidence = item.Evidence + "\n\n" + pluginoutput;
                                            item.Hostport = item.Hostport + ", " + hostname + ":" + port;
                                             
                                            if (!item.Host.Contains(hostname))
                                            {
                                                item.Host = item.Host + ", " + hostname;
                                            }

                                            if (!item.Port.Contains(port))
                                            {
                                                item.Port = item.Port + ", " + port;
                                            }
                                        }

                                    }

                                    if (isvalid == "1")
                                    {
                                        vulns.Add(new Vulnerability()
                                        {
                                            VulnName = vulnname,
                                            Severity = severity,
                                            Host = hostname,
                                            Port = port,
                                            Evidence = pluginoutput,
                                            Hostport = hostname + ":" + port,
                                            Solution = solutions

                                        });

                                    }

                                }
                            }
                        }
                        //get high vulns out
                        if (chk_high.Checked)
                        {
                            //for each report host that is high
                            var query1 = from p in xml.Elements("NessusClientData_v2").Elements("Report").Elements("ReportHost")
                                         select p;
                            foreach (var record1 in query1)
                            {
                                string hostname = record1.Attribute("name").Value;
                                //for each reportitem in the nessus report for that host
                                var query2 = from p in xml.Elements("NessusClientData_v2").Elements("Report").Elements("ReportHost").Elements("ReportItem")
                                             where (int)p.Attribute("severity") == 3 && (string)p.Parent.Attribute("name") == hostname
                                             select p;


                                foreach (var record in query2)
                                {
                                    string isvalid = "1";
                                    string severity = "3 High";
                                    string vulnname = record.Element("plugin_name").Value.ToString();
                                    string port = record.Attribute("port").Value.ToString();
                                    string pluginoutput = record.Element("plugin_output").ElementValueNull();
                                    string solutions = record.Element("solution").ElementValueNull();
                                    foreach (var item in vulns)
                                    {

                                        if (item.VulnName == vulnname)
                                        {
                                            isvalid = "2";
                                            item.Evidence = item.Evidence + "\n\n" + pluginoutput;
                                            item.Hostport = item.Hostport + ", " + hostname + ":" + port;
                                            if (!item.Host.Contains(hostname))
                                            {
                                                item.Host = item.Host + ", " + hostname;
                                            }

                                            if (!item.Port.Contains(port))
                                            {
                                                item.Port = item.Port + ", " + port;
                                            }
                                        }

                                    }

                                    if (isvalid == "1")
                                    {
                                        vulns.Add(new Vulnerability()
                                        {
                                            VulnName = vulnname,
                                            Severity = severity,
                                            Host = hostname,
                                            Port = port,
                                            Evidence = pluginoutput,
                                            Hostport = hostname + ":" + port,
                                            Solution = solutions

                                        });

                                    }

                                }
                            }
                        }

                        if (chk_medium.Checked)
                        {
                            //for each report host that is high
                            var query1 = from p in xml.Elements("NessusClientData_v2").Elements("Report").Elements("ReportHost")
                                         select p;
                            foreach (var record1 in query1)
                            {
                                string hostname = record1.Attribute("name").Value;
                                //for each reportitem in the nessus report for that host
                                var query2 = from p in xml.Elements("NessusClientData_v2").Elements("Report").Elements("ReportHost").Elements("ReportItem")
                                             where (int)p.Attribute("severity") == 2 && (string)p.Parent.Attribute("name") == hostname
                                             select p;


                                foreach (var record in query2)
                                {
                                    string isvalid = "1";
                                    string severity = "2 Medium";
                                    string vulnname = record.Element("plugin_name").Value.ToString();
                                    string port = record.Attribute("port").Value.ToString();
                                    string pluginoutput = record.Element("plugin_output").ElementValueNull();
                                    string solutions = record.Element("solution").ElementValueNull();
                                    foreach (var item in vulns)
                                    {

                                        if (item.VulnName == vulnname)
                                        {
                                            isvalid = "2";
                                            item.Evidence = item.Evidence + "\n\n" + pluginoutput;
                                            item.Hostport = item.Hostport + ", " + hostname + ":" + port;
                                            if (!item.Host.Contains(hostname))
                                            {
                                                item.Host = item.Host + ", " + hostname;
                                            }

                                            if (!item.Port.Contains(port))
                                            {
                                                item.Port = item.Port + ", " + port;
                                            }
                                        }

                                    }
                                    if (isvalid == "1")
                                    {
                                        vulns.Add(new Vulnerability()
                                        {
                                            VulnName = vulnname,
                                            Severity = severity,
                                            Host = hostname,
                                            Port = port,
                                            Evidence = pluginoutput,
                                            Hostport = hostname + ":" + port,
                                            Solution = solutions

                                        });

                                    }
                                }
                            }
                        }
                        if (chk_low.Checked)
                        {
                            //for each report host that is high
                            var query1 = from p in xml.Elements("NessusClientData_v2").Elements("Report").Elements("ReportHost")
                                         select p;
                            foreach (var record1 in query1)
                            {
                                string hostname = record1.Attribute("name").Value;
                                //for each reportitem in the nessus report for that host
                                var query2 = from p in xml.Elements("NessusClientData_v2").Elements("Report").Elements("ReportHost").Elements("ReportItem")
                                             where (int)p.Attribute("severity") == 1 && (string)p.Parent.Attribute("name") == hostname
                                             select p;


                                foreach (var record in query2)
                                {
                                    string isvalid = "1";
                                    string severity = "1 Low";
                                    string vulnname = record.Element("plugin_name").Value.ToString();
                                    string port = record.Attribute("port").Value.ToString();
                                    string pluginoutput = record.Element("plugin_output").ElementValueNull();
                                    string solutions = record.Element("solution").ElementValueNull();
                                    foreach (var item in vulns)
                                    {

                                        if (item.VulnName == vulnname)
                                        {
                                            isvalid = "2";
                                            item.Evidence = item.Evidence + "\n\n" + pluginoutput;
                                            item.Hostport = item.Hostport + ", " + hostname + ":" + port;
                                            if (!item.Host.Contains(hostname))
                                            {
                                                item.Host = item.Host + ", " + hostname;
                                            }

                                            if (!item.Port.Contains(port))
                                            {
                                                item.Port = item.Port + ", " + port;
                                            }
                                        }

                                    }
                                    if (isvalid == "1")
                                    {
                                        vulns.Add(new Vulnerability()
                                        {
                                            VulnName = vulnname,
                                            Severity = severity,
                                            Host = hostname,
                                            Port = port,
                                            Evidence = pluginoutput,
                                            Hostport = hostname + ":" + port,
                                            Solution = solutions

                                        });

                                    }
                                }
                            }
                        }
                        if (chk_info.Checked)
                        {
                            //for each report host that is high
                            var query1 = from p in xml.Elements("NessusClientData_v2").Elements("Report").Elements("ReportHost")
                                         select p;
                            foreach (var record1 in query1)
                            {
                                string hostname = record1.Attribute("name").Value;
                                //for each reportitem in the nessus report for that host
                                var query2 = from p in xml.Elements("NessusClientData_v2").Elements("Report").Elements("ReportHost").Elements("ReportItem")
                                             where (int)p.Attribute("severity") == 0 && (string)p.Parent.Attribute("name") == hostname
                                             select p;


                                foreach (var record in query2)
                                {
                                    string isvalid = "1";
                                    string severity = "0 Lnfo";
                                    string vulnname = record.Element("plugin_name").Value.ToString();
                                    string port = record.Attribute("port").Value.ToString();
                                    string pluginoutput = record.Element("plugin_output").ElementValueNull();
                                    string solutions = record.Element("solution").ElementValueNull();
                                    foreach (var item in vulns)
                                    {

                                        if (item.VulnName == vulnname)
                                        {
                                            isvalid = "2";
                                            item.Evidence = item.Evidence + "\n\n" + pluginoutput;
                                            item.Hostport = item.Hostport + ", " + hostname + ":" + port;
                                            if (!item.Host.Contains(hostname))
                                            {
                                                item.Host = item.Host + ", " + hostname;
                                            }

                                            if (!item.Port.Contains(port))
                                            {
                                                item.Port = item.Port + ", " + port;
                                            }
                                        }

                                    }
                                    if (isvalid == "1")
                                    {
                                        vulns.Add(new Vulnerability()
                                        {
                                            VulnName = vulnname,
                                            Severity = severity,
                                            Host = hostname,
                                            Port = port,
                                            Evidence = pluginoutput,
                                            Hostport = hostname + ":" + port,
                                            Solution = solutions

                                        });

                                    }
                                }
                            }
                        }

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


            
            //customers = customers.OrderBy(customer => customer.Name).ToList();

            vulns.Sort((left, right) => String.Compare(left.Severity, right.Severity, StringComparison.CurrentCulture));
            vulns.Reverse();
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = vulns;

        }

        private void webbutton_Click(object sender, EventArgs e)
        {
            //get me webservices
            //open nessus file
            XDocument xml = XDocument.Load(textBox1.Text);

            //for each report host
            var query1 = from p in xml.Elements("NessusClientData_v2").Elements("Report").Elements("ReportHost")
                         select p;
            foreach (var record1 in query1)
            {
                string hostname = record1.Attribute("name").Value;
                //for each reportitem in host
                var query2 = from p in xml.Elements("NessusClientData_v2").Elements("Report").Elements("ReportHost").Elements("ReportItem")
                             where (string)p.Attribute("svc_name") == "www" && (string)p.Parent.Attribute("name") == hostname
                             select p;


                foreach (var record in query2)
                {

                    string isvalid = "1";
                    string name = record.Element("plugin_name").Value.ToString();
                    string port = record.Attribute("port").Value.ToString();
                    string pluginoutput = record.Element("plugin_output").ElementValueNull();

                    foreach (DataGridViewRow row in this.dataGridView2.Rows)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            if (cell.Value == null || cell.Value.Equals(""))
                            {
                                continue;
                            }
                            if (cell.Value.Equals(name))
                            {
                                isvalid = "2";

                                if (!row.Cells[1].Value.ToString().Contains(hostname))
                                {
                                    row.Cells[1].Value = row.Cells[1].Value + hostname;
                                }
                                if (!row.Cells[2].Value.ToString().Contains(port))
                                {
                                    row.Cells[2].Value = row.Cells[2].Value + port;
                                }

                            }
                        }
                    }

                    if (isvalid == "1")
                    {
                        dataGridView2.Rows.Add(hostname, port);
                    }
                }
            }




        }

        private void findtxtbox_TextChanged(object sender, EventArgs e)
        {

            dataGridView1.DataSource = null;
            dataGridView1.DataSource = vulns;

            if (findtxtbox.Text != string.Empty)
            {
                var searchResult = vulns.Where(a => a.VulnName.Contains(findtxtbox.Text));
                dataGridView1.DataSource = searchResult.ToList();
            }
            else
            {
                foreach (var vuln in vulns)
                {
                    dataGridView1.DataSource = null;
                    dataGridView1.DataSource = vulns;
                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in this.dataGridView1.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Value = "";

                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = vulns;

            //making sure all columns are sortable
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.Automatic;
            }

            // Check which column is selected, otherwise set NewColumn to null.
            DataGridViewColumn newColumn =
                dataGridView1.Columns.GetColumnCount(
                DataGridViewElementStates.Selected) == 1 ?
                dataGridView1.SelectedColumns[0] : null;

            DataGridViewColumn oldColumn = dataGridView1.SortedColumn;
            ListSortDirection direction;

            // If oldColumn is null, then the DataGridView is not currently sorted. 
            if (oldColumn != null)
            {
                // Sort the same column again, reversing the SortOrder. 
                if (oldColumn == newColumn &&
                    dataGridView1.SortOrder == SortOrder.Ascending)
                {
                    direction = ListSortDirection.Descending;
                }
                else
                {
                    // Sort a new column and remove the old SortGlyph.
                    direction = ListSortDirection.Ascending;
                    oldColumn.HeaderCell.SortGlyphDirection = SortOrder.None;
                }
            }
            else
            {
                direction = ListSortDirection.Ascending;
            }

            // If no column has been selected, display an error dialog  box. 
            if (newColumn == null)
            {
                MessageBox.Show("Select a single column and try again.",
                    "Error: Invalid Selection", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            else
            {
                dataGridView1.Sort(newColumn, direction);
                newColumn.HeaderCell.SortGlyphDirection =
                    direction == ListSortDirection.Ascending ?
                    SortOrder.Ascending : SortOrder.Descending;
            }

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
            dataGridView1.SelectAll();
            DataObject dataObj = dataGridView1.GetClipboardContent();
            if (dataObj != null)
                Clipboard.SetDataObject(dataObj);
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

            foreach (DataGridViewRow row in this.dataGridView1.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    //if (!cell.Size.IsEmpty) MessageBox.Show(cell.Value.ToString()); // note the ! operator
                    string val = cell.Value.ToString();
                    string test = val.Replace("\n", "");
                    cell.Value = test;

                }
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel Documents (*.xls)|*.xls";
            sfd.FileName = "export.xls";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                //ToCsV(dataGridView1, @"c:\export.xls");
                ToCsV(dataGridView1, sfd.FileName); // Here dataGridview1 is your grid view name 
            }


            //copyAlltoClipboard();
            //Microsoft.Office.Interop.Excel.Application xlexcel;
            //Microsoft.Office.Interop.Excel.Workbook xlWorkBook;
            //Microsoft.Office.Interop.Excel.Worksheet xlWorkSheet;
            //object misValue = System.Reflection.Missing.Value;
            //xlexcel = new Microsoft.Office.Interop.Excel.Application();
            //xlexcel.Visible = true;
            //xlWorkBook = xlexcel.Workbooks.Add(misValue);
            //xlWorkSheet = (Microsoft.Office.Interop.Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
            //Microsoft.Office.Interop.Excel.Range CR = (Microsoft.Office.Interop.Excel.Range)xlWorkSheet.Cells[1, 1];
            //CR.Select();
            //xlWorkSheet.PasteSpecial(CR, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, true);
        }

        private void SelectAll_Click(object sender, EventArgs e)
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