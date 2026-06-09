namespace ReconnectDemo_CS
{
    partial class ReconnectDemo_CS
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReconnectDemo_CS));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ButtonStop = new System.Windows.Forms.Button();
            this.ComboBoxCamList = new System.Windows.Forms.ComboBox();
            this.ButtonStart = new System.Windows.Forms.Button();
            this.ButtonEnum = new System.Windows.Forms.Button();
            this.listBoxResult = new System.Windows.Forms.ListBox();
            this.ButtonClean = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
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
            // ReconnectDemo_CS
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ButtonClean);
            this.Controls.Add(this.listBoxResult);
            this.Controls.Add(this.groupBox1);
            this.MaximizeBox = false;
            this.Name = "ReconnectDemo_CS";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ReconnectDemo_CS_FormClosed);
            this.groupBox1.ResumeLayout(false);
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
    }
}

