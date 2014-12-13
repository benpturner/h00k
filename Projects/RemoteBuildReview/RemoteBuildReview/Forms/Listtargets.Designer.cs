namespace RemoteBuildReview
{
    partial class Listtargets
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
            this.testedtargets = new System.Windows.Forms.ListBox();
            this.totaltargetstested = new System.Windows.Forms.Label();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.subnets = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // testedtargets
            // 
            this.testedtargets.FormattingEnabled = true;
            this.testedtargets.Location = new System.Drawing.Point(27, 98);
            this.testedtargets.Name = "testedtargets";
            this.testedtargets.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.testedtargets.Size = new System.Drawing.Size(311, 290);
            this.testedtargets.TabIndex = 0;
            // 
            // totaltargetstested
            // 
            this.totaltargetstested.AutoSize = true;
            this.totaltargetstested.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.totaltargetstested.Location = new System.Drawing.Point(144, 75);
            this.totaltargetstested.Name = "totaltargetstested";
            this.totaltargetstested.Size = new System.Drawing.Size(57, 20);
            this.totaltargetstested.TabIndex = 1;
            this.totaltargetstested.Text = "label1";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Multiselect = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(27, 18);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 2;
            this.button1.Text = "Browse";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(34, 75);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 20);
            this.label1.TabIndex = 3;
            this.label1.Text = "Total Hosts:";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(72, 394);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(212, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "Highlight and Copy";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // subnets
            // 
            this.subnets.FormattingEnabled = true;
            this.subnets.Location = new System.Drawing.Point(344, 98);
            this.subnets.Name = "subnets";
            this.subnets.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.subnets.Size = new System.Drawing.Size(234, 290);
            this.subnets.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(395, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(138, 20);
            this.label2.TabIndex = 6;
            this.label2.Text = "Unique Subnets";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(355, 394);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(212, 23);
            this.button3.TabIndex = 7;
            this.button3.Text = "Highlight and Copy";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // Listtargets
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(590, 445);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.subnets);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.totaltargetstested);
            this.Controls.Add(this.testedtargets);
            this.Name = "Listtargets";
            this.Text = "TargetsList";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox testedtargets;
        private System.Windows.Forms.Label totaltargetstested;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ListBox subnets;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button3;
    }
}