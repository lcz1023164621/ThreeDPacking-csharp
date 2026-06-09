namespace MVIDCodeReaderDemo_CS
{
    partial class MVIDCodeReaderDemoRaw_CS
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MVIDCodeReaderDemoRaw_CS));
            this.label1 = new System.Windows.Forms.Label();
            this.CloseForm = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxImageWidth = new System.Windows.Forms.TextBox();
            this.textBoxImageHeight = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBoxConvertType = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // CloseForm
            // 
            resources.ApplyResources(this.CloseForm, "CloseForm");
            this.CloseForm.Name = "CloseForm";
            this.CloseForm.UseVisualStyleBackColor = true;
            this.CloseForm.Click += new System.EventHandler(this.CloseForm_Click);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // textBoxImageWidth
            // 
            resources.ApplyResources(this.textBoxImageWidth, "textBoxImageWidth");
            this.textBoxImageWidth.Name = "textBoxImageWidth";
            // 
            // textBoxImageHeight
            // 
            resources.ApplyResources(this.textBoxImageHeight, "textBoxImageHeight");
            this.textBoxImageHeight.Name = "textBoxImageHeight";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // comboBoxConvertType
            // 
            this.comboBoxConvertType.FormattingEnabled = true;
            resources.ApplyResources(this.comboBoxConvertType, "comboBoxConvertType");
            this.comboBoxConvertType.Name = "comboBoxConvertType";
            // 
            // MVIDCodeReaderDemoRaw_CS
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.comboBoxConvertType);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBoxImageHeight);
            this.Controls.Add(this.textBoxImageWidth);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.CloseForm);
            this.Controls.Add(this.label1);
            this.MaximizeBox = false;
            this.Name = "MVIDCodeReaderDemoRaw_CS";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button CloseForm;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox textBoxImageWidth;
        public System.Windows.Forms.TextBox textBoxImageHeight;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.ComboBox comboBoxConvertType;
    }
}