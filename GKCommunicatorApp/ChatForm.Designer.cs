namespace GKSimpleChat
{
    partial class ChatForm
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
            if (disposing && (components != null)) {
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.miConnection = new System.Windows.Forms.ToolStripMenuItem();
            this.miView = new System.Windows.Forms.ToolStripMenuItem();
            this.miService = new System.Windows.Forms.ToolStripMenuItem();
            this.miDHTLog = new System.Windows.Forms.ToolStripMenuItem();
            this.miSysInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.miHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblConnectionStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.panTempHeader = new System.Windows.Forms.Panel();
            this.btnConnect = new System.Windows.Forms.Button();
            this.txtMemberName = new System.Windows.Forms.TextBox();
            this.lblMemberName = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lstMembers = new System.Windows.Forms.ListBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.lstChatMsgs = new System.Windows.Forms.RichTextBox();
            this.txtChatMsg = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnSendToAll = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.txtRemoteAddress = new System.Windows.Forms.ComboBox();
            this.txtRemotePort = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtLocalPort = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.panTempHeader.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miConnection,
            this.miView,
            this.miService,
            this.miHelp});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 3, 0, 3);
            this.menuStrip1.Size = new System.Drawing.Size(782, 30);
            this.menuStrip1.TabIndex = 9;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // miConnection
            // 
            this.miConnection.Name = "miConnection";
            this.miConnection.Size = new System.Drawing.Size(96, 24);
            this.miConnection.Text = "Connection";
            // 
            // miView
            // 
            this.miView.Name = "miView";
            this.miView.Size = new System.Drawing.Size(53, 24);
            this.miView.Text = "View";
            // 
            // miService
            // 
            this.miService.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miDHTLog,
            this.miSysInfo});
            this.miService.Name = "miService";
            this.miService.Size = new System.Drawing.Size(68, 24);
            this.miService.Text = "Service";
            // 
            // miDHTLog
            // 
            this.miDHTLog.Name = "miDHTLog";
            this.miDHTLog.Size = new System.Drawing.Size(213, 26);
            this.miDHTLog.Text = "DHT Log";
            this.miDHTLog.Click += new System.EventHandler(this.miDHTLog_Click);
            // 
            // miSysInfo
            // 
            this.miSysInfo.Name = "miSysInfo";
            this.miSysInfo.Size = new System.Drawing.Size(213, 26);
            this.miSysInfo.Text = "System Information";
            this.miSysInfo.Click += new System.EventHandler(this.miSysInfo_Click);
            // 
            // miHelp
            // 
            this.miHelp.Name = "miHelp";
            this.miHelp.Size = new System.Drawing.Size(53, 24);
            this.miHelp.Text = "Help";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblConnectionStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 528);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            this.statusStrip1.Size = new System.Drawing.Size(782, 25);
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblConnectionStatus
            // 
            this.lblConnectionStatus.Name = "lblConnectionStatus";
            this.lblConnectionStatus.Size = new System.Drawing.Size(266, 20);
            this.lblConnectionStatus.Text = " Attemping to connect. Please standby.";
            // 
            // panTempHeader
            // 
            this.panTempHeader.Controls.Add(this.txtRemoteAddress);
            this.panTempHeader.Controls.Add(this.txtRemotePort);
            this.panTempHeader.Controls.Add(this.label3);
            this.panTempHeader.Controls.Add(this.label2);
            this.panTempHeader.Controls.Add(this.txtLocalPort);
            this.panTempHeader.Controls.Add(this.label1);
            this.panTempHeader.Controls.Add(this.btnConnect);
            this.panTempHeader.Controls.Add(this.txtMemberName);
            this.panTempHeader.Controls.Add(this.lblMemberName);
            this.panTempHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panTempHeader.Location = new System.Drawing.Point(0, 30);
            this.panTempHeader.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panTempHeader.Name = "panTempHeader";
            this.panTempHeader.Size = new System.Drawing.Size(782, 133);
            this.panTempHeader.TabIndex = 11;
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(528, 0);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(225, 37);
            this.btnConnect.TabIndex = 5;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // txtMemberName
            // 
            this.txtMemberName.Location = new System.Drawing.Point(126, 4);
            this.txtMemberName.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtMemberName.Name = "txtMemberName";
            this.txtMemberName.Size = new System.Drawing.Size(392, 28);
            this.txtMemberName.TabIndex = 4;
            // 
            // lblMemberName
            // 
            this.lblMemberName.AutoSize = true;
            this.lblMemberName.Location = new System.Drawing.Point(7, 8);
            this.lblMemberName.Margin = new System.Windows.Forms.Padding(7, 8, 7, 8);
            this.lblMemberName.Name = "lblMemberName";
            this.lblMemberName.Size = new System.Drawing.Size(111, 21);
            this.lblMemberName.TabIndex = 3;
            this.lblMemberName.Text = "MemberName";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 163);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lstMembers);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Size = new System.Drawing.Size(782, 365);
            this.splitContainer1.SplitterDistance = 254;
            this.splitContainer1.TabIndex = 12;
            // 
            // lstMembers
            // 
            this.lstMembers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstMembers.FormattingEnabled = true;
            this.lstMembers.ItemHeight = 21;
            this.lstMembers.Location = new System.Drawing.Point(0, 0);
            this.lstMembers.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lstMembers.Name = "lstMembers";
            this.lstMembers.Size = new System.Drawing.Size(254, 365);
            this.lstMembers.TabIndex = 4;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.lstChatMsgs);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.txtChatMsg);
            this.splitContainer2.Panel2.Controls.Add(this.flowLayoutPanel1);
            this.splitContainer2.Size = new System.Drawing.Size(524, 365);
            this.splitContainer2.SplitterDistance = 209;
            this.splitContainer2.SplitterWidth = 5;
            this.splitContainer2.TabIndex = 0;
            // 
            // lstChatMsgs
            // 
            this.lstChatMsgs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstChatMsgs.Location = new System.Drawing.Point(0, 0);
            this.lstChatMsgs.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.lstChatMsgs.Name = "lstChatMsgs";
            this.lstChatMsgs.ReadOnly = true;
            this.lstChatMsgs.Size = new System.Drawing.Size(524, 209);
            this.lstChatMsgs.TabIndex = 0;
            this.lstChatMsgs.Text = "";
            // 
            // txtChatMsg
            // 
            this.txtChatMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtChatMsg.Location = new System.Drawing.Point(0, 0);
            this.txtChatMsg.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtChatMsg.Multiline = true;
            this.txtChatMsg.Name = "txtChatMsg";
            this.txtChatMsg.Size = new System.Drawing.Size(524, 99);
            this.txtChatMsg.TabIndex = 6;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.btnSendToAll);
            this.flowLayoutPanel1.Controls.Add(this.btnSend);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 99);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(524, 52);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // btnSendToAll
            // 
            this.btnSendToAll.Location = new System.Drawing.Point(408, 5);
            this.btnSendToAll.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSendToAll.Name = "btnSendToAll";
            this.btnSendToAll.Size = new System.Drawing.Size(112, 42);
            this.btnSendToAll.TabIndex = 7;
            this.btnSendToAll.Text = "Send to All";
            this.btnSendToAll.UseVisualStyleBackColor = true;
            this.btnSendToAll.Click += new System.EventHandler(this.btnSendToAll_Click);
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(288, 5);
            this.btnSend.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(112, 42);
            this.btnSend.TabIndex = 8;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // txtRemoteAddress
            // 
            this.txtRemoteAddress.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.txtRemoteAddress.FormattingEnabled = true;
            this.txtRemoteAddress.Items.AddRange(new object[] {
            "127.0.0.1",
            "195.162.27.155"});
            this.txtRemoteAddress.Location = new System.Drawing.Point(182, 76);
            this.txtRemoteAddress.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtRemoteAddress.Name = "txtRemoteAddress";
            this.txtRemoteAddress.Size = new System.Drawing.Size(157, 29);
            this.txtRemoteAddress.TabIndex = 11;
            // 
            // txtRemotePort
            // 
            this.txtRemotePort.Location = new System.Drawing.Point(345, 76);
            this.txtRemotePort.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtRemotePort.Name = "txtRemotePort";
            this.txtRemotePort.Size = new System.Drawing.Size(159, 28);
            this.txtRemotePort.TabIndex = 9;
            this.txtRemotePort.Text = "8888";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(341, 51);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(98, 21);
            this.label3.TabIndex = 6;
            this.label3.Text = "Remote Port";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(178, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(124, 21);
            this.label2.TabIndex = 7;
            this.label2.Text = "Remote Address";
            // 
            // txtLocalPort
            // 
            this.txtLocalPort.Location = new System.Drawing.Point(11, 76);
            this.txtLocalPort.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtLocalPort.Name = "txtLocalPort";
            this.txtLocalPort.Size = new System.Drawing.Size(159, 28);
            this.txtLocalPort.TabIndex = 10;
            this.txtLocalPort.Text = "11000";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 51);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 21);
            this.label1.TabIndex = 8;
            this.label1.Text = "Local Port";
            // 
            // ChatForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(782, 553);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panTempHeader);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "ChatForm";
            this.Text = "ChatForm";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panTempHeader.ResumeLayout(false);
            this.panTempHeader.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            this.splitContainer2.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem miConnection;
        private System.Windows.Forms.ToolStripMenuItem miService;
        private System.Windows.Forms.ToolStripMenuItem miHelp;
        private System.Windows.Forms.ToolStripMenuItem miView;
        private System.Windows.Forms.ToolStripMenuItem miDHTLog;
        private System.Windows.Forms.ToolStripMenuItem miSysInfo;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblConnectionStatus;
        private System.Windows.Forms.Panel panTempHeader;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.TextBox txtMemberName;
        private System.Windows.Forms.Label lblMemberName;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListBox lstMembers;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TextBox txtChatMsg;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnSendToAll;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.RichTextBox lstChatMsgs;
        private System.Windows.Forms.ComboBox txtRemoteAddress;
        private System.Windows.Forms.TextBox txtRemotePort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtLocalPort;
        private System.Windows.Forms.Label label1;
    }
}