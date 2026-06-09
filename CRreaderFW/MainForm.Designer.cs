namespace WindowsFormsApp1
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.scanTestControl = new ScanTestControl();
            this.SuspendLayout();
            // 
            // scanTestControl
            // 
            this.scanTestControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scanTestControl.Location = new System.Drawing.Point(0, 0);
            this.scanTestControl.Name = "scanTestControl";
            this.scanTestControl.Size = new System.Drawing.Size(960, 610);
            this.scanTestControl.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(960, 610);
            this.Controls.Add(this.scanTestControl);
            this.MinimumSize = new System.Drawing.Size(829, 568);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CRreader 采码记录与历史回溯";
            this.ResumeLayout(false);
        }

        private ScanTestControl scanTestControl;
    }
}
