namespace BSLib.TeamsNet.UI
{
    partial class ChatForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripMenuItem miConnection;
        private System.Windows.Forms.ToolStripMenuItem miService;
        private System.Windows.Forms.ToolStripMenuItem miHelp;
        private System.Windows.Forms.ToolStripMenuItem miView;
        private System.Windows.Forms.ToolStripMenuItem miDHTLog;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel lblConnectionStatus;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private BSLib.TeamsNet.UI.PeersList lstMembers;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TextBox txtChatMsg;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnSendToAll;
        private System.Windows.Forms.Button btnSend;
        private BSLib.TeamsNet.UI.MessagesList lstChatMsgs;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem miConnect;
        private System.Windows.Forms.ToolStripMenuItem miDisconnect;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem miProfile;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem miExit;
        private System.Windows.Forms.ToolStripButton tbConnect;
        private System.Windows.Forms.ToolStripButton tbDisconnect;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton tbProfile;
        private System.Windows.Forms.ToolStripMenuItem miPeerProfile;
        private System.Windows.Forms.ToolStripStatusLabel lblTicks;
        private System.Windows.Forms.ToolStripMenuItem miAddPeer;
        private System.Windows.Forms.ToolStripMenuItem miPeersList;
        private System.Windows.Forms.ToolStripMenuItem miAllPeers;
        private System.Windows.Forms.ToolStripMenuItem miOnlyFriends;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem miConnectionInfo;
        private System.Windows.Forms.ToolStripMenuItem miContents;
        private System.Windows.Forms.ToolStripMenuItem miAbout;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripDropDownButton tbPresenceStatus;
        private System.Windows.Forms.ContextMenuStrip menuPresenceStatuses;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabChat;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.miConnection = new System.Windows.Forms.ToolStripMenuItem();
            this.miConnect = new System.Windows.Forms.ToolStripMenuItem();
            this.miDisconnect = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.miProfile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.miExit = new System.Windows.Forms.ToolStripMenuItem();
            this.miView = new System.Windows.Forms.ToolStripMenuItem();
            this.miPeersList = new System.Windows.Forms.ToolStripMenuItem();
            this.miAllPeers = new System.Windows.Forms.ToolStripMenuItem();
            this.miOnlyFriends = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.miConnectionInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.miService = new System.Windows.Forms.ToolStripMenuItem();
            this.miDHTLog = new System.Windows.Forms.ToolStripMenuItem();
            this.miHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.miContents = new System.Windows.Forms.ToolStripMenuItem();
            this.miAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tbConnect = new System.Windows.Forms.ToolStripButton();
            this.tbDisconnect = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tbProfile = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.tbPresenceStatus = new System.Windows.Forms.ToolStripDropDownButton();
            this.menuPresenceStatuses = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.lblConnectionStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblTicks = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lstMembers = new BSLib.TeamsNet.UI.PeersList();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.miPeerProfile = new System.Windows.Forms.ToolStripMenuItem();
            this.miAddPeer = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.lstChatMsgs = new BSLib.TeamsNet.UI.MessagesList();
            this.txtChatMsg = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnSendToAll = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabChat = new System.Windows.Forms.TabPage();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
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
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(731, 24);
            this.menuStrip1.TabIndex = 9;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // miConnection
            // 
            this.miConnection.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miConnect,
            this.miDisconnect,
            this.toolStripMenuItem1,
            this.miProfile,
            this.toolStripMenuItem2,
            this.miExit});
            this.miConnection.Name = "miConnection";
            this.miConnection.Size = new System.Drawing.Size(81, 20);
            this.miConnection.Text = "Connection";
            // 
            // miConnect
            // 
            this.miConnect.Name = "miConnect";
            this.miConnect.Size = new System.Drawing.Size(133, 22);
            this.miConnect.Text = "Connect";
            this.miConnect.Click += new System.EventHandler(this.miConnect_Click);
            // 
            // miDisconnect
            // 
            this.miDisconnect.Name = "miDisconnect";
            this.miDisconnect.Size = new System.Drawing.Size(133, 22);
            this.miDisconnect.Text = "Disconnect";
            this.miDisconnect.Click += new System.EventHandler(this.miDisconnect_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(130, 6);
            // 
            // miProfile
            // 
            this.miProfile.Name = "miProfile";
            this.miProfile.Size = new System.Drawing.Size(133, 22);
            this.miProfile.Text = "My Profile";
            this.miProfile.Click += new System.EventHandler(this.miMyProfile_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(130, 6);
            // 
            // miExit
            // 
            this.miExit.Name = "miExit";
            this.miExit.Size = new System.Drawing.Size(133, 22);
            this.miExit.Text = "Exit";
            this.miExit.Click += new System.EventHandler(this.miExit_Click);
            // 
            // miView
            // 
            this.miView.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miPeersList});
            this.miView.Name = "miView";
            this.miView.Size = new System.Drawing.Size(44, 20);
            this.miView.Text = "View";
            // 
            // miPeersList
            // 
            this.miPeersList.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miAllPeers,
            this.miOnlyFriends,
            this.toolStripMenuItem3,
            this.miConnectionInfo});
            this.miPeersList.Name = "miPeersList";
            this.miPeersList.Size = new System.Drawing.Size(120, 22);
            this.miPeersList.Text = "Peers list";
            // 
            // miAllPeers
            // 
            this.miAllPeers.Checked = true;
            this.miAllPeers.CheckState = System.Windows.Forms.CheckState.Checked;
            this.miAllPeers.Name = "miAllPeers";
            this.miAllPeers.Size = new System.Drawing.Size(160, 22);
            this.miAllPeers.Text = "All peers";
            // 
            // miOnlyFriends
            // 
            this.miOnlyFriends.Enabled = false;
            this.miOnlyFriends.Name = "miOnlyFriends";
            this.miOnlyFriends.Size = new System.Drawing.Size(160, 22);
            this.miOnlyFriends.Text = "Only friends";
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(157, 6);
            // 
            // miConnectionInfo
            // 
            this.miConnectionInfo.Checked = true;
            this.miConnectionInfo.CheckOnClick = true;
            this.miConnectionInfo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.miConnectionInfo.Name = "miConnectionInfo";
            this.miConnectionInfo.Size = new System.Drawing.Size(160, 22);
            this.miConnectionInfo.Text = "Connection Info";
            this.miConnectionInfo.CheckedChanged += new System.EventHandler(this.miConnectionInfo_CheckedChanged);
            // 
            // miService
            // 
            this.miService.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miDHTLog});
            this.miService.Name = "miService";
            this.miService.Size = new System.Drawing.Size(56, 20);
            this.miService.Text = "Service";
            // 
            // miDHTLog
            // 
            this.miDHTLog.Name = "miDHTLog";
            this.miDHTLog.Size = new System.Drawing.Size(120, 22);
            this.miDHTLog.Text = "DHT Log";
            this.miDHTLog.Click += new System.EventHandler(this.miDHTLog_Click);
            // 
            // miHelp
            // 
            this.miHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miContents,
            this.miAbout});
            this.miHelp.Name = "miHelp";
            this.miHelp.Size = new System.Drawing.Size(44, 20);
            this.miHelp.Text = "Help";
            // 
            // miContents
            // 
            this.miContents.Enabled = false;
            this.miContents.Name = "miContents";
            this.miContents.Size = new System.Drawing.Size(122, 22);
            this.miContents.Text = "Contents";
            // 
            // miAbout
            // 
            this.miAbout.Enabled = false;
            this.miAbout.Name = "miAbout";
            this.miAbout.Size = new System.Drawing.Size(122, 22);
            this.miAbout.Text = "About";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tbConnect,
            this.tbDisconnect,
            this.toolStripSeparator1,
            this.tbProfile,
            this.toolStripSeparator2,
            this.tbPresenceStatus,
            this.toolStripSeparator3});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(731, 27);
            this.toolStrip1.TabIndex = 13;
            // 
            // tbConnect
            // 
            this.tbConnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tbConnect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbConnect.Name = "tbConnect";
            this.tbConnect.Size = new System.Drawing.Size(56, 24);
            this.tbConnect.Text = "Connect";
            this.tbConnect.Click += new System.EventHandler(this.miConnect_Click);
            // 
            // tbDisconnect
            // 
            this.tbDisconnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tbDisconnect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbDisconnect.Name = "tbDisconnect";
            this.tbDisconnect.Size = new System.Drawing.Size(70, 24);
            this.tbDisconnect.Text = "Disconnect";
            this.tbDisconnect.Click += new System.EventHandler(this.miDisconnect_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // tbProfile
            // 
            this.tbProfile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tbProfile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbProfile.Name = "tbProfile";
            this.tbProfile.Size = new System.Drawing.Size(65, 24);
            this.tbProfile.Text = "My Profile";
            this.tbProfile.Click += new System.EventHandler(this.miMyProfile_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 27);
            // 
            // tbPresenceStatus
            // 
            this.tbPresenceStatus.DropDown = this.menuPresenceStatuses;
            this.tbPresenceStatus.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbPresenceStatus.Name = "tbPresenceStatus";
            this.tbPresenceStatus.Size = new System.Drawing.Size(99, 24);
            this.tbPresenceStatus.Text = "PresenceStatus";
            // 
            // menuPresenceStatuses
            // 
            this.menuPresenceStatuses.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuPresenceStatuses.Name = "menuPresenceStatuses";
            this.menuPresenceStatuses.OwnerItem = this.tbPresenceStatus;
            this.menuPresenceStatuses.Size = new System.Drawing.Size(61, 4);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblConnectionStatus,
            this.lblTicks});
            this.statusStrip1.Location = new System.Drawing.Point(0, 420);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 13, 0);
            this.statusStrip1.Size = new System.Drawing.Size(731, 22);
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // lblConnectionStatus
            // 
            this.lblConnectionStatus.Name = "lblConnectionStatus";
            this.lblConnectionStatus.Size = new System.Drawing.Size(12, 17);
            this.lblConnectionStatus.Text = "-";
            // 
            // lblTicks
            // 
            this.lblTicks.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblTicks.Name = "lblTicks";
            this.lblTicks.Size = new System.Drawing.Size(15, 17);
            this.lblTicks.Text = "+";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 51);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lstMembers);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(731, 369);
            this.splitContainer1.SplitterDistance = 234;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 12;
            // 
            // lstMembers
            // 
            this.lstMembers.ContextMenuStrip = this.contextMenuStrip1;
            this.lstMembers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstMembers.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.lstMembers.Location = new System.Drawing.Point(0, 0);
            this.lstMembers.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.lstMembers.Name = "lstMembers";
            this.lstMembers.ShowConnectionInfo = false;
            this.lstMembers.Size = new System.Drawing.Size(234, 369);
            this.lstMembers.TabIndex = 4;
            this.lstMembers.SelectedValueChanged += new System.EventHandler(this.lstMembers_SelectedValueChanged);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miPeerProfile,
            this.miAddPeer});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(123, 48);
            // 
            // miPeerProfile
            // 
            this.miPeerProfile.Name = "miPeerProfile";
            this.miPeerProfile.Size = new System.Drawing.Size(122, 22);
            this.miPeerProfile.Text = "Profile";
            this.miPeerProfile.Click += new System.EventHandler(this.miPeerProfile_Click);
            // 
            // miAddPeer
            // 
            this.miAddPeer.Name = "miAddPeer";
            this.miAddPeer.Size = new System.Drawing.Size(122, 22);
            this.miAddPeer.Text = "Add peer";
            this.miAddPeer.Click += new System.EventHandler(this.miAddPeer_Click);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
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
            this.splitContainer2.Size = new System.Drawing.Size(494, 369);
            this.splitContainer2.SplitterDistance = 204;
            this.splitContainer2.TabIndex = 0;
            // 
            // tabChat
            //
            this.tabChat.Controls.Add(this.splitContainer2);
            this.tabChat.Location = new System.Drawing.Point(4, 26);
            this.tabChat.Name = "tabChat";
            this.tabChat.Size = new System.Drawing.Size(564, 291);
            this.tabChat.TabIndex = 0;
            this.tabChat.Text = "Chat";
            // 
            // tabControl1
            //
            this.tabControl1.Controls.Add(this.tabChat);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(572, 321);
            // 
            // lstChatMsgs
            // 
            this.lstChatMsgs.BackColor = System.Drawing.Color.LightGray;
            this.lstChatMsgs.Core = null;
            this.lstChatMsgs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstChatMsgs.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.lstChatMsgs.LocalId = null;
            this.lstChatMsgs.Location = new System.Drawing.Point(0, 0);
            this.lstChatMsgs.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.lstChatMsgs.Name = "lstChatMsgs";
            this.lstChatMsgs.Size = new System.Drawing.Size(494, 204);
            this.lstChatMsgs.TabIndex = 0;
            // 
            // txtChatMsg
            // 
            this.txtChatMsg.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtChatMsg.Location = new System.Drawing.Point(0, 0);
            this.txtChatMsg.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtChatMsg.Multiline = true;
            this.txtChatMsg.Name = "txtChatMsg";
            this.txtChatMsg.Size = new System.Drawing.Size(494, 119);
            this.txtChatMsg.TabIndex = 6;
            this.txtChatMsg.Text = "test";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.btnSendToAll);
            this.flowLayoutPanel1.Controls.Add(this.btnSend);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 119);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(494, 42);
            this.flowLayoutPanel1.TabIndex = 0;
            // 
            // btnSendToAll
            // 
            this.btnSendToAll.Location = new System.Drawing.Point(401, 4);
            this.btnSendToAll.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSendToAll.Name = "btnSendToAll";
            this.btnSendToAll.Size = new System.Drawing.Size(90, 34);
            this.btnSendToAll.TabIndex = 7;
            this.btnSendToAll.Text = "Send to All";
            this.btnSendToAll.UseVisualStyleBackColor = true;
            this.btnSendToAll.Click += new System.EventHandler(this.btnSendToAll_Click);
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(305, 4);
            this.btnSend.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(90, 34);
            this.btnSend.TabIndex = 8;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 27);
            // 
            // ChatForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(1024, 768);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "ChatForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "BSLib.TeamsNet";
            this.Load += new System.EventHandler(this.ChatForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.contextMenuStrip1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
