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
            this.lblMemberName = new System.Windows.Forms.Label();
            this.txtMemberName = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.lstMembers = new System.Windows.Forms.ListBox();
            this.lstChatMsgs = new System.Windows.Forms.ListBox();
            this.txtChatMsg = new System.Windows.Forms.TextBox();
            this.btnChat = new System.Windows.Forms.Button();
            this.btnWhisper = new System.Windows.Forms.Button();
            this.lblConnectionStatus = new System.Windows.Forms.Label();
            this.btnSysInfo = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblMemberName
            // 
            this.lblMemberName.AutoSize = true;
            this.lblMemberName.Location = new System.Drawing.Point(20, 18);
            this.lblMemberName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblMemberName.Name = "lblMemberName";
            this.lblMemberName.Size = new System.Drawing.Size(96, 17);
            this.lblMemberName.TabIndex = 0;
            this.lblMemberName.Text = "MemberName";
            // 
            // txtMemberName
            // 
            this.txtMemberName.Location = new System.Drawing.Point(24, 38);
            this.txtMemberName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtMemberName.Name = "txtMemberName";
            this.txtMemberName.Size = new System.Drawing.Size(349, 22);
            this.txtMemberName.TabIndex = 1;
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(383, 34);
            this.btnConnect.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(200, 28);
            this.btnConnect.TabIndex = 2;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // lstMembers
            // 
            this.lstMembers.FormattingEnabled = true;
            this.lstMembers.ItemHeight = 16;
            this.lstMembers.Location = new System.Drawing.Point(24, 105);
            this.lstMembers.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lstMembers.Name = "lstMembers";
            this.lstMembers.Size = new System.Drawing.Size(319, 276);
            this.lstMembers.TabIndex = 3;
            // 
            // lstChatMsgs
            // 
            this.lstChatMsgs.FormattingEnabled = true;
            this.lstChatMsgs.ItemHeight = 16;
            this.lstChatMsgs.Location = new System.Drawing.Point(352, 105);
            this.lstChatMsgs.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lstChatMsgs.Name = "lstChatMsgs";
            this.lstChatMsgs.Size = new System.Drawing.Size(561, 212);
            this.lstChatMsgs.TabIndex = 4;
            // 
            // txtChatMsg
            // 
            this.txtChatMsg.Location = new System.Drawing.Point(352, 357);
            this.txtChatMsg.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtChatMsg.Name = "txtChatMsg";
            this.txtChatMsg.Size = new System.Drawing.Size(319, 22);
            this.txtChatMsg.TabIndex = 5;
            // 
            // btnChat
            // 
            this.btnChat.Location = new System.Drawing.Point(697, 354);
            this.btnChat.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnChat.Name = "btnChat";
            this.btnChat.Size = new System.Drawing.Size(100, 28);
            this.btnChat.TabIndex = 6;
            this.btnChat.Text = "Chat";
            this.btnChat.UseVisualStyleBackColor = true;
            this.btnChat.Click += new System.EventHandler(this.btnChat_Click);
            // 
            // btnWhisper
            // 
            this.btnWhisper.Location = new System.Drawing.Point(815, 354);
            this.btnWhisper.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnWhisper.Name = "btnWhisper";
            this.btnWhisper.Size = new System.Drawing.Size(100, 28);
            this.btnWhisper.TabIndex = 6;
            this.btnWhisper.Text = "Whisper";
            this.btnWhisper.UseVisualStyleBackColor = true;
            this.btnWhisper.Click += new System.EventHandler(this.btnWhisper_Click);
            // 
            // lblConnectionStatus
            // 
            this.lblConnectionStatus.AutoSize = true;
            this.lblConnectionStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblConnectionStatus.Location = new System.Drawing.Point(20, 427);
            this.lblConnectionStatus.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblConnectionStatus.Name = "lblConnectionStatus";
            this.lblConnectionStatus.Size = new System.Drawing.Size(287, 17);
            this.lblConnectionStatus.TabIndex = 7;
            this.lblConnectionStatus.Text = "Attemping to connect. Please standby.";
            this.lblConnectionStatus.Visible = false;
            // 
            // btnSysInfo
            // 
            this.btnSysInfo.Location = new System.Drawing.Point(815, 35);
            this.btnSysInfo.Margin = new System.Windows.Forms.Padding(4);
            this.btnSysInfo.Name = "btnSysInfo";
            this.btnSysInfo.Size = new System.Drawing.Size(100, 28);
            this.btnSysInfo.TabIndex = 6;
            this.btnSysInfo.Text = "SysInfo";
            this.btnSysInfo.UseVisualStyleBackColor = true;
            this.btnSysInfo.Click += new System.EventHandler(this.btnSysInfo_Click);
            // 
            // ChatForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(957, 454);
            this.Controls.Add(this.lblConnectionStatus);
            this.Controls.Add(this.btnSysInfo);
            this.Controls.Add(this.btnWhisper);
            this.Controls.Add(this.btnChat);
            this.Controls.Add(this.txtChatMsg);
            this.Controls.Add(this.lstChatMsgs);
            this.Controls.Add(this.lstMembers);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.txtMemberName);
            this.Controls.Add(this.lblMemberName);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "ChatForm";
            this.Text = "ChatForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblMemberName;
        private System.Windows.Forms.TextBox txtMemberName;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.ListBox lstMembers;
        private System.Windows.Forms.ListBox lstChatMsgs;
        private System.Windows.Forms.TextBox txtChatMsg;
        private System.Windows.Forms.Button btnChat;
        private System.Windows.Forms.Button btnWhisper;
        private System.Windows.Forms.Label lblConnectionStatus;
        private System.Windows.Forms.Button btnSysInfo;
    }
}