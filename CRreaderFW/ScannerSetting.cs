using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class ScannerSetting : Form
    {
        private ScannerSettingsData _settings;
        private NumericUpDown numDeviceIndex;
        private NumericUpDown numExposureTimeUs;
        private NumericUpDown numGainDb;
        private NumericUpDown numAcquisitionFrameRate;
        private NumericUpDown numHeartbeatTimeoutMs;
        private NumericUpDown numJpegQuality;
        private NumericUpDown numGevPacketSize;
        private CheckBox chkAutoPacketSize;
        private ComboBox cmbImageSaveFormat;
        private CheckBox chkExposureAuto;
        private CheckBox chkGainAuto;
        private ComboBox cmbAutoFocusCommand;
        private NumericUpDown numAutoFocusWaitMs;
        private ComboBox cmbAutoConfig;
        private ComboBox cmbFocusModeSelector;
        private NumericUpDown numFocusPositionIndex;
        private CheckBox chkUseManualFocusPosition;
        private NumericUpDown numFocusStep;
        private Label lblScanFrequencyHint;
        private TextBox txtSignalServerIp;
        private NumericUpDown numSignalServerPort;
        private TextBox txtSignalReceiveServerIp;
        private NumericUpDown numSignalReceiveServerPort;
        private NumericUpDown numSignalSendRetryIntervalMs;
        private NumericUpDown numSignalSendRetryMaxCount;
        private Label lblSignalSendRetryHint;
        private CheckBox chkSignalScanSuccessUntilStopped;
        private CheckBox chkSelectAllSymbologies;
        private readonly Dictionary<string, CheckBox> _symbologyCheckBoxes = new Dictionary<string, CheckBox>(StringComparer.OrdinalIgnoreCase);
        private bool _updatingSelectAllSymbologies;

        public ScannerSetting()
            : this(ScannerSettingsData.CreateDefault())
        {
        }

        public ScannerSetting(ScannerSettingsData settings)
        {
            InitializeComponent();
            BuildScanParametersTab();
            BuildSymbologyTab();
            BuildAdvancedParametersTab();
            _settings = settings ?? ScannerSettingsData.CreateDefault();
            _settings.Normalize();
            LoadSettingsToControls();
        }

        internal ScannerSettingsData Settings
        {
            get { return _settings; }
        }

        private void BuildScanParametersTab()
        {
            tabScanParams.Controls.Clear();
            tabScanParams.AutoScroll = true;
            tabScanParams.Padding = new Padding(12);

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                Padding = new Padding(0)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            lblScanInterval.Text = "采码频率 (ms)";
            lblScanFrequencyHint = new Label
            {
                AutoSize = true,
                ForeColor = Color.FromArgb(96, 96, 96),
                Margin = new Padding(8, 10, 0, 0)
            };
            numScanIntervalMs.ValueChanged += (s, e) => UpdateScanFrequencyHint();

            var scanIntervalPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Dock = DockStyle.Fill
            };
            scanIntervalPanel.Controls.Add(numScanIntervalMs);
            scanIntervalPanel.Controls.Add(lblScanFrequencyHint);

            chkAutoFocus.Text = "启用自动对焦（采码前执行对焦）";

            cmbAutoFocusCommand = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 180
            };
            cmbAutoFocusCommand.Items.AddRange(new object[] { "FocusOnce", "FocusContinuous", "FocusMode" });
            chkAutoFocus.CheckedChanged += (s, e) => UpdateFocusEditorsState();

            numAutoFocusWaitMs = CreateNumeric(0, 10000, 100);
            cmbAutoConfig = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 220
            };
            cmbAutoConfig.Items.AddRange(new object[]
            {
                "0 - 全自动对焦",
                "1 - 仅调整电机",
                "2 - 自动对焦后恢复参数"
            });
            cmbFocusModeSelector = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 180
            };
            cmbFocusModeSelector.Items.AddRange(new object[]
            {
                "0 - 全画面对焦",
                "1 - ROI 区域对焦"
            });
            numFocusPositionIndex = CreateNumeric(0, 7, 1);
            chkUseManualFocusPosition = new CheckBox
            {
                AutoSize = true,
                Text = "采码前加载预设对焦位（关闭自动对焦时使用）"
            };
            numFocusStep = CreateNumeric(1, 1000, 1);

            chkExposureAuto = new CheckBox
            {
                AutoSize = true,
                Text = "曝光自适应（连续自动曝光）"
            };
            chkExposureAuto.CheckedChanged += (s, e) => UpdateAdaptiveEditorsState();

            numExposureTimeUs = CreateNumeric(1000, 200000, 1000);
            chkGainAuto = new CheckBox
            {
                AutoSize = true,
                Text = "增益自适应（连续自动增益）"
            };
            chkGainAuto.CheckedChanged += (s, e) => UpdateAdaptiveEditorsState();

            numGainDb = CreateNumeric(0, 24, 0.5m, 1);
            chkAutoReconnect.Text = "断线自动重连";

            txtSignalServerIp = new TextBox { Width = 150 };
            numSignalServerPort = CreateNumeric(1, 65535, 1);
            txtSignalReceiveServerIp = new TextBox { Width = 150 };
            numSignalReceiveServerPort = CreateNumeric(1, 65535, 1);
            numSignalSendRetryIntervalMs = CreateNumeric(100, 10000, 100);
            numSignalSendRetryMaxCount = CreateNumeric(0, 100, 1);
            chkSignalScanSuccessUntilStopped = new CheckBox
            {
                AutoSize = true,
                Text = "信号1/2均按最大次数重发；信号1等3确认，信号2等5确认后停止",
                Enabled = false
            };
            lblSignalSendRetryHint = new Label
            {
                AutoSize = true,
                ForeColor = Color.FromArgb(96, 96, 96),
                Margin = new Padding(8, 10, 0, 0),
                Text = "信号1/2共用发送上限；达上限后停止重发并继续采码，等机械臂确认(3/5)；建议间隔>=500ms"
            };
            var signalRetryPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Dock = DockStyle.Fill
            };
            signalRetryPanel.Controls.Add(numSignalSendRetryMaxCount);
            signalRetryPanel.Controls.Add(lblSignalSendRetryHint);

            AddTableRow(table, "采码频率", scanIntervalPanel);
            AddTableRow(table, "自动对焦", chkAutoFocus);
            AddTableRow(table, "对焦命令", cmbAutoFocusCommand);
            AddTableRow(table, "对焦后等待 (ms)", numAutoFocusWaitMs);
            AddTableRow(table, "自动对焦模式", cmbAutoConfig);
            AddTableRow(table, "对焦区域", cmbFocusModeSelector);
            AddTableRow(table, "预设位编号 (0-7)", numFocusPositionIndex);
            AddTableRow(table, "手动对焦", chkUseManualFocusPosition);
            AddTableRow(table, "对焦步进单位", numFocusStep);
            AddTableRow(table, "曝光自适应", chkExposureAuto);
            AddTableRow(table, "曝光时间 (us)", numExposureTimeUs);
            AddTableRow(table, "增益自适应", chkGainAuto);
            AddTableRow(table, "增益 (dB)", numGainDb);
            AddTableRow(table, "连接", chkAutoReconnect);
            AddTableRow(table, "信号发送IP", txtSignalServerIp);
            AddTableRow(table, "信号发送端口", numSignalServerPort);
            AddTableRow(table, "信号接收IP", txtSignalReceiveServerIp);
            AddTableRow(table, "信号接收端口", numSignalReceiveServerPort);
            AddTableRow(table, "信号发送重发间隔(ms)", numSignalSendRetryIntervalMs);
            AddTableRow(table, "信号发送最大次数", signalRetryPanel);
            AddTableRow(table, "信号确认协议", chkSignalScanSuccessUntilStopped);
            AddTableRow(table, "光源模式", grpLightMode);

            tabScanParams.Controls.Add(table);
        }

        private void BuildSymbologyTab()
        {
            tabSymbology.Controls.Clear();
            tabSymbology.AutoScroll = true;
            tabSymbology.Padding = new Padding(12);

            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                Padding = new Padding(0)
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            var headerPanel = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 8)
            };
            var lblTitle = new Label
            {
                AutoSize = true,
                Text = "条码类型",
                Font = new Font(Font, FontStyle.Bold),
                Margin = new Padding(0, 6, 16, 0)
            };
            chkSelectAllSymbologies = new CheckBox
            {
                AutoSize = true,
                Text = "全选",
                Checked = true
            };
            chkSelectAllSymbologies.CheckedChanged += (s, e) => ApplySelectAllSymbologies();
            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(chkSelectAllSymbologies);
            AddRootRow(root, headerPanel, 34F);

            AddRootRow(root, CreateSymbologyGroup("一维码", BarcodeSymbologyCategory.OneDimensional), 150F);
            AddRootRow(root, CreateSymbologyGroup("二维码", BarcodeSymbologyCategory.TwoDimensional), 78F);
            AddRootRow(root, CreateSymbologyGroup("堆叠码", BarcodeSymbologyCategory.Stacked), 78F);

            var hint = new Label
            {
                AutoSize = true,
                ForeColor = Color.FromArgb(96, 96, 96),
                Margin = new Padding(0, 8, 0, 0),
                Text = "勾选需要识别的条码类型，保存后将在连接扫码器时写入设备。"
            };
            AddRootRow(root, hint, 28F);

            tabSymbology.Controls.Add(root);
        }

        private GroupBox CreateSymbologyGroup(string title, BarcodeSymbologyCategory category)
        {
            var group = new GroupBox
            {
                Text = title,
                AutoSize = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(10, 18, 10, 10)
            };

            var grid = new TableLayoutPanel
            {
                AutoSize = true,
                Dock = DockStyle.Top,
                ColumnCount = 5,
                Padding = new Padding(0)
            };
            for (int i = 0; i < 5; i++)
            {
                grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            }

            var entries = BarcodeSymbologyCatalog.All.Where(entry => entry.Category == category).ToList();
            int rowCount = (entries.Count + 4) / 5;
            for (int i = 0; i < rowCount; i++)
            {
                grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            }
            grid.RowCount = rowCount;

            for (int i = 0; i < entries.Count; i++)
            {
                BarcodeSymbologyEntry entry = entries[i];
                var checkBox = new CheckBox
                {
                    AutoSize = true,
                    Text = entry.DisplayName,
                    Tag = entry.SdkKey,
                    Checked = true,
                    Margin = new Padding(0, 4, 8, 0)
                };
                checkBox.CheckedChanged += (s, e) => UpdateSelectAllSymbologiesState();
                _symbologyCheckBoxes[entry.SdkKey] = checkBox;
                grid.Controls.Add(checkBox, i % 5, i / 5);
            }

            group.Controls.Add(grid);
            return group;
        }

        private static void AddRootRow(TableLayoutPanel table, Control control, float height)
        {
            int row = table.RowCount;
            table.RowCount = row + 1;
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
            control.Dock = DockStyle.Fill;
            table.Controls.Add(control, 0, row);
        }

        private void ApplySelectAllSymbologies()
        {
            if (_updatingSelectAllSymbologies || chkSelectAllSymbologies == null)
            {
                return;
            }

            bool selectAll = chkSelectAllSymbologies.Checked;
            _updatingSelectAllSymbologies = true;
            try
            {
                foreach (CheckBox checkBox in _symbologyCheckBoxes.Values)
                {
                    checkBox.Checked = selectAll;
                }
            }
            finally
            {
                _updatingSelectAllSymbologies = false;
            }
        }

        private void UpdateSelectAllSymbologiesState()
        {
            if (_updatingSelectAllSymbologies || chkSelectAllSymbologies == null || _symbologyCheckBoxes.Count == 0)
            {
                return;
            }

            bool allChecked = _symbologyCheckBoxes.Values.All(checkBox => checkBox.Checked);
            _updatingSelectAllSymbologies = true;
            try
            {
                chkSelectAllSymbologies.Checked = allChecked;
            }
            finally
            {
                _updatingSelectAllSymbologies = false;
            }
        }

        private void LoadSymbologySettingsToControls()
        {
            if (_symbologyCheckBoxes.Count == 0)
            {
                return;
            }

            HashSet<string> enabled = _settings.EnabledBarcodeSymbologies ?? BarcodeSymbologyCatalog.CreateDefaultEnabledSet();
            _updatingSelectAllSymbologies = true;
            try
            {
                foreach (KeyValuePair<string, CheckBox> pair in _symbologyCheckBoxes)
                {
                    pair.Value.Checked = enabled.Contains(pair.Key);
                }

                chkSelectAllSymbologies.Checked = _symbologyCheckBoxes.Values.All(checkBox => checkBox.Checked);
            }
            finally
            {
                _updatingSelectAllSymbologies = false;
            }
        }

        private void SaveSymbologyControlsToSettings()
        {
            if (_symbologyCheckBoxes.Count == 0)
            {
                return;
            }

            var enabled = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, CheckBox> pair in _symbologyCheckBoxes)
            {
                if (pair.Value.Checked)
                {
                    enabled.Add(pair.Key);
                }
            }

            _settings.EnabledBarcodeSymbologies = enabled;
        }

        private void BuildAdvancedParametersTab()
        {
            numDeviceIndex = CreateNumeric(0, 16, 1);
            numAcquisitionFrameRate = CreateNumeric(1, 120, 1, 1);
            numHeartbeatTimeoutMs = CreateNumeric(500, 60000, 500);
            numJpegQuality = CreateNumeric(50, 100, 1);
            cmbImageSaveFormat = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 130
            };
            cmbImageSaveFormat.Items.AddRange(new object[] { "JPG", "BMP" });
            numGevPacketSize = CreateNumeric(576, 9000, 64);
            chkAutoPacketSize = new CheckBox
            {
                AutoSize = true,
                Text = "自动协商 GigE 包大小"
            };
            chkAutoPacketSize.CheckedChanged += (s, e) =>
            {
                if (numGevPacketSize != null)
                    numGevPacketSize.Enabled = !chkAutoPacketSize.Checked;
            };

            var sdkTable = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                Padding = new Padding(0, 8, 0, 0)
            };
            sdkTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
            sdkTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            AddTableRow(sdkTable, "设备索引", numDeviceIndex);
            AddTableRow(sdkTable, "采集帧率(fps)", numAcquisitionFrameRate);
            AddTableRow(sdkTable, "心跳超时 (ms)", numHeartbeatTimeoutMs);
            AddTableRow(sdkTable, "图片格式", cmbImageSaveFormat);
            AddTableRow(sdkTable, "JPEG 质量", numJpegQuality);
            AddTableRow(sdkTable, "网络优化", chkAutoPacketSize);
            AddTableRow(sdkTable, "GigE 包大小", numGevPacketSize);

            tabAdvanced.Controls.Add(sdkTable);
        }

        private static NumericUpDown CreateNumeric(decimal min, decimal max, decimal increment, int decimals = 0)
        {
            return new NumericUpDown
            {
                Minimum = min,
                Maximum = max,
                Increment = increment,
                DecimalPlaces = decimals,
                Width = 130
            };
        }

        private static void AddTableRow(TableLayoutPanel table, string labelText, Control editor)
        {
            int row = table.RowCount;
            table.RowCount = row + 1;
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, editor is GroupBox ? 86F : 38F));

            var label = new Label
            {
                Text = labelText,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            if (editor is GroupBox)
            {
                editor.Dock = DockStyle.Fill;
            }
            else if (editor is FlowLayoutPanel)
            {
                editor.Dock = DockStyle.Fill;
            }
            else
            {
                editor.Dock = DockStyle.Left;
            }

            table.Controls.Add(label, 0, row);
            table.Controls.Add(editor, 1, row);
        }

        private void UpdateScanFrequencyHint()
        {
            if (lblScanFrequencyHint == null)
                return;

            int interval = (int)numScanIntervalMs.Value;
            if (interval <= 0)
            {
                lblScanFrequencyHint.Text = string.Empty;
                return;
            }

            double hz = 1000.0 / interval;
            lblScanFrequencyHint.Text = string.Format("约 {0:F1} 次/秒", hz);
        }

        private void UpdateAdaptiveEditorsState()
        {
            if (numExposureTimeUs != null)
                numExposureTimeUs.Enabled = !chkExposureAuto.Checked;
            if (numGainDb != null)
                numGainDb.Enabled = !chkGainAuto.Checked;
        }

        private void UpdateFocusEditorsState()
        {
            bool autoFocus = chkAutoFocus.Checked;
            if (cmbAutoFocusCommand != null)
                cmbAutoFocusCommand.Enabled = autoFocus;
            if (numAutoFocusWaitMs != null)
                numAutoFocusWaitMs.Enabled = autoFocus;
            if (cmbAutoConfig != null)
                cmbAutoConfig.Enabled = autoFocus;
            if (cmbFocusModeSelector != null)
                cmbFocusModeSelector.Enabled = autoFocus;

            if (numFocusPositionIndex != null)
                numFocusPositionIndex.Enabled = !autoFocus;
            if (chkUseManualFocusPosition != null)
                chkUseManualFocusPosition.Enabled = !autoFocus;
            if (numFocusStep != null)
                numFocusStep.Enabled = !autoFocus;
        }

        private void LoadSettingsToControls()
        {
            txtReaderIp.Text = _settings.ReaderIp;
            numReaderPort.Value = Clamp(_settings.ReaderPort, (int)numReaderPort.Minimum, (int)numReaderPort.Maximum);
            numScanIntervalMs.Value = Clamp(_settings.ScanIntervalMs, (int)numScanIntervalMs.Minimum, (int)numScanIntervalMs.Maximum);
            chkAutoFocus.Checked = _settings.AutoFocus;
            chkAutoReconnect.Checked = _settings.AutoReconnect;
            chkSaveRawImage.Checked = _settings.SaveRawImage;
            txtImageSavePath.Text = _settings.ImageSavePath;
            rdoLightAlways.Checked = string.Equals(_settings.LightMode, "常亮模式", StringComparison.OrdinalIgnoreCase);
            rdoLightStrobe.Checked = !rdoLightAlways.Checked;

            if (chkExposureAuto != null)
            {
                chkExposureAuto.Checked = _settings.ExposureAuto;
                chkGainAuto.Checked = _settings.GainAuto;
                numExposureTimeUs.Value = Clamp((decimal)_settings.ExposureTimeUs, numExposureTimeUs.Minimum, numExposureTimeUs.Maximum);
                numGainDb.Value = Clamp((decimal)_settings.GainDb, numGainDb.Minimum, numGainDb.Maximum);

                string focusCommand = string.IsNullOrWhiteSpace(_settings.AutoFocusCommand)
                    ? "FocusOnce"
                    : _settings.AutoFocusCommand.Trim();
                if (cmbAutoFocusCommand.Items.Contains(focusCommand))
                    cmbAutoFocusCommand.SelectedItem = focusCommand;
                else
                    cmbAutoFocusCommand.SelectedIndex = 0;

                numAutoFocusWaitMs.Value = Clamp(_settings.AutoFocusWaitMs, (int)numAutoFocusWaitMs.Minimum, (int)numAutoFocusWaitMs.Maximum);
                cmbAutoConfig.SelectedIndex = Clamp(_settings.AutoConfig, 0, cmbAutoConfig.Items.Count - 1);
                cmbFocusModeSelector.SelectedIndex = Clamp(_settings.FocusModeSelector, 0, cmbFocusModeSelector.Items.Count - 1);
                numFocusPositionIndex.Value = Clamp(_settings.FocusPositionIndex, (int)numFocusPositionIndex.Minimum, (int)numFocusPositionIndex.Maximum);
                chkUseManualFocusPosition.Checked = _settings.UseManualFocusPosition;
                numFocusStep.Value = Clamp(_settings.FocusStep, (int)numFocusStep.Minimum, (int)numFocusStep.Maximum);

                UpdateFocusEditorsState();
                UpdateAdaptiveEditorsState();
                UpdateScanFrequencyHint();
            }

            if (numDeviceIndex != null)
            {
                numDeviceIndex.Value = Clamp(_settings.DeviceIndex, (int)numDeviceIndex.Minimum, (int)numDeviceIndex.Maximum);
                numAcquisitionFrameRate.Value = Clamp((decimal)_settings.AcquisitionFrameRate, numAcquisitionFrameRate.Minimum, numAcquisitionFrameRate.Maximum);
                numHeartbeatTimeoutMs.Value = Clamp(_settings.GevHeartbeatTimeoutMs, (int)numHeartbeatTimeoutMs.Minimum, (int)numHeartbeatTimeoutMs.Maximum);
                numJpegQuality.Value = Clamp(_settings.JpegQuality, (int)numJpegQuality.Minimum, (int)numJpegQuality.Maximum);
                string imageSaveFormat = string.Equals(_settings.ImageSaveFormat, "BMP", StringComparison.OrdinalIgnoreCase) ? "BMP" : "JPG";
                cmbImageSaveFormat.SelectedItem = imageSaveFormat;
                chkAutoPacketSize.Checked = _settings.UseAutoPacketSize;
                int packetSize = _settings.GevSCPSPacketSize > 0 ? _settings.GevSCPSPacketSize : 1500;
                numGevPacketSize.Value = Clamp(packetSize, (int)numGevPacketSize.Minimum, (int)numGevPacketSize.Maximum);
                numGevPacketSize.Enabled = !chkAutoPacketSize.Checked;
            }

            if (numSignalSendRetryIntervalMs != null)
            {
                txtSignalServerIp.Text = _settings.SignalServerIp;
                numSignalServerPort.Value = Clamp(_settings.SignalServerPort, (int)numSignalServerPort.Minimum, (int)numSignalServerPort.Maximum);
                txtSignalReceiveServerIp.Text = _settings.SignalReceiveServerIp;
                numSignalReceiveServerPort.Value = Clamp(_settings.SignalReceiveServerPort, (int)numSignalReceiveServerPort.Minimum, (int)numSignalReceiveServerPort.Maximum);
                numSignalSendRetryIntervalMs.Value = Clamp(_settings.SignalSendRetryIntervalMs, (int)numSignalSendRetryIntervalMs.Minimum, (int)numSignalSendRetryIntervalMs.Maximum);
                numSignalSendRetryMaxCount.Value = Clamp(_settings.SignalSendRetryMaxCount, (int)numSignalSendRetryMaxCount.Minimum, (int)numSignalSendRetryMaxCount.Maximum);
                chkSignalScanSuccessUntilStopped.Checked = true;
            }

            LoadSymbologySettingsToControls();
        }

        private bool SaveControlsToSettings()
        {
            IPAddress unused;
            if (!IPAddress.TryParse(txtReaderIp.Text.Trim(), out unused))
            {
                MessageBox.Show(this, "请输入合法的扫码器 IPv4 地址。", "设置错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabReaderSettings.SelectedTab = tabNetwork;
                txtReaderIp.Focus();
                return false;
            }

            string imagePath = txtImageSavePath.Text.Trim();
            if (imagePath.Length == 0)
            {
                imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scan_images");
                txtImageSavePath.Text = imagePath;
            }

            try
            {
                Directory.CreateDirectory(imagePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "图片保存目录不可用：" + ex.Message, "设置错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabReaderSettings.SelectedTab = tabAdvanced;
                txtImageSavePath.Focus();
                return false;
            }

            _settings.ReaderIp = txtReaderIp.Text.Trim();
            _settings.ReaderPort = (int)numReaderPort.Value;
            _settings.ScanIntervalMs = (int)numScanIntervalMs.Value;
            _settings.AutoFocus = chkAutoFocus.Checked;
            _settings.AutoReconnect = chkAutoReconnect.Checked;
            _settings.SaveRawImage = chkSaveRawImage.Checked;
            _settings.ImageSavePath = imagePath;
            _settings.LightMode = rdoLightAlways.Checked ? "常亮模式" : "频闪模式";

            if (chkExposureAuto != null)
            {
                _settings.ExposureAuto = chkExposureAuto.Checked;
                _settings.GainAuto = chkGainAuto.Checked;
                _settings.ExposureTimeUs = (float)numExposureTimeUs.Value;
                _settings.GainDb = (float)numGainDb.Value;
                _settings.AutoFocusCommand = cmbAutoFocusCommand.SelectedItem?.ToString() ?? "FocusOnce";
                _settings.AutoFocusWaitMs = (int)numAutoFocusWaitMs.Value;
                _settings.AutoConfig = cmbAutoConfig.SelectedIndex;
                _settings.FocusModeSelector = cmbFocusModeSelector.SelectedIndex;
                _settings.FocusPositionIndex = (int)numFocusPositionIndex.Value;
                _settings.UseManualFocusPosition = chkUseManualFocusPosition.Checked;
                _settings.FocusStep = (int)numFocusStep.Value;
            }

            if (numDeviceIndex != null)
            {
                _settings.DeviceIndex = (int)numDeviceIndex.Value;
                _settings.AcquisitionFrameRate = (float)numAcquisitionFrameRate.Value;
                _settings.GevHeartbeatTimeoutMs = (int)numHeartbeatTimeoutMs.Value;
                _settings.JpegQuality = (int)numJpegQuality.Value;
                _settings.ImageSaveFormat = cmbImageSaveFormat.SelectedItem?.ToString() ?? "JPG";
                _settings.UseAutoPacketSize = chkAutoPacketSize.Checked;
                _settings.GevSCPSPacketSize = (int)numGevPacketSize.Value;
            }

            if (numSignalSendRetryIntervalMs != null)
            {
                if (!IPAddress.TryParse(txtSignalServerIp.Text.Trim(), out unused))
                {
                    MessageBox.Show(this, "请输入合法的信号发送通道 IPv4 地址。", "设置错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    tabReaderSettings.SelectedTab = tabScanParams;
                    txtSignalServerIp.Focus();
                    return false;
                }
                if (!IPAddress.TryParse(txtSignalReceiveServerIp.Text.Trim(), out unused))
                {
                    MessageBox.Show(this, "请输入合法的信号接收通道 IPv4 地址。", "设置错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    tabReaderSettings.SelectedTab = tabScanParams;
                    txtSignalReceiveServerIp.Focus();
                    return false;
                }

                _settings.SignalServerIp = txtSignalServerIp.Text.Trim();
                _settings.SignalServerPort = (int)numSignalServerPort.Value;
                _settings.SignalReceiveServerIp = txtSignalReceiveServerIp.Text.Trim();
                _settings.SignalReceiveServerPort = (int)numSignalReceiveServerPort.Value;
                _settings.SignalSendRetryIntervalMs = (int)numSignalSendRetryIntervalMs.Value;
                _settings.SignalSendRetryMaxCount = (int)numSignalSendRetryMaxCount.Value;
                _settings.SignalScanSuccessUntilStopped = true;
            }

            SaveSymbologyControlsToSettings();
            if (_settings.EnabledBarcodeSymbologies == null || _settings.EnabledBarcodeSymbologies.Count == 0)
            {
                MessageBox.Show(this, "请至少勾选一个条码类型。", "设置错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tabReaderSettings.SelectedTab = tabSymbology;
                return false;
            }

            _settings.Normalize();
            return true;
        }

        private void btnApplySettings_Click(object sender, EventArgs e)
        {
            if (SaveControlsToSettings())
            {
                ScannerSettingsStore.Save(_settings);
                MessageBox.Show(this, "扫码器设置已应用并保存。", "设置", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            if (!SaveControlsToSettings())
            {
                return;
            }

            ScannerSettingsStore.Save(_settings);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnRestoreDefaults_Click(object sender, EventArgs e)
        {
            _settings = ScannerSettingsData.CreateDefault();
            LoadSettingsToControls();
        }

        private void btnCancelSettings_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnTestNetwork_Click(object sender, EventArgs e)
        {
            IPAddress address;
            if (!IPAddress.TryParse(txtReaderIp.Text.Trim(), out address))
            {
                MessageBox.Show(this, "IP 地址格式不正确。", "网络测试", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show(this, "网络参数格式正确。设备连通性将在连接扫码器时按 IP 或设备索引自动匹配。", "网络测试", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnSelectImagePath_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择采码照片保存目录";
                if (Directory.Exists(txtImageSavePath.Text))
                {
                    dialog.SelectedPath = txtImageSavePath.Text;
                }
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    txtImageSavePath.Text = dialog.SelectedPath;
                }
            }
        }

        private static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }
            return value;
        }

        private static decimal Clamp(decimal value, decimal min, decimal max)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }
            return value;
        }
    }
}
