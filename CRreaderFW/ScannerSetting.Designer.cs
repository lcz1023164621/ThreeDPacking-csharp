namespace WindowsFormsApp1
{
    partial class ScannerSetting
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
            this.tabReaderSettings = new System.Windows.Forms.TabControl();
            this.tabNetwork = new System.Windows.Forms.TabPage();
            this.tableNetwork = new System.Windows.Forms.TableLayoutPanel();
            this.lblReaderIp = new System.Windows.Forms.Label();
            this.txtReaderIp = new System.Windows.Forms.TextBox();
            this.lblReaderPort = new System.Windows.Forms.Label();
            this.numReaderPort = new System.Windows.Forms.NumericUpDown();
            this.btnTestNetwork = new System.Windows.Forms.Button();
            this.tabScanParams = new System.Windows.Forms.TabPage();
            this.tableScanParams = new System.Windows.Forms.TableLayoutPanel();
            this.lblScanInterval = new System.Windows.Forms.Label();
            this.numScanIntervalMs = new System.Windows.Forms.NumericUpDown();
            this.chkAutoFocus = new System.Windows.Forms.CheckBox();
            this.chkAutoReconnect = new System.Windows.Forms.CheckBox();
            this.grpLightMode = new System.Windows.Forms.GroupBox();
            this.rdoLightStrobe = new System.Windows.Forms.RadioButton();
            this.rdoLightAlways = new System.Windows.Forms.RadioButton();
            this.tabAdvanced = new System.Windows.Forms.TabPage();
            this.tableAdvanced = new System.Windows.Forms.TableLayoutPanel();
            this.chkSaveRawImage = new System.Windows.Forms.CheckBox();
            this.lblImageSavePath = new System.Windows.Forms.Label();
            this.txtImageSavePath = new System.Windows.Forms.TextBox();
            this.btnSelectImagePath = new System.Windows.Forms.Button();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.btnCancelSettings = new System.Windows.Forms.Button();
            this.btnRestoreDefaults = new System.Windows.Forms.Button();
            this.btnSaveSettings = new System.Windows.Forms.Button();
            this.btnApplySettings = new System.Windows.Forms.Button();
            this.tabReaderSettings.SuspendLayout();
            this.tabNetwork.SuspendLayout();
            this.tableNetwork.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numReaderPort)).BeginInit();
            this.tabScanParams.SuspendLayout();
            this.tableScanParams.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numScanIntervalMs)).BeginInit();
            this.grpLightMode.SuspendLayout();
            this.tabAdvanced.SuspendLayout();
            this.tableAdvanced.SuspendLayout();
            this.panelButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabReaderSettings
            // 
            this.tabReaderSettings.Controls.Add(this.tabNetwork);
            this.tabReaderSettings.Controls.Add(this.tabScanParams);
            this.tabReaderSettings.Controls.Add(this.tabAdvanced);
            this.tabReaderSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabReaderSettings.Location = new System.Drawing.Point(0, 0);
            this.tabReaderSettings.Name = "tabReaderSettings";
            this.tabReaderSettings.SelectedIndex = 0;
            this.tabReaderSettings.Size = new System.Drawing.Size(640, 400);
            this.tabReaderSettings.TabIndex = 0;
            // 
            // tabNetwork
            // 
            this.tabNetwork.Controls.Add(this.tableNetwork);
            this.tabNetwork.Location = new System.Drawing.Point(4, 25);
            this.tabNetwork.Name = "tabNetwork";
            this.tabNetwork.Padding = new System.Windows.Forms.Padding(12);
            this.tabNetwork.Size = new System.Drawing.Size(632, 309);
            this.tabNetwork.TabIndex = 0;
            this.tabNetwork.Text = "网络设置";
            this.tabNetwork.UseVisualStyleBackColor = true;
            // 
            // tableNetwork
            // 
            this.tableNetwork.ColumnCount = 3;
            this.tableNetwork.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.tableNetwork.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableNetwork.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableNetwork.Controls.Add(this.lblReaderIp, 0, 0);
            this.tableNetwork.Controls.Add(this.txtReaderIp, 1, 0);
            this.tableNetwork.Controls.Add(this.lblReaderPort, 0, 1);
            this.tableNetwork.Controls.Add(this.numReaderPort, 1, 1);
            this.tableNetwork.Controls.Add(this.btnTestNetwork, 2, 0);
            this.tableNetwork.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableNetwork.Location = new System.Drawing.Point(12, 12);
            this.tableNetwork.Name = "tableNetwork";
            this.tableNetwork.RowCount = 3;
            this.tableNetwork.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tableNetwork.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tableNetwork.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableNetwork.Size = new System.Drawing.Size(608, 138);
            this.tableNetwork.TabIndex = 0;
            // 
            // lblReaderIp
            // 
            this.lblReaderIp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblReaderIp.Location = new System.Drawing.Point(3, 0);
            this.lblReaderIp.Name = "lblReaderIp";
            this.lblReaderIp.Size = new System.Drawing.Size(124, 44);
            this.lblReaderIp.TabIndex = 0;
            this.lblReaderIp.Text = "扫码器IP";
            this.lblReaderIp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtReaderIp
            // 
            this.txtReaderIp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtReaderIp.Location = new System.Drawing.Point(133, 8);
            this.txtReaderIp.Margin = new System.Windows.Forms.Padding(3, 8, 12, 3);
            this.txtReaderIp.Name = "txtReaderIp";
            this.txtReaderIp.Size = new System.Drawing.Size(343, 25);
            this.txtReaderIp.TabIndex = 1;
            // 
            // lblReaderPort
            // 
            this.lblReaderPort.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblReaderPort.Location = new System.Drawing.Point(3, 44);
            this.lblReaderPort.Name = "lblReaderPort";
            this.lblReaderPort.Size = new System.Drawing.Size(124, 44);
            this.lblReaderPort.TabIndex = 2;
            this.lblReaderPort.Text = "端口";
            this.lblReaderPort.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numReaderPort
            // 
            this.numReaderPort.Dock = System.Windows.Forms.DockStyle.Left;
            this.numReaderPort.Location = new System.Drawing.Point(133, 52);
            this.numReaderPort.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.numReaderPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.numReaderPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numReaderPort.Name = "numReaderPort";
            this.numReaderPort.Size = new System.Drawing.Size(120, 25);
            this.numReaderPort.TabIndex = 3;
            this.numReaderPort.Value = new decimal(new int[] {
            8000,
            0,
            0,
            0});
            // 
            // btnTestNetwork
            // 
            this.btnTestNetwork.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnTestNetwork.Location = new System.Drawing.Point(489, 5);
            this.btnTestNetwork.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            this.btnTestNetwork.Name = "btnTestNetwork";
            this.btnTestNetwork.Size = new System.Drawing.Size(116, 34);
            this.btnTestNetwork.TabIndex = 4;
            this.btnTestNetwork.Text = "测试网络";
            this.btnTestNetwork.UseVisualStyleBackColor = true;
            this.btnTestNetwork.Click += new System.EventHandler(this.btnTestNetwork_Click);
            // 
            // tabScanParams
            // 
            this.tabScanParams.Controls.Add(this.tableScanParams);
            this.tabScanParams.Location = new System.Drawing.Point(4, 25);
            this.tabScanParams.Name = "tabScanParams";
            this.tabScanParams.Padding = new System.Windows.Forms.Padding(12);
            this.tabScanParams.Size = new System.Drawing.Size(632, 309);
            this.tabScanParams.TabIndex = 1;
            this.tabScanParams.Text = "采码参数";
            this.tabScanParams.UseVisualStyleBackColor = true;
            // 
            // tableScanParams
            // 
            this.tableScanParams.ColumnCount = 2;
            this.tableScanParams.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 160F));
            this.tableScanParams.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableScanParams.Controls.Add(this.lblScanInterval, 0, 0);
            this.tableScanParams.Controls.Add(this.numScanIntervalMs, 1, 0);
            this.tableScanParams.Controls.Add(this.chkAutoFocus, 1, 1);
            this.tableScanParams.Controls.Add(this.chkAutoReconnect, 1, 2);
            this.tableScanParams.Controls.Add(this.grpLightMode, 1, 3);
            this.tableScanParams.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableScanParams.Location = new System.Drawing.Point(12, 12);
            this.tableScanParams.Name = "tableScanParams";
            this.tableScanParams.RowCount = 5;
            this.tableScanParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tableScanParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableScanParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableScanParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 86F));
            this.tableScanParams.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableScanParams.Size = new System.Drawing.Size(608, 238);
            this.tableScanParams.TabIndex = 0;
            // 
            // lblScanInterval
            // 
            this.lblScanInterval.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblScanInterval.Location = new System.Drawing.Point(3, 0);
            this.lblScanInterval.Name = "lblScanInterval";
            this.lblScanInterval.Size = new System.Drawing.Size(154, 44);
            this.lblScanInterval.TabIndex = 0;
            this.lblScanInterval.Text = "采码时间间隔(ms)";
            this.lblScanInterval.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numScanIntervalMs
            // 
            this.numScanIntervalMs.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numScanIntervalMs.Location = new System.Drawing.Point(163, 8);
            this.numScanIntervalMs.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.numScanIntervalMs.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numScanIntervalMs.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numScanIntervalMs.Name = "numScanIntervalMs";
            this.numScanIntervalMs.Size = new System.Drawing.Size(130, 25);
            this.numScanIntervalMs.TabIndex = 1;
            this.numScanIntervalMs.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // chkAutoFocus
            // 
            this.chkAutoFocus.AutoSize = true;
            this.chkAutoFocus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkAutoFocus.Location = new System.Drawing.Point(163, 47);
            this.chkAutoFocus.Name = "chkAutoFocus";
            this.chkAutoFocus.Size = new System.Drawing.Size(442, 32);
            this.chkAutoFocus.TabIndex = 2;
            this.chkAutoFocus.Text = "自动对焦";
            this.chkAutoFocus.UseVisualStyleBackColor = true;
            // 
            // chkAutoReconnect
            // 
            this.chkAutoReconnect.AutoSize = true;
            this.chkAutoReconnect.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkAutoReconnect.Location = new System.Drawing.Point(163, 85);
            this.chkAutoReconnect.Name = "chkAutoReconnect";
            this.chkAutoReconnect.Size = new System.Drawing.Size(442, 32);
            this.chkAutoReconnect.TabIndex = 3;
            this.chkAutoReconnect.Text = "断线自动重连";
            this.chkAutoReconnect.UseVisualStyleBackColor = true;
            // 
            // grpLightMode
            // 
            this.grpLightMode.Controls.Add(this.rdoLightStrobe);
            this.grpLightMode.Controls.Add(this.rdoLightAlways);
            this.grpLightMode.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpLightMode.Location = new System.Drawing.Point(163, 123);
            this.grpLightMode.Name = "grpLightMode";
            this.grpLightMode.Size = new System.Drawing.Size(442, 80);
            this.grpLightMode.TabIndex = 4;
            this.grpLightMode.TabStop = false;
            this.grpLightMode.Text = "光源模式";
            // 
            // rdoLightStrobe
            // 
            this.rdoLightStrobe.AutoSize = true;
            this.rdoLightStrobe.Checked = true;
            this.rdoLightStrobe.Location = new System.Drawing.Point(125, 35);
            this.rdoLightStrobe.Name = "rdoLightStrobe";
            this.rdoLightStrobe.Size = new System.Drawing.Size(88, 19);
            this.rdoLightStrobe.TabIndex = 1;
            this.rdoLightStrobe.TabStop = true;
            this.rdoLightStrobe.Text = "频闪模式";
            this.rdoLightStrobe.UseVisualStyleBackColor = true;
            // 
            // rdoLightAlways
            // 
            this.rdoLightAlways.AutoSize = true;
            this.rdoLightAlways.Location = new System.Drawing.Point(20, 35);
            this.rdoLightAlways.Name = "rdoLightAlways";
            this.rdoLightAlways.Size = new System.Drawing.Size(88, 19);
            this.rdoLightAlways.TabIndex = 0;
            this.rdoLightAlways.Text = "常亮模式";
            this.rdoLightAlways.UseVisualStyleBackColor = true;
            // 
            // tabAdvanced
            // 
            this.tabAdvanced.Controls.Add(this.tableAdvanced);
            this.tabAdvanced.Location = new System.Drawing.Point(4, 25);
            this.tabAdvanced.Name = "tabAdvanced";
            this.tabAdvanced.Padding = new System.Windows.Forms.Padding(12);
            this.tabAdvanced.Size = new System.Drawing.Size(632, 309);
            this.tabAdvanced.TabIndex = 2;
            this.tabAdvanced.Text = "高级设置";
            this.tabAdvanced.UseVisualStyleBackColor = true;
            // 
            // tableAdvanced
            // 
            this.tableAdvanced.ColumnCount = 3;
            this.tableAdvanced.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 130F));
            this.tableAdvanced.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableAdvanced.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 110F));
            this.tableAdvanced.Controls.Add(this.chkSaveRawImage, 1, 0);
            this.tableAdvanced.Controls.Add(this.lblImageSavePath, 0, 1);
            this.tableAdvanced.Controls.Add(this.txtImageSavePath, 1, 1);
            this.tableAdvanced.Controls.Add(this.btnSelectImagePath, 2, 1);
            this.tableAdvanced.Dock = System.Windows.Forms.DockStyle.Top;
            this.tableAdvanced.Location = new System.Drawing.Point(12, 12);
            this.tableAdvanced.Name = "tableAdvanced";
            this.tableAdvanced.RowCount = 3;
            this.tableAdvanced.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tableAdvanced.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tableAdvanced.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableAdvanced.Size = new System.Drawing.Size(608, 142);
            this.tableAdvanced.TabIndex = 0;
            // 
            // chkSaveRawImage
            // 
            this.chkSaveRawImage.AutoSize = true;
            this.chkSaveRawImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkSaveRawImage.Location = new System.Drawing.Point(133, 3);
            this.chkSaveRawImage.Name = "chkSaveRawImage";
            this.chkSaveRawImage.Size = new System.Drawing.Size(362, 38);
            this.chkSaveRawImage.TabIndex = 0;
            this.chkSaveRawImage.Text = "保存采码照片";
            this.chkSaveRawImage.UseVisualStyleBackColor = true;
            // 
            // lblImageSavePath
            // 
            this.lblImageSavePath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblImageSavePath.Location = new System.Drawing.Point(3, 44);
            this.lblImageSavePath.Name = "lblImageSavePath";
            this.lblImageSavePath.Size = new System.Drawing.Size(124, 44);
            this.lblImageSavePath.TabIndex = 1;
            this.lblImageSavePath.Text = "图片保存目录";
            this.lblImageSavePath.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtImageSavePath
            // 
            this.txtImageSavePath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtImageSavePath.Location = new System.Drawing.Point(133, 52);
            this.txtImageSavePath.Margin = new System.Windows.Forms.Padding(3, 8, 3, 3);
            this.txtImageSavePath.Name = "txtImageSavePath";
            this.txtImageSavePath.Size = new System.Drawing.Size(362, 25);
            this.txtImageSavePath.TabIndex = 2;
            // 
            // btnSelectImagePath
            // 
            this.btnSelectImagePath.Dock = System.Windows.Forms.DockStyle.Top;
            this.btnSelectImagePath.Location = new System.Drawing.Point(501, 49);
            this.btnSelectImagePath.Margin = new System.Windows.Forms.Padding(3, 5, 3, 3);
            this.btnSelectImagePath.Name = "btnSelectImagePath";
            this.btnSelectImagePath.Size = new System.Drawing.Size(104, 34);
            this.btnSelectImagePath.TabIndex = 3;
            this.btnSelectImagePath.Text = "选择";
            this.btnSelectImagePath.UseVisualStyleBackColor = true;
            this.btnSelectImagePath.Click += new System.EventHandler(this.btnSelectImagePath_Click);
            // 
            // panelButtons
            // 
            this.panelButtons.Controls.Add(this.btnCancelSettings);
            this.panelButtons.Controls.Add(this.btnRestoreDefaults);
            this.panelButtons.Controls.Add(this.btnSaveSettings);
            this.panelButtons.Controls.Add(this.btnApplySettings);
            this.panelButtons.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelButtons.Location = new System.Drawing.Point(0, 400);
            this.panelButtons.Name = "panelButtons";
            this.panelButtons.Padding = new System.Windows.Forms.Padding(10);
            this.panelButtons.Size = new System.Drawing.Size(640, 60);
            this.panelButtons.TabIndex = 1;
            // 
            // btnCancelSettings
            // 
            this.btnCancelSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelSettings.Location = new System.Drawing.Point(532, 12);
            this.btnCancelSettings.Name = "btnCancelSettings";
            this.btnCancelSettings.Size = new System.Drawing.Size(96, 36);
            this.btnCancelSettings.TabIndex = 3;
            this.btnCancelSettings.Text = "取消";
            this.btnCancelSettings.UseVisualStyleBackColor = true;
            this.btnCancelSettings.Click += new System.EventHandler(this.btnCancelSettings_Click);
            // 
            // btnRestoreDefaults
            // 
            this.btnRestoreDefaults.Location = new System.Drawing.Point(12, 12);
            this.btnRestoreDefaults.Name = "btnRestoreDefaults";
            this.btnRestoreDefaults.Size = new System.Drawing.Size(110, 36);
            this.btnRestoreDefaults.TabIndex = 0;
            this.btnRestoreDefaults.Text = "恢复默认";
            this.btnRestoreDefaults.UseVisualStyleBackColor = true;
            this.btnRestoreDefaults.Click += new System.EventHandler(this.btnRestoreDefaults_Click);
            // 
            // btnSaveSettings
            // 
            this.btnSaveSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveSettings.Location = new System.Drawing.Point(430, 12);
            this.btnSaveSettings.Name = "btnSaveSettings";
            this.btnSaveSettings.Size = new System.Drawing.Size(96, 36);
            this.btnSaveSettings.TabIndex = 2;
            this.btnSaveSettings.Text = "保存";
            this.btnSaveSettings.UseVisualStyleBackColor = true;
            this.btnSaveSettings.Click += new System.EventHandler(this.btnSaveSettings_Click);
            // 
            // btnApplySettings
            // 
            this.btnApplySettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApplySettings.Location = new System.Drawing.Point(328, 12);
            this.btnApplySettings.Name = "btnApplySettings";
            this.btnApplySettings.Size = new System.Drawing.Size(96, 36);
            this.btnApplySettings.TabIndex = 1;
            this.btnApplySettings.Text = "应用";
            this.btnApplySettings.UseVisualStyleBackColor = true;
            this.btnApplySettings.Click += new System.EventHandler(this.btnApplySettings_Click);
            // 
            // ScannerSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 460);
            this.Controls.Add(this.tabReaderSettings);
            this.Controls.Add(this.panelButtons);
            this.MinimumSize = new System.Drawing.Size(620, 390);
            this.Name = "ScannerSetting";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "扫码器设置";
            this.tabReaderSettings.ResumeLayout(false);
            this.tabNetwork.ResumeLayout(false);
            this.tableNetwork.ResumeLayout(false);
            this.tableNetwork.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numReaderPort)).EndInit();
            this.tabScanParams.ResumeLayout(false);
            this.tableScanParams.ResumeLayout(false);
            this.tableScanParams.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numScanIntervalMs)).EndInit();
            this.grpLightMode.ResumeLayout(false);
            this.grpLightMode.PerformLayout();
            this.tabAdvanced.ResumeLayout(false);
            this.tableAdvanced.ResumeLayout(false);
            this.tableAdvanced.PerformLayout();
            this.panelButtons.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.TabControl tabReaderSettings;
        private System.Windows.Forms.TabPage tabNetwork;
        private System.Windows.Forms.TabPage tabScanParams;
        private System.Windows.Forms.TabPage tabAdvanced;
        private System.Windows.Forms.TableLayoutPanel tableNetwork;
        private System.Windows.Forms.Label lblReaderIp;
        private System.Windows.Forms.TextBox txtReaderIp;
        private System.Windows.Forms.Label lblReaderPort;
        private System.Windows.Forms.NumericUpDown numReaderPort;
        private System.Windows.Forms.Button btnTestNetwork;
        private System.Windows.Forms.TableLayoutPanel tableScanParams;
        private System.Windows.Forms.Label lblScanInterval;
        private System.Windows.Forms.NumericUpDown numScanIntervalMs;
        private System.Windows.Forms.CheckBox chkAutoFocus;
        private System.Windows.Forms.CheckBox chkAutoReconnect;
        private System.Windows.Forms.GroupBox grpLightMode;
        private System.Windows.Forms.RadioButton rdoLightStrobe;
        private System.Windows.Forms.RadioButton rdoLightAlways;
        private System.Windows.Forms.TableLayoutPanel tableAdvanced;
        private System.Windows.Forms.CheckBox chkSaveRawImage;
        private System.Windows.Forms.Label lblImageSavePath;
        private System.Windows.Forms.TextBox txtImageSavePath;
        private System.Windows.Forms.Button btnSelectImagePath;
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.Button btnCancelSettings;
        private System.Windows.Forms.Button btnRestoreDefaults;
        private System.Windows.Forms.Button btnSaveSettings;
        private System.Windows.Forms.Button btnApplySettings;
    }
}
