using System;
using System.Drawing;
using System.IO;
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
        private NumericUpDown numHeartbeatTimeoutMs;
        private NumericUpDown numJpegQuality;
        private CheckBox chkAutoPacketSize;

        public ScannerSetting()
            : this(ScannerSettingsData.CreateDefault())
        {
        }

        public ScannerSetting(ScannerSettingsData settings)
        {
            InitializeComponent();
            InitSdkParameterControls();
            _settings = settings ?? ScannerSettingsData.CreateDefault();
            _settings.Normalize();
            LoadSettingsToControls();
        }

        internal ScannerSettingsData Settings
        {
            get { return _settings; }
        }

        private void InitSdkParameterControls()
        {
            var panelSdk = new Panel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Padding = new Padding(0, 8, 0, 0)
            };

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                Padding = new Padding(0)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 160F));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            table.RowCount = 6;
            for (int i = 0; i < 6; i++)
            {
                table.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            }

            numDeviceIndex = CreateNumeric(0, 16, 0);
            numExposureTimeUs = CreateNumeric(1000, 200000, 1000);
            numGainDb = CreateNumeric(0, 24, 0.5m, 1);
            numHeartbeatTimeoutMs = CreateNumeric(500, 60000, 500);
            numJpegQuality = CreateNumeric(50, 100, 1);
            chkAutoPacketSize = new CheckBox
            {
                AutoSize = true,
                Text = "自动协商 GigE 包大小",
                Dock = DockStyle.Fill
            };

            AddRow(table, 0, "设备索引", numDeviceIndex);
            AddRow(table, 1, "曝光时间(us)", numExposureTimeUs);
            AddRow(table, 2, "增益(dB)", numGainDb);
            AddRow(table, 3, "心跳超时(ms)", numHeartbeatTimeoutMs);
            AddRow(table, 4, "JPEG质量", numJpegQuality);
            AddRow(table, 5, "网络优化", chkAutoPacketSize);

            panelSdk.Controls.Add(table);
            tabScanParams.Controls.Add(panelSdk);
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

        private static void AddRow(TableLayoutPanel table, int row, string labelText, Control editor)
        {
            var label = new Label
            {
                Text = labelText,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            editor.Dock = DockStyle.Left;
            table.Controls.Add(label, 0, row);
            table.Controls.Add(editor, 1, row);
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

            if (numDeviceIndex != null)
            {
                numDeviceIndex.Value = Clamp(_settings.DeviceIndex, (int)numDeviceIndex.Minimum, (int)numDeviceIndex.Maximum);
                numExposureTimeUs.Value = Clamp((decimal)_settings.ExposureTimeUs, numExposureTimeUs.Minimum, numExposureTimeUs.Maximum);
                numGainDb.Value = Clamp((decimal)_settings.GainDb, numGainDb.Minimum, numGainDb.Maximum);
                numHeartbeatTimeoutMs.Value = Clamp(_settings.GevHeartbeatTimeoutMs, (int)numHeartbeatTimeoutMs.Minimum, (int)numHeartbeatTimeoutMs.Maximum);
                numJpegQuality.Value = Clamp(_settings.JpegQuality, (int)numJpegQuality.Minimum, (int)numJpegQuality.Maximum);
                chkAutoPacketSize.Checked = _settings.UseAutoPacketSize;
            }
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
            if (numDeviceIndex != null)
            {
                _settings.DeviceIndex = (int)numDeviceIndex.Value;
                _settings.ExposureTimeUs = (float)numExposureTimeUs.Value;
                _settings.GainDb = (float)numGainDb.Value;
                _settings.GevHeartbeatTimeoutMs = (int)numHeartbeatTimeoutMs.Value;
                _settings.JpegQuality = (int)numJpegQuality.Value;
                _settings.UseAutoPacketSize = chkAutoPacketSize.Checked;
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
