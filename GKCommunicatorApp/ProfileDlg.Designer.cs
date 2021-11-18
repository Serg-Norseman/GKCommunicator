namespace GKCommunicatorApp
{
    partial class ProfileDlg
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabProfile;
        private System.Windows.Forms.Label lblLanguages;
        private System.Windows.Forms.Label lblTimeZone;
        private System.Windows.Forms.Label lblCountry;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.TextBox txtLanguages;
        private System.Windows.Forms.TextBox txtTimeZone;
        private System.Windows.Forms.TextBox txtCountry;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.CheckBox chkLanguagesVisible;
        private System.Windows.Forms.CheckBox chkTimeZoneVisible;
        private System.Windows.Forms.CheckBox chkCountryVisible;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.TextBox txtEndPoint;
        private System.Windows.Forms.Label lblEndPoint;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabProfile = new System.Windows.Forms.TabPage();
            this.txtEndPoint = new System.Windows.Forms.TextBox();
            this.lblEndPoint = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.chkLanguagesVisible = new System.Windows.Forms.CheckBox();
            this.chkTimeZoneVisible = new System.Windows.Forms.CheckBox();
            this.chkCountryVisible = new System.Windows.Forms.CheckBox();
            this.txtLanguages = new System.Windows.Forms.TextBox();
            this.txtTimeZone = new System.Windows.Forms.TextBox();
            this.txtCountry = new System.Windows.Forms.TextBox();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.lblLanguages = new System.Windows.Forms.Label();
            this.lblTimeZone = new System.Windows.Forms.Label();
            this.lblCountry = new System.Windows.Forms.Label();
            this.lblUserName = new System.Windows.Forms.Label();
            this.tabControl.SuspendLayout();
            this.tabProfile.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabProfile);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(474, 435);
            this.tabControl.TabIndex = 0;
            // 
            // tabProfile
            // 
            this.tabProfile.Controls.Add(this.txtEndPoint);
            this.tabProfile.Controls.Add(this.lblEndPoint);
            this.tabProfile.Controls.Add(this.btnSave);
            this.tabProfile.Controls.Add(this.chkLanguagesVisible);
            this.tabProfile.Controls.Add(this.chkTimeZoneVisible);
            this.tabProfile.Controls.Add(this.chkCountryVisible);
            this.tabProfile.Controls.Add(this.txtLanguages);
            this.tabProfile.Controls.Add(this.txtTimeZone);
            this.tabProfile.Controls.Add(this.txtCountry);
            this.tabProfile.Controls.Add(this.txtUserName);
            this.tabProfile.Controls.Add(this.lblLanguages);
            this.tabProfile.Controls.Add(this.lblTimeZone);
            this.tabProfile.Controls.Add(this.lblCountry);
            this.tabProfile.Controls.Add(this.lblUserName);
            this.tabProfile.Location = new System.Drawing.Point(4, 30);
            this.tabProfile.Margin = new System.Windows.Forms.Padding(4);
            this.tabProfile.Name = "tabProfile";
            this.tabProfile.Padding = new System.Windows.Forms.Padding(8);
            this.tabProfile.Size = new System.Drawing.Size(466, 401);
            this.tabProfile.TabIndex = 0;
            this.tabProfile.Text = "Profile";
            this.tabProfile.UseVisualStyleBackColor = true;
            // 
            // txtEndPoint
            // 
            this.txtEndPoint.Location = new System.Drawing.Point(16, 37);
            this.txtEndPoint.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.txtEndPoint.Name = "txtEndPoint";
            this.txtEndPoint.ReadOnly = true;
            this.txtEndPoint.Size = new System.Drawing.Size(429, 28);
            this.txtEndPoint.TabIndex = 11;
            // 
            // lblEndPoint
            // 
            this.lblEndPoint.AutoSize = true;
            this.lblEndPoint.Location = new System.Drawing.Point(12, 12);
            this.lblEndPoint.Margin = new System.Windows.Forms.Padding(4);
            this.lblEndPoint.Name = "lblEndPoint";
            this.lblEndPoint.Size = new System.Drawing.Size(73, 21);
            this.lblEndPoint.TabIndex = 10;
            this.lblEndPoint.Text = "EndPoint";
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(340, 353);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(105, 37);
            this.btnSave.TabIndex = 9;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // chkLanguagesVisible
            // 
            this.chkLanguagesVisible.AutoSize = true;
            this.chkLanguagesVisible.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkLanguagesVisible.Location = new System.Drawing.Point(367, 271);
            this.chkLanguagesVisible.Margin = new System.Windows.Forms.Padding(4);
            this.chkLanguagesVisible.Name = "chkLanguagesVisible";
            this.chkLanguagesVisible.Size = new System.Drawing.Size(78, 25);
            this.chkLanguagesVisible.TabIndex = 8;
            this.chkLanguagesVisible.Text = "Visible";
            this.chkLanguagesVisible.UseVisualStyleBackColor = true;
            // 
            // chkTimeZoneVisible
            // 
            this.chkTimeZoneVisible.AutoSize = true;
            this.chkTimeZoneVisible.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkTimeZoneVisible.Location = new System.Drawing.Point(367, 206);
            this.chkTimeZoneVisible.Margin = new System.Windows.Forms.Padding(4);
            this.chkTimeZoneVisible.Name = "chkTimeZoneVisible";
            this.chkTimeZoneVisible.Size = new System.Drawing.Size(78, 25);
            this.chkTimeZoneVisible.TabIndex = 8;
            this.chkTimeZoneVisible.Text = "Visible";
            this.chkTimeZoneVisible.UseVisualStyleBackColor = true;
            // 
            // chkCountryVisible
            // 
            this.chkCountryVisible.AutoSize = true;
            this.chkCountryVisible.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkCountryVisible.Location = new System.Drawing.Point(367, 141);
            this.chkCountryVisible.Margin = new System.Windows.Forms.Padding(4);
            this.chkCountryVisible.Name = "chkCountryVisible";
            this.chkCountryVisible.Size = new System.Drawing.Size(78, 25);
            this.chkCountryVisible.TabIndex = 8;
            this.chkCountryVisible.Text = "Visible";
            this.chkCountryVisible.UseVisualStyleBackColor = true;
            // 
            // txtLanguages
            // 
            this.txtLanguages.Location = new System.Drawing.Point(16, 297);
            this.txtLanguages.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.txtLanguages.Name = "txtLanguages";
            this.txtLanguages.Size = new System.Drawing.Size(429, 28);
            this.txtLanguages.TabIndex = 7;
            // 
            // txtTimeZone
            // 
            this.txtTimeZone.Location = new System.Drawing.Point(16, 232);
            this.txtTimeZone.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.txtTimeZone.Name = "txtTimeZone";
            this.txtTimeZone.Size = new System.Drawing.Size(429, 28);
            this.txtTimeZone.TabIndex = 6;
            // 
            // txtCountry
            // 
            this.txtCountry.Location = new System.Drawing.Point(16, 167);
            this.txtCountry.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.txtCountry.Name = "txtCountry";
            this.txtCountry.Size = new System.Drawing.Size(429, 28);
            this.txtCountry.TabIndex = 5;
            // 
            // txtUserName
            // 
            this.txtUserName.Location = new System.Drawing.Point(16, 102);
            this.txtUserName.Margin = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(429, 28);
            this.txtUserName.TabIndex = 4;
            // 
            // lblLanguages
            // 
            this.lblLanguages.AutoSize = true;
            this.lblLanguages.Location = new System.Drawing.Point(12, 272);
            this.lblLanguages.Margin = new System.Windows.Forms.Padding(4);
            this.lblLanguages.Name = "lblLanguages";
            this.lblLanguages.Size = new System.Drawing.Size(82, 21);
            this.lblLanguages.TabIndex = 3;
            this.lblLanguages.Text = "Languages";
            // 
            // lblTimeZone
            // 
            this.lblTimeZone.AutoSize = true;
            this.lblTimeZone.Location = new System.Drawing.Point(12, 207);
            this.lblTimeZone.Margin = new System.Windows.Forms.Padding(4);
            this.lblTimeZone.Name = "lblTimeZone";
            this.lblTimeZone.Size = new System.Drawing.Size(78, 21);
            this.lblTimeZone.TabIndex = 2;
            this.lblTimeZone.Text = "TimeZone";
            // 
            // lblCountry
            // 
            this.lblCountry.AutoSize = true;
            this.lblCountry.Location = new System.Drawing.Point(12, 142);
            this.lblCountry.Margin = new System.Windows.Forms.Padding(4);
            this.lblCountry.Name = "lblCountry";
            this.lblCountry.Size = new System.Drawing.Size(66, 21);
            this.lblCountry.TabIndex = 1;
            this.lblCountry.Text = "Country";
            // 
            // lblUserName
            // 
            this.lblUserName.AutoSize = true;
            this.lblUserName.Location = new System.Drawing.Point(12, 77);
            this.lblUserName.Margin = new System.Windows.Forms.Padding(4);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Size = new System.Drawing.Size(83, 21);
            this.lblUserName.TabIndex = 0;
            this.lblUserName.Text = "UserName";
            // 
            // ProfileDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(474, 435);
            this.Controls.Add(this.tabControl);
            this.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProfileDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ProfileDlg";
            this.Load += new System.EventHandler(this.Form_Load);
            this.tabControl.ResumeLayout(false);
            this.tabProfile.ResumeLayout(false);
            this.tabProfile.PerformLayout();
            this.ResumeLayout(false);

        }
    }
}
