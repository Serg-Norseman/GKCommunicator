namespace GKCommunicatorApp
{
    partial class ICEExperimentForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.idText = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnSTUNDetect = new System.Windows.Forms.Button();
            this.outputText = new System.Windows.Forms.TextBox();
            this.btnCopy = new System.Windows.Forms.Button();
            this.startServer = new System.Windows.Forms.Button();
            this.idInput = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnPaste = new System.Windows.Forms.Button();
            this.chatInput = new System.Windows.Forms.TextBox();
            this.chatOutput = new System.Windows.Forms.RichTextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // idText
            // 
            this.idText.Location = new System.Drawing.Point(665, 66);
            this.idText.Margin = new System.Windows.Forms.Padding(4);
            this.idText.Name = "idText";
            this.idText.ReadOnly = true;
            this.idText.Size = new System.Drawing.Size(265, 22);
            this.idText.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(752, 46);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Connection ID";
            // 
            // btnSTUNDetect
            // 
            this.btnSTUNDetect.Location = new System.Drawing.Point(16, 15);
            this.btnSTUNDetect.Margin = new System.Windows.Forms.Padding(4);
            this.btnSTUNDetect.Name = "btnSTUNDetect";
            this.btnSTUNDetect.Size = new System.Drawing.Size(149, 123);
            this.btnSTUNDetect.TabIndex = 2;
            this.btnSTUNDetect.Text = "STUN Detect";
            this.btnSTUNDetect.UseVisualStyleBackColor = true;
            this.btnSTUNDetect.Click += new System.EventHandler(this.btnSTUNDetect_Click);
            // 
            // outputText
            // 
            this.outputText.Location = new System.Drawing.Point(173, 15);
            this.outputText.Margin = new System.Windows.Forms.Padding(4);
            this.outputText.Multiline = true;
            this.outputText.Name = "outputText";
            this.outputText.ReadOnly = true;
            this.outputText.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.outputText.Size = new System.Drawing.Size(459, 122);
            this.outputText.TabIndex = 3;
            // 
            // btnCopy
            // 
            this.btnCopy.Location = new System.Drawing.Point(940, 66);
            this.btnCopy.Margin = new System.Windows.Forms.Padding(4);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(33, 25);
            this.btnCopy.TabIndex = 4;
            this.btnCopy.Text = "📝";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // startServer
            // 
            this.startServer.Location = new System.Drawing.Point(92, 75);
            this.startServer.Margin = new System.Windows.Forms.Padding(4);
            this.startServer.Name = "startServer";
            this.startServer.Size = new System.Drawing.Size(149, 57);
            this.startServer.TabIndex = 5;
            this.startServer.Text = "Start";
            this.startServer.UseVisualStyleBackColor = true;
            this.startServer.Visible = false;
            this.startServer.Click += new System.EventHandler(this.startServer_Click);
            // 
            // idInput
            // 
            this.idInput.Location = new System.Drawing.Point(16, 43);
            this.idInput.Margin = new System.Windows.Forms.Padding(4);
            this.idInput.Name = "idInput";
            this.idInput.Size = new System.Drawing.Size(265, 22);
            this.idInput.TabIndex = 7;
            this.idInput.TextChanged += new System.EventHandler(this.idInput_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(96, 23);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(142, 17);
            this.label2.TabIndex = 8;
            this.label2.Text = "Target Connection ID";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnPaste);
            this.groupBox1.Controls.Add(this.idInput);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.startServer);
            this.groupBox1.Location = new System.Drawing.Point(648, 116);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(339, 151);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            // 
            // btnPaste
            // 
            this.btnPaste.Location = new System.Drawing.Point(291, 43);
            this.btnPaste.Margin = new System.Windows.Forms.Padding(4);
            this.btnPaste.Name = "btnPaste";
            this.btnPaste.Size = new System.Drawing.Size(33, 25);
            this.btnPaste.TabIndex = 10;
            this.btnPaste.Text = "📝";
            this.btnPaste.UseVisualStyleBackColor = true;
            this.btnPaste.Click += new System.EventHandler(this.btnPaste_Click);
            // 
            // chatInput
            // 
            this.chatInput.Location = new System.Drawing.Point(16, 470);
            this.chatInput.Margin = new System.Windows.Forms.Padding(4);
            this.chatInput.Name = "chatInput";
            this.chatInput.Size = new System.Drawing.Size(616, 22);
            this.chatInput.TabIndex = 10;
            this.chatInput.Visible = false;
            this.chatInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.chatInput_KeyPress);
            // 
            // chatOutput
            // 
            this.chatOutput.Location = new System.Drawing.Point(16, 145);
            this.chatOutput.Margin = new System.Windows.Forms.Padding(4);
            this.chatOutput.Name = "chatOutput";
            this.chatOutput.ReadOnly = true;
            this.chatOutput.Size = new System.Drawing.Size(616, 317);
            this.chatOutput.TabIndex = 11;
            this.chatOutput.Text = "";
            this.chatOutput.Visible = false;
            // 
            // ICEExperimentForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1003, 554);
            this.Controls.Add(this.chatOutput);
            this.Controls.Add(this.chatInput);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnCopy);
            this.Controls.Add(this.outputText);
            this.Controls.Add(this.btnSTUNDetect);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.idText);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ICEExperimentForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ICE-Experiment";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.TextBox idText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSTUNDetect;
        private System.Windows.Forms.TextBox outputText;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Button startServer;
        private System.Windows.Forms.TextBox idInput;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnPaste;
        private System.Windows.Forms.TextBox chatInput;
        private System.Windows.Forms.RichTextBox chatOutput;
    }
}

