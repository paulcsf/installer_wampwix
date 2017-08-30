namespace SuiteEngConfNS
{
    partial class SuiteEngConf
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
            this.txtip = new System.Windows.Forms.TextBox();
            this.txtdbname = new System.Windows.Forms.TextBox();
            this.txtuser = new System.Windows.Forms.TextBox();
            this.txtpass = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtpassconf = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtfname = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.btn_fname = new System.Windows.Forms.Button();
            this.Lbl_error = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.btn_start = new System.Windows.Forms.Button();
            this.btn_stop = new System.Windows.Forms.Button();
            this.serviceCntl = new System.ServiceProcess.ServiceController();
            this.label8 = new System.Windows.Forms.Label();
            this.txtservicename = new System.Windows.Forms.TextBox();
            this.txtstatus = new System.Windows.Forms.TextBox();
            this.btn_testdb = new System.Windows.Forms.Button();
            this.txtpasshidden = new System.Windows.Forms.TextBox();
            this.btn_SaveConfig = new System.Windows.Forms.Button();
            this.txtdbport = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtip
            // 
            this.txtip.Location = new System.Drawing.Point(137, 148);
            this.txtip.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtip.Name = "txtip";
            this.txtip.Size = new System.Drawing.Size(169, 22);
            this.txtip.TabIndex = 0;
            // 
            // txtdbname
            // 
            this.txtdbname.Location = new System.Drawing.Point(137, 209);
            this.txtdbname.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtdbname.Name = "txtdbname";
            this.txtdbname.Size = new System.Drawing.Size(169, 22);
            this.txtdbname.TabIndex = 1;
            // 
            // txtuser
            // 
            this.txtuser.Location = new System.Drawing.Point(137, 241);
            this.txtuser.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtuser.Name = "txtuser";
            this.txtuser.Size = new System.Drawing.Size(169, 22);
            this.txtuser.TabIndex = 2;
            // 
            // txtpass
            // 
            this.txtpass.Location = new System.Drawing.Point(137, 273);
            this.txtpass.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtpass.Name = "txtpass";
            this.txtpass.PasswordChar = '*';
            this.txtpass.Size = new System.Drawing.Size(169, 22);
            this.txtpass.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(61, 151);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 17);
            this.label1.TabIndex = 4;
            this.label1.Text = "Server IP";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 212);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(110, 17);
            this.label2.TabIndex = 5;
            this.label2.Text = "Database Name";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(72, 244);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 17);
            this.label3.TabIndex = 6;
            this.label3.Text = "User ID";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(59, 276);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 17);
            this.label4.TabIndex = 7;
            this.label4.Text = "Password";
            // 
            // txtpassconf
            // 
            this.txtpassconf.Location = new System.Drawing.Point(137, 305);
            this.txtpassconf.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtpassconf.Name = "txtpassconf";
            this.txtpassconf.PasswordChar = '*';
            this.txtpassconf.Size = new System.Drawing.Size(169, 22);
            this.txtpassconf.TabIndex = 8;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 308);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(121, 17);
            this.label5.TabIndex = 9;
            this.label5.Text = "Confirm Password";
            // 
            // txtfname
            // 
            this.txtfname.Location = new System.Drawing.Point(137, 47);
            this.txtfname.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtfname.Name = "txtfname";
            this.txtfname.ReadOnly = true;
            this.txtfname.Size = new System.Drawing.Size(575, 22);
            this.txtfname.TabIndex = 10;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(57, 50);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 17);
            this.label6.TabIndex = 11;
            this.label6.Text = "File Name";
            // 
            // btn_fname
            // 
            this.btn_fname.Location = new System.Drawing.Point(137, 118);
            this.btn_fname.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_fname.Name = "btn_fname";
            this.btn_fname.Size = new System.Drawing.Size(47, 22);
            this.btn_fname.TabIndex = 12;
            this.btn_fname.Text = "***";
            this.btn_fname.UseVisualStyleBackColor = true;
            this.btn_fname.Visible = false;
            this.btn_fname.Click += new System.EventHandler(this.btn_fname_Click);
            // 
            // Lbl_error
            // 
            this.Lbl_error.Location = new System.Drawing.Point(133, 352);
            this.Lbl_error.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.Lbl_error.Name = "Lbl_error";
            this.Lbl_error.Size = new System.Drawing.Size(576, 220);
            this.Lbl_error.TabIndex = 13;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(28, 82);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(99, 17);
            this.label7.TabIndex = 14;
            this.label7.Text = "Service Status";
            // 
            // btn_start
            // 
            this.btn_start.Location = new System.Drawing.Point(335, 79);
            this.btn_start.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_start.Name = "btn_start";
            this.btn_start.Size = new System.Drawing.Size(76, 25);
            this.btn_start.TabIndex = 16;
            this.btn_start.Text = "Start";
            this.btn_start.UseVisualStyleBackColor = true;
            this.btn_start.Click += new System.EventHandler(this.btn_start_Click);
            // 
            // btn_stop
            // 
            this.btn_stop.Location = new System.Drawing.Point(419, 79);
            this.btn_stop.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_stop.Name = "btn_stop";
            this.btn_stop.Size = new System.Drawing.Size(76, 25);
            this.btn_stop.TabIndex = 17;
            this.btn_stop.Text = "Stop";
            this.btn_stop.UseVisualStyleBackColor = true;
            this.btn_stop.Click += new System.EventHandler(this.btn_stop_Click);
            // 
            // serviceCntl
            // 
            this.serviceCntl.ServiceName = "DiagEng";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(31, 18);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(96, 17);
            this.label8.TabIndex = 19;
            this.label8.Text = "Service Name";
            // 
            // txtservicename
            // 
            this.txtservicename.Location = new System.Drawing.Point(137, 15);
            this.txtservicename.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtservicename.Name = "txtservicename";
            this.txtservicename.ReadOnly = true;
            this.txtservicename.Size = new System.Drawing.Size(575, 22);
            this.txtservicename.TabIndex = 18;
            // 
            // txtstatus
            // 
            this.txtstatus.Location = new System.Drawing.Point(137, 79);
            this.txtstatus.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtstatus.Name = "txtstatus";
            this.txtstatus.ReadOnly = true;
            this.txtstatus.Size = new System.Drawing.Size(169, 22);
            this.txtstatus.TabIndex = 20;
            // 
            // btn_testdb
            // 
            this.btn_testdb.Location = new System.Drawing.Point(335, 148);
            this.btn_testdb.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_testdb.Name = "btn_testdb";
            this.btn_testdb.Size = new System.Drawing.Size(160, 25);
            this.btn_testdb.TabIndex = 21;
            this.btn_testdb.Text = "Test Database";
            this.btn_testdb.UseVisualStyleBackColor = true;
            this.btn_testdb.Click += new System.EventHandler(this.btn_testdb_Click);
            // 
            // txtpasshidden
            // 
            this.txtpasshidden.Enabled = false;
            this.txtpasshidden.Location = new System.Drawing.Point(324, 253);
            this.txtpasshidden.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtpasshidden.Name = "txtpasshidden";
            this.txtpasshidden.PasswordChar = '*';
            this.txtpasshidden.Size = new System.Drawing.Size(169, 22);
            this.txtpasshidden.TabIndex = 22;
            this.txtpasshidden.Visible = false;
            // 
            // btn_SaveConfig
            // 
            this.btn_SaveConfig.Location = new System.Drawing.Point(335, 180);
            this.btn_SaveConfig.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btn_SaveConfig.Name = "btn_SaveConfig";
            this.btn_SaveConfig.Size = new System.Drawing.Size(160, 25);
            this.btn_SaveConfig.TabIndex = 23;
            this.btn_SaveConfig.Text = "Save Configuration";
            this.btn_SaveConfig.UseVisualStyleBackColor = true;
            this.btn_SaveConfig.Click += new System.EventHandler(this.btn_SaveConfig_Click);
            // 
            // txtdbport
            // 
            this.txtdbport.Location = new System.Drawing.Point(136, 180);
            this.txtdbport.Margin = new System.Windows.Forms.Padding(4);
            this.txtdbport.Name = "txtdbport";
            this.txtdbport.Size = new System.Drawing.Size(169, 22);
            this.txtdbport.TabIndex = 24;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(48, 183);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(80, 17);
            this.label9.TabIndex = 25;
            this.label9.Text = "Server Port";
            // 
            // SuiteEngConf
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(729, 583);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txtdbport);
            this.Controls.Add(this.btn_SaveConfig);
            this.Controls.Add(this.txtpasshidden);
            this.Controls.Add(this.btn_testdb);
            this.Controls.Add(this.txtstatus);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.txtservicename);
            this.Controls.Add(this.btn_stop);
            this.Controls.Add(this.btn_start);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.Lbl_error);
            this.Controls.Add(this.btn_fname);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtfname);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtpassconf);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtpass);
            this.Controls.Add(this.txtuser);
            this.Controls.Add(this.txtdbname);
            this.Controls.Add(this.txtip);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "SuiteEngConf";
            this.Text = "Diagnostics Engine Configuration";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtip;
        private System.Windows.Forms.TextBox txtdbname;
        private System.Windows.Forms.TextBox txtuser;
        private System.Windows.Forms.TextBox txtpass;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtpassconf;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtfname;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btn_fname;
        private System.Windows.Forms.Label Lbl_error;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btn_start;
        private System.Windows.Forms.Button btn_stop;
        private System.ServiceProcess.ServiceController serviceCntl;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtservicename;
        private System.Windows.Forms.TextBox txtstatus;
        private System.Windows.Forms.Button btn_testdb;
        private System.Windows.Forms.TextBox txtpasshidden;
        private System.Windows.Forms.Button btn_SaveConfig;
        private System.Windows.Forms.TextBox txtdbport;
        private System.Windows.Forms.Label label9;
    }
}

