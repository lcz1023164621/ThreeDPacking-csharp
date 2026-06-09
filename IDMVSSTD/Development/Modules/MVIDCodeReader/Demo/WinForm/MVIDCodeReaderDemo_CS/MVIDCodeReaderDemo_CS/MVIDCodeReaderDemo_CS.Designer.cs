namespace MVIDCodeReaderDemo_CS
{
    partial class MVIDCodeReaderDemo_CS
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MVIDCodeReaderDemo_CS));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ButtonStop = new System.Windows.Forms.Button();
            this.ComboBoxCamList = new System.Windows.Forms.ComboBox();
            this.ButtonStart = new System.Windows.Forms.Button();
            this.ButtonEnum = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ButtonCutout = new System.Windows.Forms.Button();
            this.ButtonProc = new System.Windows.Forms.Button();
            this.ButtonInit = new System.Windows.Forms.Button();
            this.TextFilePath = new System.Windows.Forms.TextBox();
            this.ButtonLoad = new System.Windows.Forms.Button();
            this.listBoxResult = new System.Windows.Forms.ListBox();
            this.ButtonClean = new System.Windows.Forms.Button();
            this.pictureBoxDisplay = new System.Windows.Forms.PictureBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.dataGridViewAlgorithm = new System.Windows.Forms.DataGridView();
            this.ParamName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ParamValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.buttonSetParam = new System.Windows.Forms.Button();
            this.buttonGetParam = new System.Windows.Forms.Button();
            this.textBoxFrameRate = new System.Windows.Forms.TextBox();
            this.textBoxGain = new System.Windows.Forms.TextBox();
            this.textBoxExposure = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDisplay)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAlgorithm)).BeginInit();
            this.groupBox4.SuspendLayout();
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
            this.ComboBoxCamList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
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
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.ButtonCutout);
            this.groupBox2.Controls.Add(this.ButtonProc);
            this.groupBox2.Controls.Add(this.ButtonInit);
            this.groupBox2.Controls.Add(this.TextFilePath);
            this.groupBox2.Controls.Add(this.ButtonLoad);
            resources.ApplyResources(this.groupBox2, "groupBox2");
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.TabStop = false;
            // 
            // ButtonCutout
            // 
            resources.ApplyResources(this.ButtonCutout, "ButtonCutout");
            this.ButtonCutout.Name = "ButtonCutout";
            this.ButtonCutout.UseVisualStyleBackColor = true;
            this.ButtonCutout.Click += new System.EventHandler(this.ButtonCutout_Click);
            // 
            // ButtonProc
            // 
            resources.ApplyResources(this.ButtonProc, "ButtonProc");
            this.ButtonProc.Name = "ButtonProc";
            this.ButtonProc.UseVisualStyleBackColor = true;
            this.ButtonProc.Click += new System.EventHandler(this.ButtonProc_Click);
            // 
            // ButtonInit
            // 
            resources.ApplyResources(this.ButtonInit, "ButtonInit");
            this.ButtonInit.Name = "ButtonInit";
            this.ButtonInit.UseVisualStyleBackColor = true;
            this.ButtonInit.Click += new System.EventHandler(this.ButtonInit_Click);
            // 
            // TextFilePath
            // 
            resources.ApplyResources(this.TextFilePath, "TextFilePath");
            this.TextFilePath.Name = "TextFilePath";
            this.TextFilePath.ReadOnly = true;
            // 
            // ButtonLoad
            // 
            resources.ApplyResources(this.ButtonLoad, "ButtonLoad");
            this.ButtonLoad.Name = "ButtonLoad";
            this.ButtonLoad.UseVisualStyleBackColor = true;
            this.ButtonLoad.Click += new System.EventHandler(this.ButtonLoad_Click);
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
            this.pictureBoxDisplay.ErrorImage = null;
            resources.ApplyResources(this.pictureBoxDisplay, "pictureBoxDisplay");
            this.pictureBoxDisplay.Name = "pictureBoxDisplay";
            this.pictureBoxDisplay.TabStop = false;
            this.pictureBoxDisplay.DoubleClick += new System.EventHandler(this.pictureBoxDisplay_DoubleClick);
            this.pictureBoxDisplay.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBoxDisplay_Paint);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.dataGridViewAlgorithm);
            resources.ApplyResources(this.groupBox3, "groupBox3");
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.TabStop = false;
            // 
            // dataGridViewAlgorithm
            // 
            this.dataGridViewAlgorithm.AllowUserToAddRows = false;
            this.dataGridViewAlgorithm.AllowUserToDeleteRows = false;
            this.dataGridViewAlgorithm.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewAlgorithm.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ParamName,
            this.ParamValue});
            resources.ApplyResources(this.dataGridViewAlgorithm, "dataGridViewAlgorithm");
            this.dataGridViewAlgorithm.Name = "dataGridViewAlgorithm";
            this.dataGridViewAlgorithm.RowTemplate.Height = 23;
            this.dataGridViewAlgorithm.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.dataGridViewAlgorithm_CellBeginEdit);
            this.dataGridViewAlgorithm.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridViewAlgorithm_CellEndEdit);
            // 
            // ParamName
            // 
            this.ParamName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            resources.ApplyResources(this.ParamName, "ParamName");
            this.ParamName.Name = "ParamName";
            this.ParamName.ReadOnly = true;
            // 
            // ParamValue
            // 
            this.ParamValue.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            resources.ApplyResources(this.ParamValue, "ParamValue");
            this.ParamValue.Name = "ParamValue";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.buttonSetParam);
            this.groupBox4.Controls.Add(this.buttonGetParam);
            this.groupBox4.Controls.Add(this.textBoxFrameRate);
            this.groupBox4.Controls.Add(this.textBoxGain);
            this.groupBox4.Controls.Add(this.textBoxExposure);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Controls.Add(this.label1);
            resources.ApplyResources(this.groupBox4, "groupBox4");
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.TabStop = false;
            // 
            // buttonSetParam
            // 
            resources.ApplyResources(this.buttonSetParam, "buttonSetParam");
            this.buttonSetParam.Name = "buttonSetParam";
            this.buttonSetParam.UseVisualStyleBackColor = true;
            this.buttonSetParam.Click += new System.EventHandler(this.buttonSetParam_Click);
            // 
            // buttonGetParam
            // 
            resources.ApplyResources(this.buttonGetParam, "buttonGetParam");
            this.buttonGetParam.Name = "buttonGetParam";
            this.buttonGetParam.UseVisualStyleBackColor = true;
            this.buttonGetParam.Click += new System.EventHandler(this.buttonGetParam_Click);
            // 
            // textBoxFrameRate
            // 
            resources.ApplyResources(this.textBoxFrameRate, "textBoxFrameRate");
            this.textBoxFrameRate.Name = "textBoxFrameRate";
            // 
            // textBoxGain
            // 
            resources.ApplyResources(this.textBoxGain, "textBoxGain");
            this.textBoxGain.Name = "textBoxGain";
            // 
            // textBoxExposure
            // 
            resources.ApplyResources(this.textBoxExposure, "textBoxExposure");
            this.textBoxExposure.Name = "textBoxExposure";
            // 
            // label3
            // 
            resources.ApplyResources(this.label3, "label3");
            this.label3.Name = "label3";
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // MVIDCodeReaderDemo_CS
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.pictureBoxDisplay);
            this.Controls.Add(this.ButtonClean);
            this.Controls.Add(this.listBoxResult);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox3);
            this.MaximizeBox = false;
            this.Name = "MVIDCodeReaderDemo_CS";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MVIDCodeReaderDemo_CS_FormClosed);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDisplay)).EndInit();
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewAlgorithm)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button ButtonEnum;
        private System.Windows.Forms.Button ButtonStart;
        private System.Windows.Forms.ComboBox ComboBoxCamList;
        private System.Windows.Forms.Button ButtonStop;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button ButtonLoad;
        private System.Windows.Forms.Button ButtonCutout;
        private System.Windows.Forms.Button ButtonProc;
        private System.Windows.Forms.Button ButtonInit;
        private System.Windows.Forms.TextBox TextFilePath;
        private System.Windows.Forms.ListBox listBoxResult;
        private System.Windows.Forms.Button ButtonClean;
        private System.Windows.Forms.PictureBox pictureBoxDisplay;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.DataGridView dataGridViewAlgorithm;
        private System.Windows.Forms.DataGridViewTextBoxColumn ParamName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ParamValue;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox textBoxFrameRate;
        private System.Windows.Forms.TextBox textBoxGain;
        private System.Windows.Forms.TextBox textBoxExposure;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonGetParam;
        private System.Windows.Forms.Button buttonSetParam;
    }
}

