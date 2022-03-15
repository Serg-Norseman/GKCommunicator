namespace GKLocations.Manager
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private GKNetLocationsPlugin.Editor.LocationsControl locationsControl;

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.locationsControl = new GKNetLocationsPlugin.Editor.LocationsControl();
            this.SuspendLayout();
            // 
            // locationsControl
            // 
            this.locationsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.locationsControl.Location = new System.Drawing.Point(0, 31);
            this.locationsControl.Name = "locationsControl";
            this.locationsControl.Size = new System.Drawing.Size(1144, 591);
            this.locationsControl.TabIndex = 1;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1144, 622);
            this.Controls.Add(this.locationsControl);
            this.Name = "MainForm";
            this.Text = "GKLocations Manager";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
