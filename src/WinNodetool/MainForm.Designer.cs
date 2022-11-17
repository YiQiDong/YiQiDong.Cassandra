namespace WinNodetool
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.gbOperate = new System.Windows.Forms.GroupBox();
            this.btnRun = new System.Windows.Forms.Button();
            this.btnSelectCassandraContainerDir = new System.Windows.Forms.Button();
            this.txtArguments = new System.Windows.Forms.TextBox();
            this.btnSelectCassandraImageDir = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtCassandraContainerDir = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtCassandraImageDir = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.gbResult = new System.Windows.Forms.GroupBox();
            this.txtLogs = new System.Windows.Forms.TextBox();
            this.flpCommonCommands = new System.Windows.Forms.FlowLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.gbOperate.SuspendLayout();
            this.gbResult.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbOperate
            // 
            this.gbOperate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbOperate.Controls.Add(this.flpCommonCommands);
            this.gbOperate.Controls.Add(this.btnRun);
            this.gbOperate.Controls.Add(this.btnSelectCassandraContainerDir);
            this.gbOperate.Controls.Add(this.txtArguments);
            this.gbOperate.Controls.Add(this.btnSelectCassandraImageDir);
            this.gbOperate.Controls.Add(this.label4);
            this.gbOperate.Controls.Add(this.label3);
            this.gbOperate.Controls.Add(this.txtCassandraContainerDir);
            this.gbOperate.Controls.Add(this.label2);
            this.gbOperate.Controls.Add(this.txtCassandraImageDir);
            this.gbOperate.Controls.Add(this.label1);
            this.gbOperate.Location = new System.Drawing.Point(12, 12);
            this.gbOperate.Name = "gbOperate";
            this.gbOperate.Size = new System.Drawing.Size(1128, 286);
            this.gbOperate.TabIndex = 0;
            this.gbOperate.TabStop = false;
            this.gbOperate.Text = "操作";
            // 
            // btnRun
            // 
            this.btnRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRun.Location = new System.Drawing.Point(974, 138);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(150, 46);
            this.btnRun.TabIndex = 2;
            this.btnRun.Text = "运行";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnSelectCassandraContainerDir
            // 
            this.btnSelectCassandraContainerDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectCassandraContainerDir.Location = new System.Drawing.Point(974, 86);
            this.btnSelectCassandraContainerDir.Name = "btnSelectCassandraContainerDir";
            this.btnSelectCassandraContainerDir.Size = new System.Drawing.Size(150, 46);
            this.btnSelectCassandraContainerDir.TabIndex = 2;
            this.btnSelectCassandraContainerDir.Text = "..";
            this.btnSelectCassandraContainerDir.UseVisualStyleBackColor = true;
            this.btnSelectCassandraContainerDir.Click += new System.EventHandler(this.btnSelectCassandraContainerDir_Click);
            // 
            // txtArguments
            // 
            this.txtArguments.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtArguments.Location = new System.Drawing.Point(166, 143);
            this.txtArguments.Name = "txtArguments";
            this.txtArguments.Size = new System.Drawing.Size(803, 38);
            this.txtArguments.TabIndex = 1;
            // 
            // btnSelectCassandraImageDir
            // 
            this.btnSelectCassandraImageDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectCassandraImageDir.Location = new System.Drawing.Point(973, 33);
            this.btnSelectCassandraImageDir.Name = "btnSelectCassandraImageDir";
            this.btnSelectCassandraImageDir.Size = new System.Drawing.Size(150, 46);
            this.btnSelectCassandraImageDir.TabIndex = 2;
            this.btnSelectCassandraImageDir.Text = "..";
            this.btnSelectCassandraImageDir.UseVisualStyleBackColor = true;
            this.btnSelectCassandraImageDir.Click += new System.EventHandler(this.btnSelectCassandraImageDir_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(71, 148);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(86, 31);
            this.label3.TabIndex = 0;
            this.label3.Text = "参数：";
            // 
            // txtCassandraContainerDir
            // 
            this.txtCassandraContainerDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCassandraContainerDir.Location = new System.Drawing.Point(166, 92);
            this.txtCassandraContainerDir.Name = "txtCassandraContainerDir";
            this.txtCassandraContainerDir.Size = new System.Drawing.Size(803, 38);
            this.txtCassandraContainerDir.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(25, 94);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(134, 31);
            this.label2.TabIndex = 0;
            this.label2.Text = "数据目录：";
            // 
            // txtCassandraImageDir
            // 
            this.txtCassandraImageDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCassandraImageDir.Location = new System.Drawing.Point(164, 38);
            this.txtCassandraImageDir.Name = "txtCassandraImageDir";
            this.txtCassandraImageDir.Size = new System.Drawing.Size(803, 38);
            this.txtCassandraImageDir.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(134, 31);
            this.label1.TabIndex = 0;
            this.label1.Text = "镜像目录：";
            // 
            // gbResult
            // 
            this.gbResult.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gbResult.Controls.Add(this.txtLogs);
            this.gbResult.Location = new System.Drawing.Point(12, 304);
            this.gbResult.Name = "gbResult";
            this.gbResult.Size = new System.Drawing.Size(1124, 714);
            this.gbResult.TabIndex = 1;
            this.gbResult.TabStop = false;
            this.gbResult.Text = "日志";
            // 
            // txtLogs
            // 
            this.txtLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLogs.Location = new System.Drawing.Point(3, 34);
            this.txtLogs.Multiline = true;
            this.txtLogs.Name = "txtLogs";
            this.txtLogs.ReadOnly = true;
            this.txtLogs.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLogs.Size = new System.Drawing.Size(1118, 677);
            this.txtLogs.TabIndex = 0;
            // 
            // flpCommonCommands
            // 
            this.flpCommonCommands.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpCommonCommands.Location = new System.Drawing.Point(166, 187);
            this.flpCommonCommands.Name = "flpCommonCommands";
            this.flpCommonCommands.Size = new System.Drawing.Size(801, 93);
            this.flpCommonCommands.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(23, 187);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(134, 31);
            this.label4.TabIndex = 0;
            this.label4.Text = "常用命令：";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 31F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1152, 1030);
            this.Controls.Add(this.gbResult);
            this.Controls.Add(this.gbOperate);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WinNodetool";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.gbOperate.ResumeLayout(false);
            this.gbOperate.PerformLayout();
            this.gbResult.ResumeLayout(false);
            this.gbResult.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private GroupBox gbOperate;
        private Button btnRun;
        private Button btnSelectCassandraContainerDir;
        private TextBox txtArguments;
        private Button btnSelectCassandraImageDir;
        private Label label3;
        private TextBox txtCassandraContainerDir;
        private Label label2;
        private TextBox txtCassandraImageDir;
        private Label label1;
        private GroupBox gbResult;
        private TextBox txtLogs;
        private FlowLayoutPanel flpCommonCommands;
        private Label label4;
    }
}