namespace RemoteBuildReview
{
    partial class Nessus
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.reportbutton = new System.Windows.Forms.Button();
            this.dataGridView2 = new System.Windows.Forms.DataGridView();
            this.host = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ports = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.https = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.openbutton = new System.Windows.Forms.Button();
            this.chk_high = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.chk_low = new System.Windows.Forms.CheckBox();
            this.chk_info = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chk_critical = new System.Windows.Forms.CheckBox();
            this.chk_medium = new System.Windows.Forms.CheckBox();
            this.webbutton = new System.Windows.Forms.Button();
            this.findtxtbox = new System.Windows.Forms.TextBox();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.clearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.testedtargets = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.totaltargetstested = new System.Windows.Forms.Label();
            this.SelectAll = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(6, 27);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(398, 20);
            this.textBox1.TabIndex = 6;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Multiselect = true;
            // 
            // reportbutton
            // 
            this.reportbutton.Location = new System.Drawing.Point(497, 27);
            this.reportbutton.Name = "reportbutton";
            this.reportbutton.Size = new System.Drawing.Size(75, 20);
            this.reportbutton.TabIndex = 7;
            this.reportbutton.Text = "Report";
            this.reportbutton.UseVisualStyleBackColor = true;
            this.reportbutton.Click += new System.EventHandler(this.button2_Click);
            // 
            // dataGridView2
            // 
            this.dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView2.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.host,
            this.ports,
            this.https});
            this.dataGridView2.Location = new System.Drawing.Point(690, 53);
            this.dataGridView2.Name = "dataGridView2";
            this.dataGridView2.Size = new System.Drawing.Size(365, 659);
            this.dataGridView2.TabIndex = 10;
            // 
            // host
            // 
            this.host.HeaderText = "host";
            this.host.Name = "host";
            // 
            // ports
            // 
            this.ports.HeaderText = "ports";
            this.ports.Name = "ports";
            // 
            // https
            // 
            this.https.HeaderText = "https";
            this.https.Name = "https";
            // 
            // openbutton
            // 
            this.openbutton.Location = new System.Drawing.Point(413, 27);
            this.openbutton.Name = "openbutton";
            this.openbutton.Size = new System.Drawing.Size(75, 20);
            this.openbutton.TabIndex = 11;
            this.openbutton.Text = "Browse";
            this.openbutton.UseVisualStyleBackColor = true;
            this.openbutton.Click += new System.EventHandler(this.button5_Click);
            // 
            // chk_high
            // 
            this.chk_high.AutoSize = true;
            this.chk_high.Checked = true;
            this.chk_high.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_high.Location = new System.Drawing.Point(25, 41);
            this.chk_high.Name = "chk_high";
            this.chk_high.Size = new System.Drawing.Size(48, 17);
            this.chk_high.TabIndex = 12;
            this.chk_high.Text = "High";
            this.chk_high.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(1086, 106);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(63, 17);
            this.checkBox2.TabIndex = 13;
            this.checkBox2.Text = "Medium";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // chk_low
            // 
            this.chk_low.AutoSize = true;
            this.chk_low.Checked = true;
            this.chk_low.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_low.Location = new System.Drawing.Point(25, 87);
            this.chk_low.Name = "chk_low";
            this.chk_low.Size = new System.Drawing.Size(46, 17);
            this.chk_low.TabIndex = 14;
            this.chk_low.Text = "Low";
            this.chk_low.UseVisualStyleBackColor = true;
            // 
            // chk_info
            // 
            this.chk_info.AutoSize = true;
            this.chk_info.Checked = true;
            this.chk_info.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_info.Location = new System.Drawing.Point(25, 109);
            this.chk_info.Name = "chk_info";
            this.chk_info.Size = new System.Drawing.Size(44, 17);
            this.chk_info.TabIndex = 15;
            this.chk_info.Text = "Info";
            this.chk_info.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chk_critical);
            this.groupBox1.Controls.Add(this.chk_medium);
            this.groupBox1.Controls.Add(this.chk_info);
            this.groupBox1.Controls.Add(this.chk_low);
            this.groupBox1.Controls.Add(this.chk_high);
            this.groupBox1.Location = new System.Drawing.Point(1061, 53);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(87, 149);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Options";
            // 
            // chk_critical
            // 
            this.chk_critical.AutoSize = true;
            this.chk_critical.Checked = true;
            this.chk_critical.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_critical.Location = new System.Drawing.Point(25, 18);
            this.chk_critical.Name = "chk_critical";
            this.chk_critical.Size = new System.Drawing.Size(57, 17);
            this.chk_critical.TabIndex = 18;
            this.chk_critical.Text = "Critical";
            this.chk_critical.UseVisualStyleBackColor = true;
            this.chk_critical.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // chk_medium
            // 
            this.chk_medium.AutoSize = true;
            this.chk_medium.Checked = true;
            this.chk_medium.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chk_medium.Location = new System.Drawing.Point(25, 64);
            this.chk_medium.Name = "chk_medium";
            this.chk_medium.Size = new System.Drawing.Size(63, 17);
            this.chk_medium.TabIndex = 17;
            this.chk_medium.Text = "Medium";
            this.chk_medium.UseVisualStyleBackColor = true;
            // 
            // webbutton
            // 
            this.webbutton.Location = new System.Drawing.Point(690, 27);
            this.webbutton.Name = "webbutton";
            this.webbutton.Size = new System.Drawing.Size(125, 20);
            this.webbutton.TabIndex = 17;
            this.webbutton.Text = "Get me Web Services";
            this.webbutton.UseVisualStyleBackColor = true;
            this.webbutton.Click += new System.EventHandler(this.webbutton_Click);
            // 
            // findtxtbox
            // 
            this.findtxtbox.Location = new System.Drawing.Point(578, 27);
            this.findtxtbox.Name = "findtxtbox";
            this.findtxtbox.Size = new System.Drawing.Size(106, 20);
            this.findtxtbox.TabIndex = 18;
            this.findtxtbox.Text = "Find.....";
            this.findtxtbox.TextChanged += new System.EventHandler(this.findtxtbox_TextChanged);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(5, 53);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(684, 659);
            this.dataGridView1.TabIndex = 19;
            this.dataGridView1.Tag = "XC";
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1227, 24);
            this.menuStrip1.TabIndex = 20;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.clearToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // clearToolStripMenuItem
            // 
            this.clearToolStripMenuItem.Name = "clearToolStripMenuItem";
            this.clearToolStripMenuItem.Size = new System.Drawing.Size(99, 22);
            this.clearToolStripMenuItem.Text = "Clear";
            this.clearToolStripMenuItem.Click += new System.EventHandler(this.clearToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(40, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(987, 27);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(153, 20);
            this.button2.TabIndex = 22;
            this.button2.Text = "Export to C:\\Vulns.xml";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click_1);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(821, 27);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(123, 23);
            this.button1.TabIndex = 23;
            this.button1.Text = "To CSV / Excel";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // testedtargets
            // 
            this.testedtargets.FormattingEnabled = true;
            this.testedtargets.Location = new System.Drawing.Point(1064, 267);
            this.testedtargets.Name = "testedtargets";
            this.testedtargets.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.testedtargets.Size = new System.Drawing.Size(141, 95);
            this.testedtargets.TabIndex = 24;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1061, 251);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 13);
            this.label1.TabIndex = 25;
            this.label1.Text = "Total Targets Tested:";
            // 
            // totaltargetstested
            // 
            this.totaltargetstested.AutoSize = true;
            this.totaltargetstested.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.totaltargetstested.Location = new System.Drawing.Point(1170, 251);
            this.totaltargetstested.Name = "totaltargetstested";
            this.totaltargetstested.Size = new System.Drawing.Size(0, 16);
            this.totaltargetstested.TabIndex = 26;
            // 
            // SelectAll
            // 
            this.SelectAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SelectAll.Location = new System.Drawing.Point(1072, 369);
            this.SelectAll.Name = "SelectAll";
            this.SelectAll.Size = new System.Drawing.Size(98, 23);
            this.SelectAll.TabIndex = 27;
            this.SelectAll.Text = "Select all";
            this.SelectAll.UseVisualStyleBackColor = true;
            this.SelectAll.Click += new System.EventHandler(this.SelectAll_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(1067, 480);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(75, 23);
            this.button3.TabIndex = 28;
            this.button3.Text = "TargetsList";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // Nessus
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1227, 745);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.SelectAll);
            this.Controls.Add(this.totaltargetstested);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.testedtargets);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.findtxtbox);
            this.Controls.Add(this.webbutton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.openbutton);
            this.Controls.Add(this.dataGridView2);
            this.Controls.Add(this.reportbutton);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Nessus";
            this.Text = "Nessus Viewer";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView2)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button reportbutton;
        private System.Windows.Forms.DataGridView dataGridView2;
        private System.Windows.Forms.DataGridViewTextBoxColumn host;
        private System.Windows.Forms.DataGridViewTextBoxColumn ports;
        private System.Windows.Forms.DataGridViewTextBoxColumn https;
        private System.Windows.Forms.Button openbutton;
        private System.Windows.Forms.CheckBox chk_high;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox chk_low;
        private System.Windows.Forms.CheckBox chk_info;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chk_medium;
        private System.Windows.Forms.Button webbutton;
        private System.Windows.Forms.TextBox findtxtbox;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem clearToolStripMenuItem;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.CheckBox chk_critical;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListBox testedtargets;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label totaltargetstested;
        private System.Windows.Forms.Button SelectAll;
        private System.Windows.Forms.Button button3;
    }
}