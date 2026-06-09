namespace Grab_Callback_CS
{
    partial class Grab_Callback_CS
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Grab_Callback_CS));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ButtonStop = new System.Windows.Forms.Button();
            this.ComboBoxCamList = new System.Windows.Forms.ComboBox();
            this.ButtonStart = new System.Windows.Forms.Button();
            this.ButtonEnum = new System.Windows.Forms.Button();
            this.listBoxResult = new System.Windows.Forms.ListBox();
            this.ButtonClean = new System.Windows.Forms.Button();
            this.pictureBoxDisplay = new System.Windows.Forms.PictureBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDisplay)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ButtonStop);
            this.groupBox1.Controls.Add(this.ComboBoxCamList);
            this.groupBox1.Controls.Add(this.ButtonStart);
            this.groupBox1.Controls.Add(this.ButtonEnum);
            resources.ApplyResources(this.groupBox1, "groupBox1");
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.TabStop = false;
            // 
            // ButtonStop
            // 
            resources.ApplyResources(this.ButtonStop, "ButtonStop");
            this.ButtonStop.Name = "ButtonStop";
            this.ButtonStop.UseVisualStyleBackColor = true;
            this.ButtonStop.Click += new System.EventHandler(this.ButtonStop_Click);
            // 
            // ComboBoxCamList
            // 
            this.ComboBoxCamList.FormattingEnabled = true;
            resources.ApplyResources(this.ComboBoxCamList, "ComboBoxCamList");
            this.ComboBoxCamList.Name = "ComboBoxCamList";
            this.ComboBoxCamList.SelectedIndexChanged += new System.EventHandler(this.ComboBoxCamList_SelectedIndexChanged);
            // 
            // ButtonStart
            // 
            resources.ApplyResources(this.ButtonStart, "ButtonStart");
            this.ButtonStart.Name = "ButtonStart";
            this.ButtonStart.UseVisualStyleBackColor = true;
            this.ButtonStart.Click += new System.EventHandler(this.ButtonStart_Click);
            // 
            // ButtonEnum
            // 
            resources.ApplyResources(this.ButtonEnum, "ButtonEnum");
            this.ButtonEnum.Name = "ButtonEnum";
            this.ButtonEnum.UseVisualStyleBackColor = true;
            this.ButtonEnum.Click += new System.EventHandler(this.ButtonEnum_Click);
            // 
            // listBoxResult
            // 
            this.listBoxResult.FormattingEnabled = true;
            resources.ApplyResources(this.listBoxResult, "listBoxResult");
            this.listBoxResult.Name = "listBoxResult";
            // 
            // ButtonClean
            // 
            resources.ApplyResources(this.ButtonClean, "ButtonClean");
            this.ButtonClean.Name = "ButtonClean";
            this.ButtonClean.UseVisualStyleBackColor = true;
            this.ButtonClean.Click += new System.EventHandler(this.ButtonClean_Click);
            // 
            // pictureBoxDisplay
            // 
            this.pictureBoxDisplay.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            resources.ApplyResources(this.pictureBoxDisplay, "pictureBoxDisplay");
            this.pictureBoxDisplay.Name = "pictureBoxDisplay";
            this.pictureBoxDisplay.TabStop = false;
            this.pictureBoxDisplay.DoubleClick += new System.EventHandler(this.pictureBoxDisplay_DoubleClick);
            this.pictureBoxDisplay.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBoxDisplay_Paint);
            // 
            // Grab_Callback_CS
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pictureBoxDisplay);
            this.Controls.Add(this.ButtonClean);
            this.Controls.Add(this.listBoxResult);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.Name = "Grab_Callback_CS";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Grab_Callback_CS_FormClosed);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDisplay)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button ButtonEnum;
        private System.Windows.Forms.Button ButtonStart;
        private System.Windows.Forms.ComboBox ComboBoxCamList;
        private System.Windows.Forms.Button ButtonStop;
        private System.Windows.Forms.ListBox listBoxResult;
        private System.Windows.Forms.Button ButtonClean;
        private System.Windows.Forms.PictureBox pictureBoxDisplay;
    }
}

