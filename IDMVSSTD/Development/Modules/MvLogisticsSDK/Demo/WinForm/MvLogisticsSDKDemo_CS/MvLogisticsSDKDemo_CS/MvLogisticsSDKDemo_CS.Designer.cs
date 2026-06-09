namespace MvLogisticsSDKDemo_CS
{
    partial class MvLogisticsSDKDemo_CS
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
            this.pictureBoxDisplay = new System.Windows.Forms.PictureBox();
            this.ButtonStart = new System.Windows.Forms.Button();
            this.ButtonStop = new System.Windows.Forms.Button();
            this.BarcodeDis = new System.Windows.Forms.Label();
            this.DataHistory = new System.Windows.Forms.DataGridView();
            this.包裹号 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.条码 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.体积 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.重量 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BarcodeText = new System.Windows.Forms.TextBox();
            this.VolumeText = new System.Windows.Forms.TextBox();
            this.VolumeDis = new System.Windows.Forms.Label();
            this.WeightDis = new System.Windows.Forms.Label();
            this.WeightText = new System.Windows.Forms.TextBox();
            this.ButtonConfigLoad = new System.Windows.Forms.Button();
            this.ConfigPath = new System.Windows.Forms.TextBox();
            this.ButtonInit = new System.Windows.Forms.Button();
            this.ButtonDeinit = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDisplay)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataHistory)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxDisplay
            // 
            this.pictureBoxDisplay.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.pictureBoxDisplay.ErrorImage = null;
            this.pictureBoxDisplay.Location = new System.Drawing.Point(3, 12);
            this.pictureBoxDisplay.Name = "pictureBoxDisplay";
            this.pictureBoxDisplay.Size = new System.Drawing.Size(441, 310);
            this.pictureBoxDisplay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxDisplay.TabIndex = 0;
            this.pictureBoxDisplay.TabStop = false;
            this.pictureBoxDisplay.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBoxDisplay_Paint);
            // 
            // ButtonStart
            // 
            this.ButtonStart.Location = new System.Drawing.Point(104, 429);
            this.ButtonStart.Name = "ButtonStart";
            this.ButtonStart.Size = new System.Drawing.Size(75, 23);
            this.ButtonStart.TabIndex = 1;
            this.ButtonStart.Text = "开始工作";
            this.ButtonStart.UseVisualStyleBackColor = true;
            this.ButtonStart.Click += new System.EventHandler(this.ButtonStart_Click);
            // 
            // ButtonStop
            // 
            this.ButtonStop.Location = new System.Drawing.Point(199, 429);
            this.ButtonStop.Name = "ButtonStop";
            this.ButtonStop.Size = new System.Drawing.Size(75, 23);
            this.ButtonStop.TabIndex = 2;
            this.ButtonStop.Text = "停止工作";
            this.ButtonStop.UseVisualStyleBackColor = true;
            this.ButtonStop.Click += new System.EventHandler(this.ButtonStop_Click);
            // 
            // BarcodeDis
            // 
            this.BarcodeDis.AutoSize = true;
            this.BarcodeDis.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BarcodeDis.Location = new System.Drawing.Point(453, 42);
            this.BarcodeDis.Name = "BarcodeDis";
            this.BarcodeDis.Size = new System.Drawing.Size(32, 17);
            this.BarcodeDis.TabIndex = 3;
            this.BarcodeDis.Text = "条码";
            // 
            // DataHistory
            // 
            this.DataHistory.AllowUserToAddRows = false;
            this.DataHistory.AllowUserToDeleteRows = false;
            this.DataHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.DataHistory.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.包裹号,
            this.条码,
            this.体积,
            this.重量});
            this.DataHistory.Location = new System.Drawing.Point(447, 214);
            this.DataHistory.Name = "DataHistory";
            this.DataHistory.RowTemplate.Height = 23;
            this.DataHistory.Size = new System.Drawing.Size(415, 313);
            this.DataHistory.TabIndex = 0;
            // 
            // 包裹号
            // 
            this.包裹号.HeaderText = "包裹号";
            this.包裹号.Name = "包裹号";
            // 
            // 条码
            // 
            this.条码.HeaderText = "条码";
            this.条码.Name = "条码";
            // 
            // 体积
            // 
            this.体积.HeaderText = "体积";
            this.体积.Name = "体积";
            // 
            // 重量
            // 
            this.重量.HeaderText = "重量";
            this.重量.Name = "重量";
            // 
            // BarcodeText
            // 
            this.BarcodeText.Location = new System.Drawing.Point(507, 42);
            this.BarcodeText.Name = "BarcodeText";
            this.BarcodeText.Size = new System.Drawing.Size(324, 21);
            this.BarcodeText.TabIndex = 4;
            // 
            // VolumeText
            // 
            this.VolumeText.Location = new System.Drawing.Point(507, 96);
            this.VolumeText.Name = "VolumeText";
            this.VolumeText.Size = new System.Drawing.Size(324, 21);
            this.VolumeText.TabIndex = 5;
            // 
            // VolumeDis
            // 
            this.VolumeDis.AutoSize = true;
            this.VolumeDis.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.VolumeDis.Location = new System.Drawing.Point(453, 100);
            this.VolumeDis.Name = "VolumeDis";
            this.VolumeDis.Size = new System.Drawing.Size(32, 17);
            this.VolumeDis.TabIndex = 6;
            this.VolumeDis.Text = "体积";
            // 
            // WeightDis
            // 
            this.WeightDis.AutoSize = true;
            this.WeightDis.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.WeightDis.Location = new System.Drawing.Point(453, 155);
            this.WeightDis.Name = "WeightDis";
            this.WeightDis.Size = new System.Drawing.Size(32, 17);
            this.WeightDis.TabIndex = 7;
            this.WeightDis.Text = "重量";
            // 
            // WeightText
            // 
            this.WeightText.Location = new System.Drawing.Point(507, 155);
            this.WeightText.Name = "WeightText";
            this.WeightText.Size = new System.Drawing.Size(324, 21);
            this.WeightText.TabIndex = 8;
            // 
            // ButtonConfigLoad
            // 
            this.ButtonConfigLoad.Location = new System.Drawing.Point(12, 372);
            this.ButtonConfigLoad.Name = "ButtonConfigLoad";
            this.ButtonConfigLoad.Size = new System.Drawing.Size(91, 23);
            this.ButtonConfigLoad.TabIndex = 9;
            this.ButtonConfigLoad.Text = "选择配置文件";
            this.ButtonConfigLoad.UseVisualStyleBackColor = true;
            this.ButtonConfigLoad.Click += new System.EventHandler(this.ButtonConfigLoad_Click);
            // 
            // ConfigPath
            // 
            this.ConfigPath.Location = new System.Drawing.Point(121, 374);
            this.ConfigPath.Name = "ConfigPath";
            this.ConfigPath.Size = new System.Drawing.Size(267, 21);
            this.ConfigPath.TabIndex = 10;
            // 
            // ButtonInit
            // 
            this.ButtonInit.Location = new System.Drawing.Point(12, 429);
            this.ButtonInit.Name = "ButtonInit";
            this.ButtonInit.Size = new System.Drawing.Size(75, 23);
            this.ButtonInit.TabIndex = 11;
            this.ButtonInit.Text = "初始化资源";
            this.ButtonInit.UseVisualStyleBackColor = true;
            this.ButtonInit.Click += new System.EventHandler(this.ButtonInit_Click);
            // 
            // ButtonDeinit
            // 
            this.ButtonDeinit.Cursor = System.Windows.Forms.Cursors.Default;
            this.ButtonDeinit.Location = new System.Drawing.Point(303, 429);
            this.ButtonDeinit.Name = "ButtonDeinit";
            this.ButtonDeinit.Size = new System.Drawing.Size(75, 23);
            this.ButtonDeinit.TabIndex = 12;
            this.ButtonDeinit.Text = "销毁资源";
            this.ButtonDeinit.UseVisualStyleBackColor = true;
            this.ButtonDeinit.Click += new System.EventHandler(this.ButtonDeinit_Click);
            // 
            // MvLogisticsSDKDemo_CS
            // 
            this.ClientSize = new System.Drawing.Size(874, 539);
            this.Controls.Add(this.ButtonDeinit);
            this.Controls.Add(this.ButtonInit);
            this.Controls.Add(this.ConfigPath);
            this.Controls.Add(this.ButtonConfigLoad);
            this.Controls.Add(this.WeightText);
            this.Controls.Add(this.WeightDis);
            this.Controls.Add(this.VolumeDis);
            this.Controls.Add(this.VolumeText);
            this.Controls.Add(this.BarcodeText);
            this.Controls.Add(this.DataHistory);
            this.Controls.Add(this.BarcodeDis);
            this.Controls.Add(this.ButtonStop);
            this.Controls.Add(this.ButtonStart);
            this.Controls.Add(this.pictureBoxDisplay);
            this.Name = "MvLogisticsSDKDemo_CS";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDisplay)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.DataHistory)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ButtonConfigLoad;
        private System.Windows.Forms.TextBox ConfigPath;
        private System.Windows.Forms.Button ButtonInit;


        private System.Windows.Forms.PictureBox pictureBoxDisplay;
        private System.Windows.Forms.Button ButtonStart;
        private System.Windows.Forms.Button ButtonStop;
        private System.Windows.Forms.Label BarcodeDis;
        private System.Windows.Forms.DataGridViewTextBoxColumn 条码;
        private System.Windows.Forms.DataGridViewTextBoxColumn 体积;
        private System.Windows.Forms.DataGridViewTextBoxColumn 重量;
        private System.Windows.Forms.TextBox BarcodeText;
        private System.Windows.Forms.TextBox VolumeText;
        private System.Windows.Forms.Label VolumeDis;
        private System.Windows.Forms.Label WeightDis;
        private System.Windows.Forms.TextBox WeightText;
        private System.Windows.Forms.Button ButtonDeinit;
        private System.Windows.Forms.DataGridView DataHistory;
        private System.Windows.Forms.DataGridViewTextBoxColumn 包裹号;
    }
}

