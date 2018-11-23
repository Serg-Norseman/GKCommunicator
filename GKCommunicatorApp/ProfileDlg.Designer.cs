namespace GKCommunicatorApp
{
    partial class ProfileDlg
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListView lvSysInfo;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabProfile;
        private System.Windows.Forms.TabPage tabSysInfo;

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
            this.tabSysInfo = new System.Windows.Forms.TabPage();
            this.lvSysInfo = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.tabControl.SuspendLayout();
            this.tabSysInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tabProfile);
            this.tabControl.Controls.Add(this.tabSysInfo);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(468, 348);
            this.tabControl.TabIndex = 0;
            // 
            // tabProfile
            // 
            this.tabProfile.Location = new System.Drawing.Point(4, 26);
            this.tabProfile.Name = "tabProfile";
            this.tabProfile.Padding = new System.Windows.Forms.Padding(3);
            this.tabProfile.Size = new System.Drawing.Size(460, 318);
            this.tabProfile.TabIndex = 0;
            this.tabProfile.Text = "Profile";
            this.tabProfile.UseVisualStyleBackColor = true;
            // 
            // tabSysInfo
            // 
            this.tabSysInfo.Controls.Add(this.lvSysInfo);
            this.tabSysInfo.Location = new System.Drawing.Point(4, 26);
            this.tabSysInfo.Name = "tabSysInfo";
            this.tabSysInfo.Padding = new System.Windows.Forms.Padding(3);
            this.tabSysInfo.Size = new System.Drawing.Size(460, 318);
            this.tabSysInfo.TabIndex = 1;
            this.tabSysInfo.Text = "SysInfo";
            this.tabSysInfo.UseVisualStyleBackColor = true;
            // 
            // lvSysInfo
            // 
            this.lvSysInfo.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.lvSysInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvSysInfo.FullRowSelect = true;
            this.lvSysInfo.LabelEdit = true;
            this.lvSysInfo.Location = new System.Drawing.Point(3, 3);
            this.lvSysInfo.MultiSelect = false;
            this.lvSysInfo.Name = "lvSysInfo";
            this.lvSysInfo.Size = new System.Drawing.Size(454, 312);
            this.lvSysInfo.TabIndex = 0;
            this.lvSysInfo.UseCompatibleStateImageBehavior = false;
            this.lvSysInfo.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Property";
            this.columnHeader1.Width = 200;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Value";
            this.columnHeader2.Width = 200;
            // 
            // ProfileDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(468, 348);
            this.Controls.Add(this.tabControl);
            this.Font = new System.Drawing.Font("Calibri", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProfileDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ProfileDlg";
            this.Load += new System.EventHandler(this.Form_Load);
            this.tabControl.ResumeLayout(false);
            this.tabSysInfo.ResumeLayout(false);
            this.ResumeLayout(false);

        }
    }
}
