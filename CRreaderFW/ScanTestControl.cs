using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class ScanTestControl : UserControl
    {
        private readonly Timer _scanTimer = new Timer();
        private readonly List<ScanRecord> _currentProductionRecords = new List<ScanRecord>();
        private readonly List<ScanRecord> _testRecords = new List<ScanRecord>();
        private readonly List<OrderMatchItem> _orderMatchItems = new List<OrderMatchItem>();
        private readonly HistoryStore _historyStore;
        private readonly BatchStatisticsWriter _statisticsWriter;
        private IBarcodeCamera _camera;
        private ScannerSettingsData _scannerSettings;
        private ProductCatalog _productCatalog;
        private OrderCatalog _orderCatalog = OrderCatalog.Empty();
        private bool _readerConnected;
        private bool _orderInfoMatched;
        private bool _currentOrderConfirmed;
        private bool _batchActive;
        private string _batchFolderName = string.Empty;
        private string _batchImageDirectory = string.Empty;
        private readonly List<ScanRecord> _batchRecords = new List<ScanRecord>();
        private Button _btnBatchStart;
        private Button _btnBatchEnd;
        private Button _btnImportOrder;
        private ContextMenuStrip _importOrderMenu;
        private CheckBox _chkNoOrderTestMode;
        private CheckBox _chkOrderDirectReadMode;
        private Button _btnDirectReadForPacking;
        private bool _scanActive;
        private int _nextSequence = 1;
        private int _testSequence = 1;
        private SplitContainer _splitMain;

        public event Action<ScannedItemInfo> ScannedItemAdded;
        public event Action ScanItemsCleared;
        public event Action<IReadOnlyList<ScannedItemInfo>> BatchCompleted;

        public ScanTestControl()
        {
            InitializeComponent();
            ApplyCompactLayout();
            InitBatchButtons();
            InitOrderImportButton();
            InitNoOrderTestModeControl();
            InitOrderDirectReadModeControl();
            ApplyVisualStyle();
            ArrangeOrderActionButtons();

            _scannerSettings = ScannerSettingsStore.Load();
            _historyStore = new HistoryStore(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "history"));
            _statisticsWriter = new BatchStatisticsWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "statistics"));
            _historyStore.EnsureCreated();
            LoadProductCatalog();
            LoadOrderCatalog();
            txtOrderBoxInput.TextChanged += OrderBoxInput_TextChanged;

            _scanTimer.Tick += scanTimer_Tick;
            _scanTimer.Interval = _scannerSettings.ScanIntervalMs;

            InitializeRuntimeState();
        }

        private IWin32Window DialogOwner
        {
            get { return FindForm(); }
        }

        private void ApplyCompactLayout()
        {
            panelTop.SuspendLayout();
            panelTop.Controls.Clear();
            panelTop.AutoSize = true;
            panelTop.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panelTop.Dock = DockStyle.Top;
            panelTop.Padding = new Padding(4);
            foreach (Control group in new Control[] { grpCurrentOrder, grpWorkState, grpDeviceActions, grpWorkMode })
            {
                group.Dock = DockStyle.Top;
                group.Margin = new Padding(0, 0, 0, 4);
                group.MinimumSize = new Size(280, 0);
                panelTop.Controls.Add(group);
            }
            panelTop.ResumeLayout(false);
            ArrangeDeviceActionButtons();
            ArrangeOrderActionButtons();
            grpDeviceActions.Resize += (s, e) => ArrangeDeviceActionButtons();
            grpCurrentOrder.Resize += (s, e) => ArrangeOrderActionButtons();

            panelUpper.SuspendLayout();
            panelUpper.Controls.Clear();
            panelUpper.AutoSize = true;
            panelUpper.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            panelUpper.Dock = DockStyle.Top;
            panelUpper.Padding = new Padding(4, 0, 4, 4);
            grpScanImage.Dock = DockStyle.Top;
            grpScanImage.Margin = new Padding(0, 0, 0, 4);
            grpScanImage.MinimumSize = new Size(280, 0);
            picScanImage.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            picScanImage.Height = 80;
            picScanImage.Width = grpScanImage.ClientSize.Width - 18;
            grpCurrentResult.Dock = DockStyle.Top;
            grpCurrentResult.Margin = new Padding(0, 0, 0, 4);
            grpCurrentResult.MinimumSize = new Size(280, 0);
            grpAlarmSummary.Dock = DockStyle.Top;
            grpAlarmSummary.Margin = new Padding(0, 0, 0, 4);
            grpAlarmSummary.MinimumSize = new Size(280, 0);
            panelUpper.Controls.Add(grpScanImage);
            panelUpper.Controls.Add(grpCurrentResult);
            panelUpper.Controls.Add(grpAlarmSummary);
            panelUpper.ResumeLayout(false);

            splitCurrent.Orientation = Orientation.Horizontal;
            splitCurrent.Panel1MinSize = 50;
            splitCurrent.Panel2MinSize = 50;
            splitCurrent.FixedPanel = FixedPanel.None;
            splitHistory.Orientation = Orientation.Horizontal;
            splitHistory.Panel1MinSize = 50;
            splitHistory.Panel2MinSize = 50;
            splitHistory.FixedPanel = FixedPanel.None;

            statusStripMain.Dock = DockStyle.Bottom;
            tabMain.Dock = DockStyle.Fill;
            tabMain.MinimumSize = new Size(0, 120);

            Controls.Remove(panelTop);
            Controls.Remove(panelUpper);
            Controls.Remove(tabMain);

            var headerScroll = new Panel
            {
                Name = "panelHeaderScroll",
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(0)
            };
            headerScroll.Controls.Add(panelUpper);
            headerScroll.Controls.Add(panelTop);

            _splitMain = new SplitContainer
            {
                Name = "splitMain",
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterWidth = 4,
                Panel1MinSize = 120,
                Panel2MinSize = 100
            };
            _splitMain.Panel1.Controls.Add(headerScroll);
            _splitMain.Panel2.Controls.Add(tabMain);

            Controls.Add(_splitMain);
            Controls.Add(statusStripMain);
            Controls.SetChildIndex(statusStripMain, Controls.Count - 1);

            Load += ScanTestControl_Load;
            Resize += ScanTestControl_Resize;
        }

        private void ScanTestControl_Load(object sender, EventArgs e)
        {
            AdjustLayoutSplits();
            LoadDeferredOrderSources();
        }

        private void ScanTestControl_Resize(object sender, EventArgs e)
        {
            AdjustLayoutSplits();
        }

        private void AdjustLayoutSplits()
        {
            if (_splitMain == null || !_splitMain.IsHandleCreated)
            {
                return;
            }

            int totalHeight = ClientSize.Height - statusStripMain.Height;
            if (totalHeight <= _splitMain.Panel1MinSize + _splitMain.Panel2MinSize + _splitMain.SplitterWidth)
            {
                return;
            }

            const int tabAreaHeight = 228;
            int headerHeight = totalHeight - tabAreaHeight;
            headerHeight = Math.Max(220, headerHeight);
            headerHeight = Math.Min(headerHeight, totalHeight - _splitMain.Panel2MinSize - _splitMain.SplitterWidth);
            if (_splitMain.SplitterDistance != headerHeight)
            {
                _splitMain.SplitterDistance = headerHeight;
            }

            AdjustListSplitEqual(splitCurrent);
            AdjustListSplitEqual(splitHistory);
        }

        private void InitBatchButtons()
        {
            _btnBatchStart = new Button
            {
                Name = "btnBatchStart",
                Text = "批次开始",
                UseVisualStyleBackColor = true
            };
            _btnBatchEnd = new Button
            {
                Name = "btnBatchEnd",
                Text = "批次结束",
                UseVisualStyleBackColor = true,
                Enabled = false
            };
            StyleActionButton(_btnBatchStart, Color.FromArgb(32, 95, 178), Color.White);
            StyleActionButton(_btnBatchEnd, Color.FromArgb(120, 82, 46), Color.White);
            _btnBatchStart.Click += btnBatchStart_Click;
            _btnBatchEnd.Click += btnBatchEnd_Click;
            grpDeviceActions.Controls.Add(_btnBatchStart);
            grpDeviceActions.Controls.Add(_btnBatchEnd);
        }

        private void ApplyVisualStyle()
        {
            var uiFont = new Font("Microsoft YaHei UI", 9F);
            var uiFontBold = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            BackColor = Color.FromArgb(245, 247, 250);
            panelTop.BackColor = BackColor;
            panelUpper.BackColor = BackColor;
            tabMain.Font = uiFont;

            grpWorkMode.Text = "运行模式";
            grpDeviceActions.Font = uiFontBold;
            grpCurrentOrder.Font = uiFontBold;
            grpWorkState.Font = uiFontBold;
            grpAlarmSummary.Font = uiFontBold;
            grpCurrentResult.Font = uiFontBold;
            grpScanImage.Font = uiFontBold;

            StyleActionButton(btnConnectReader, Color.FromArgb(39, 103, 73), Color.White);
            StyleActionButton(btnReaderSettings, Color.FromArgb(70, 85, 105), Color.White);
            StyleActionButton(btnStartTestScan, Color.FromArgb(32, 95, 178), Color.White);
            StyleActionButton(btnStopCurrentScan, Color.FromArgb(178, 52, 52), Color.White);
            StyleActionButton(btnWaitRobotSignal, Color.FromArgb(32, 95, 178), Color.White);
            StyleActionButton(btnOpenOrderMatch, Color.FromArgb(32, 95, 178), Color.White);
            StyleActionButton(btnConfirmOrderBox, Color.FromArgb(39, 103, 73), Color.White);
            if (_btnImportOrder != null)
            {
                StyleActionButton(_btnImportOrder, Color.FromArgb(70, 85, 105), Color.White);
            }
            if (_btnDirectReadForPacking != null)
            {
                StyleActionButton(_btnDirectReadForPacking, Color.FromArgb(39, 103, 73), Color.White);
            }

            StyleStatusBadge(lblReaderStateValue);
            StyleStatusBadge(lblScanStateValue);
            StyleStatusBadge(lblRobotStateValue);
            StyleStatusBadge(lblCamera3DStateValue);
            StyleStatusBadge(lblResultOrderNoValue);
            StyleStatusBadge(lblResultBatchBoxValue);
            StyleStatusBadge(lblResultBarcodeValue);
            StyleStatusBadge(lblResultSkuValue);
            StyleStatusBadge(lblResultStatusValue);

            lblModeHint.ForeColor = Color.FromArgb(90, 96, 110);
            lblModeHint.Font = uiFont;
            rdoProductionMode.Font = uiFontBold;
            rdoTestMode.Font = uiFontBold;

            picScanImage.BackColor = Color.FromArgb(235, 238, 242);
            picScanImage.BorderStyle = BorderStyle.FixedSingle;
            txtLog.BackColor = Color.FromArgb(252, 252, 253);
            txtLog.Font = new Font("Consolas", 9F);
        }

        private static void StyleActionButton(Button button, Color backColor, Color foreColor)
        {
            if (button == null)
            {
                return;
            }

            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = backColor;
            button.ForeColor = foreColor;
            button.FlatAppearance.BorderSize = 0;
            button.Cursor = Cursors.Hand;
            button.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
        }

        private static void StyleStatusBadge(Label label)
        {
            if (label == null)
            {
                return;
            }

            label.BackColor = Color.White;
            label.BorderStyle = BorderStyle.FixedSingle;
            label.Padding = new Padding(2, 0, 2, 0);
        }

        private void InitOrderImportButton()
        {
            _importOrderMenu = new ContextMenuStrip();
            var menuImportFile = new ToolStripMenuItem("选择订单文件...");
            menuImportFile.Click += (s, e) => ImportOrderFromFileDialog();
            var menuImportFolder = new ToolStripMenuItem("选择订单文件夹...");
            menuImportFolder.Click += (s, e) => ImportOrderFromFolderDialog();
            _importOrderMenu.Items.Add(menuImportFile);
            _importOrderMenu.Items.Add(menuImportFolder);

            _btnImportOrder = new Button
            {
                Name = "btnImportOrder",
                Text = "读取订单 ▾",
                UseVisualStyleBackColor = true
            };
            _btnImportOrder.Click += btnImportOrder_Click;
            grpCurrentOrder.Controls.Add(_btnImportOrder);
        }

        private void btnImportOrder_Click(object sender, EventArgs e)
        {
            if (_importOrderMenu != null)
            {
                _importOrderMenu.Show(_btnImportOrder, new Point(0, _btnImportOrder.Height));
            }
        }

        private void ImportOrderFromFileDialog()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "订单文件 (*.xls;*.xlsx;*.csv;*.txt;*.json)|*.xls;*.xlsx;*.csv;*.txt;*.json|所有文件 (*.*)|*.*";
                dialog.Title = "读取订单文件（文件名格式：订单号-箱码编号）";
                if (dialog.ShowDialog(DialogOwner) == DialogResult.OK)
                {
                    ImportOrdersFromPath(dialog.FileName);
                }
            }
        }

        private void ImportOrderFromFolderDialog()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择订单文件夹（只读，不修改源文件；文件名格式：订单号-箱码编号）";
                if (dialog.ShowDialog(DialogOwner) == DialogResult.OK)
                {
                    ImportOrdersFromPath(dialog.SelectedPath);
                }
            }
        }

        private void InitNoOrderTestModeControl()
        {
            _chkNoOrderTestMode = new CheckBox
            {
                Name = "chkNoOrderTestMode",
                Text = "无订单测试（跳过订单匹配，批次结束导出统计）",
                AutoSize = true,
                UseVisualStyleBackColor = true
            };
            _chkNoOrderTestMode.CheckedChanged += chkNoOrderTestMode_CheckedChanged;
            grpCurrentOrder.Controls.Add(_chkNoOrderTestMode);
        }

        private bool IsNoOrderTestMode
        {
            get { return !IsProductionMode && _chkNoOrderTestMode != null && _chkNoOrderTestMode.Checked; }
        }

        private bool IsOrderDirectReadMode
        {
            get { return !IsProductionMode && _chkOrderDirectReadMode != null && _chkOrderDirectReadMode.Checked; }
        }

        private void InitOrderDirectReadModeControl()
        {
            _chkOrderDirectReadMode = new CheckBox
            {
                Name = "chkOrderDirectReadMode",
                Text = "订单直读（输入箱码，订单明细直接载入装箱）",
                AutoSize = true,
                UseVisualStyleBackColor = true
            };
            _chkOrderDirectReadMode.CheckedChanged += chkOrderDirectReadMode_CheckedChanged;

            _btnDirectReadForPacking = new Button
            {
                Name = "btnDirectReadForPacking",
                Text = "直读取装箱",
                UseVisualStyleBackColor = true,
                Visible = false
            };
            _btnDirectReadForPacking.Click += btnDirectReadForPacking_Click;

            grpCurrentOrder.Controls.Add(_chkOrderDirectReadMode);
            grpCurrentOrder.Controls.Add(_btnDirectReadForPacking);
        }

        private void ArrangeOrderActionButtons()
        {
            if (grpCurrentOrder == null)
            {
                return;
            }

            int clientW = Math.Max(grpCurrentOrder.ClientSize.Width, 280);
            const int left = 10;
            const int labelW = 58;
            const int btnW = 88;
            int btnX = Math.Max(clientW - btnW - 10, 190);
            int fieldX = left + labelW;
            int fieldW = Math.Max(btnX - fieldX - 8, 80);

            rdoInputByOrderNo.Location = new Point(left, 22);
            rdoInputByBoxCode.Location = new Point(left + 112, 22);

            lblOrderBoxInput.SetBounds(left, 45, labelW, 18);
            txtOrderBoxInput.SetBounds(fieldX, 42, fieldW, 21);
            btnOpenOrderMatch.SetBounds(btnX, 40, btnW, 26);

            lblMatchedOrderNoTitle.SetBounds(left, 69, labelW, 18);
            lblMatchedOrderNoValue.SetBounds(fieldX, 66, fieldW, 18);
            btnConfirmOrderBox.SetBounds(btnX, 64, btnW, 26);

            lblMatchedBoxCodeTitle.SetBounds(left, 93, labelW, 18);
            lblMatchedBoxCodeValue.SetBounds(fieldX, 90, fieldW, 18);

            if (_btnImportOrder != null)
            {
                _btnImportOrder.SetBounds(left, 116, btnW + 12, 26);
            }

            if (_btnDirectReadForPacking != null)
            {
                _btnDirectReadForPacking.SetBounds(btnX, 112, btnW, 26);
                _btnDirectReadForPacking.Visible = IsOrderDirectReadMode;
            }

            if (_chkNoOrderTestMode != null)
            {
                _chkNoOrderTestMode.AutoSize = true;
                _chkNoOrderTestMode.Location = new Point(left, 148);
                _chkNoOrderTestMode.MaximumSize = new Size(clientW - left * 2, 0);
            }

            if (_chkOrderDirectReadMode != null)
            {
                _chkOrderDirectReadMode.AutoSize = true;
                _chkOrderDirectReadMode.Location = new Point(left, 174);
                _chkOrderDirectReadMode.MaximumSize = new Size(clientW - left * 2, 0);
            }

            btnOpenOrderMatch.Visible = !IsOrderDirectReadMode;
            btnConfirmOrderBox.Visible = !IsOrderDirectReadMode;

            const int requiredHeight = 206;
            grpCurrentOrder.MinimumSize = new Size(280, requiredHeight);
            if (grpCurrentOrder.Height < requiredHeight)
            {
                grpCurrentOrder.Height = requiredHeight;
            }
        }

        private void ArrangeDeviceActionButtons()
        {
            const int left = 10;
            const int top = 22;
            const int rowHeight = 33;
            const int gap = 6;
            int width = Math.Max(grpDeviceActions.ClientSize.Width - left * 2, 260);
            int colWidth = (width - gap) / 2;
            int row = 0;

            btnConnectReader.SetBounds(left, top + rowHeight * row, colWidth, 29);
            btnReaderSettings.SetBounds(left + colWidth + gap, top + rowHeight * row, colWidth, 29);
            row++;

            if (IsProductionMode)
            {
                btnWaitRobotSignal.SetBounds(left, top + rowHeight * row, width, 29);
                row++;
            }
            else
            {
                if (_btnBatchStart != null && _btnBatchEnd != null)
                {
                    _btnBatchStart.SetBounds(left, top + rowHeight * row, colWidth, 29);
                    _btnBatchEnd.SetBounds(left + colWidth + gap, top + rowHeight * row, colWidth, 29);
                    row++;
                }

                btnStartTestScan.SetBounds(left, top + rowHeight * row, width, 29);
                row++;
                btnStopCurrentScan.SetBounds(left, top + rowHeight * row, width, 29);
                row++;
            }

            int requiredHeight = top + rowHeight * row + 10;
            if (grpDeviceActions.Height < requiredHeight)
            {
                grpDeviceActions.Height = requiredHeight;
            }
        }

        private static void AdjustListSplitEqual(SplitContainer split)
        {
            if (split == null || !split.IsHandleCreated)
            {
                return;
            }

            int available = split.Height - split.SplitterWidth;
            if (available < split.Panel1MinSize + split.Panel2MinSize)
            {
                return;
            }

            int half = available / 2;
            if (split.SplitterDistance != half)
            {
                split.SplitterDistance = half;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _scanTimer.Stop();
                _scanTimer.Dispose();
                if (_camera != null)
                {
                    _camera.Dispose();
                    _camera = null;
                }
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private bool IsProductionMode
        {
            get { return rdoProductionMode.Checked; }
        }

        private string CurrentOrderNo
        {
            get { return ReadMatchedLabel(lblMatchedOrderNoValue); }
        }

        private string CurrentBoxCode
        {
            get { return ReadMatchedLabel(lblMatchedBoxCodeValue); }
        }

        private static string ReadMatchedLabel(Label label)
        {
            string value = label.Text.Trim();
            return value == "-" ? string.Empty : value;
        }

        private void InitializeRuntimeState()
        {
            ConfigureGridStyles();
            UpdateModeState();
            UpdateStatusLabels("未连接", "未开始", "未等待", "未检测");
            RefreshCurrentTables();
            RefreshTestTables();
            RefreshSummaryPanels();
            AppendLog("程序启动。");
            UpdateOrderActionState();
        }

        private void LoadOrderCatalog()
        {
            try
            {
                string orderInfoPath = ResolveOrderInfoPath();
                _orderCatalog = orderInfoPath != null
                    ? OrderCatalog.Load(orderInfoPath)
                    : OrderCatalog.Empty();

                if (_orderCatalog.Count > 0)
                {
                    AppendLog("已读取订单信息：" + orderInfoPath + "，共 " + _orderCatalog.Count + " 条。");
                }
            }
            catch (Exception ex)
            {
                _orderCatalog = OrderCatalog.Empty();
                AppendLog("加载订单信息失败：" + ex.Message);
            }
        }

        private void LoadDeferredOrderSources()
        {
            string testOrdersDir = ResolveTestOrdersDirectory();
            if (testOrdersDir == null)
            {
                if (_orderCatalog.Count == 0)
                {
                    AppendLog("未找到订单文件，可通过读取订单或放置测试订单文件夹。");
                }
                return;
            }

            MergeOrderPath(testOrdersDir, "测试订单文件夹");
            if (_orderCatalog.Count > 0)
            {
                AppendLog("订单目录已就绪，共 " + _orderCatalog.Count + " 条明细。");
            }
        }

        private void MergeOrderPath(string path, string sourceLabel)
        {
            if (Directory.Exists(path))
            {
                foreach (string filePath in EnumerateOrderFiles(path))
                {
                    TryMergeOrderCatalog(OrderCatalog.Load(filePath), filePath);
                }
                return;
            }

            TryMergeOrderCatalog(OrderCatalog.Load(path), sourceLabel ?? path);
        }

        private static IEnumerable<string> EnumerateOrderFiles(string folderPath)
        {
            foreach (string path in Directory.EnumerateFiles(folderPath))
            {
                string extension = Path.GetExtension(path).ToLowerInvariant();
                if (extension == ".csv" || extension == ".txt" || extension == ".tsv" ||
                    extension == ".xls" || extension == ".xlsx" || extension == ".json")
                {
                    yield return path;
                }
            }
        }

        private void TryMergeOrderCatalog(OrderCatalog incoming, string sourceDescription)
        {
            if (incoming == null || incoming.Count == 0)
            {
                return;
            }

            IReadOnlyList<OrderCatalogConflict> conflicts = OrderCatalog.FindConflicts(_orderCatalog, incoming);
            bool overwrite = false;
            if (conflicts.Count > 0)
            {
                string message = "检测到订单与箱码冲突（一个订单只能对应一个箱码）：" +
                    Environment.NewLine + Environment.NewLine +
                    string.Join(Environment.NewLine, conflicts.Select(c => "· " + c.Describe())) +
                    Environment.NewLine + Environment.NewLine +
                    "来源：" + sourceDescription + Environment.NewLine + Environment.NewLine +
                    "是否覆盖已有记录？选择「否」将跳过本次冲突的新记录。";
                overwrite = MessageBox.Show(
                    DialogOwner,
                    message,
                    "订单冲突",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning) == DialogResult.Yes;
            }

            int beforeCount = _orderCatalog.Count;
            _orderCatalog = OrderCatalog.Merge(_orderCatalog, incoming, overwrite);
            int added = _orderCatalog.Count - beforeCount;
            if (conflicts.Count == 0)
            {
                AppendLog("已读取订单：" + sourceDescription + "，新增 " + added + " 条（源文件未修改）。");
            }
            else
            {
                AppendLog("已处理订单：" + sourceDescription + "，冲突 " + conflicts.Count + " 项，" +
                    (overwrite ? "已覆盖" : "已跳过冲突项") + "，当前共 " + _orderCatalog.Count + " 条。");
            }
        }

        private static string ResolveTestOrdersDirectory()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] candidates =
            {
                Path.Combine(baseDir, "测试订单"),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "测试订单"))
            };

            foreach (string path in candidates)
            {
                if (Directory.Exists(path))
                {
                    return path;
                }
            }

            return null;
        }

        private static string ResolveOrderInfoPath()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] names =
            {
                "OrderInfo.xls",
                "OrderInfo.xlsx",
                "OrderInfo.csv",
                "OrderInfo.tsv",
                "OrderInfo.txt"
            };

            foreach (string name in names)
            {
                string path = Path.Combine(baseDir, name);
                if (File.Exists(path))
                {
                    return path;
                }
            }

            string projectCopy = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "OrderInfo.xls"));
            return File.Exists(projectCopy) ? projectCopy : null;
        }

        private void OrderBoxInput_TextChanged(object sender, EventArgs e)
        {
            ResetMatchedOrderState();
        }

        private void ResetMatchedOrderState()
        {
            _orderInfoMatched = false;
            _currentOrderConfirmed = false;
            lblMatchedOrderNoValue.Text = "-";
            lblMatchedBoxCodeValue.Text = "-";
            _orderMatchItems.Clear();
            RefreshActiveScanTables();
            RefreshSummaryPanels();
            UpdateOrderActionState();
        }

        private void UpdateOrderActionState()
        {
            bool orderControlsEnabled = !IsNoOrderTestMode && !IsOrderDirectReadMode;
            bool directReadControlsEnabled = IsOrderDirectReadMode;
            btnOpenOrderMatch.Enabled = orderControlsEnabled;
            if (_btnImportOrder != null)
            {
                _btnImportOrder.Enabled = !IsNoOrderTestMode;
            }
            rdoInputByOrderNo.Enabled = orderControlsEnabled;
            rdoInputByBoxCode.Enabled = orderControlsEnabled || directReadControlsEnabled;
            txtOrderBoxInput.Enabled = orderControlsEnabled || directReadControlsEnabled;
            btnConfirmOrderBox.Enabled = orderControlsEnabled && _orderInfoMatched && !_currentOrderConfirmed;
            if (_btnDirectReadForPacking != null)
            {
                _btnDirectReadForPacking.Enabled = directReadControlsEnabled;
                _btnDirectReadForPacking.Visible = directReadControlsEnabled;
            }
            if (_chkNoOrderTestMode != null)
            {
                _chkNoOrderTestMode.Enabled = !IsProductionMode && !IsOrderDirectReadMode;
            }
            if (_chkOrderDirectReadMode != null)
            {
                _chkOrderDirectReadMode.Enabled = !IsProductionMode && !IsNoOrderTestMode;
            }
            btnOpenOrderMatch.Visible = !IsOrderDirectReadMode;
            btnConfirmOrderBox.Visible = !IsOrderDirectReadMode;
        }

        private void chkOrderDirectReadMode_CheckedChanged(object sender, EventArgs e)
        {
            if (IsProductionMode)
            {
                return;
            }

            if (_chkOrderDirectReadMode.Checked && _chkNoOrderTestMode != null && _chkNoOrderTestMode.Checked)
            {
                _chkNoOrderTestMode.Checked = false;
            }

            if (_batchActive)
            {
                EndBatchInternal(false);
            }

            if (IsOrderDirectReadMode)
            {
                ResetMatchedOrderState();
                _currentOrderConfirmed = false;
                _testRecords.Clear();
                _testSequence = 1;
                rdoInputByBoxCode.Checked = true;
                UpdateStatusLabels(null, "订单直读", "等待输入箱码", "等待检测");
                AppendLog("已切换为订单直读：输入箱码后点击「直读取装箱」，订单明细将直接作为装箱输入。");
            }
            else
            {
                UpdateStatusLabels(null, "未开始", null, null);
                AppendLog("已退出订单直读模式。");
            }

            UpdateOrderActionState();
            ArrangeOrderActionButtons();
            RefreshActiveScanTables();
            RefreshSummaryPanels();
        }

        private void btnDirectReadForPacking_Click(object sender, EventArgs e)
        {
            ExecuteOrderDirectRead();
        }

        private void ExecuteOrderDirectRead()
        {
            if (!IsOrderDirectReadMode)
            {
                return;
            }

            string boxCode = txtOrderBoxInput.Text.Trim();
            if (boxCode.Length == 0)
            {
                MessageBox.Show(DialogOwner, "请输入箱码编号后再直读。", "订单直读", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            OrderLookupResult lookup = LookupOrderInfo(boxCode, false);
            if (lookup == null)
            {
                MessageBox.Show(DialogOwner, "未找到该箱码对应的订单，请先读取订单文件。", "订单直读", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ResetMatchedOrderState();
                return;
            }

            var orderItems = new List<OrderMatchItem>(lookup.Items);
            EnrichMatchItemsFromProductCatalog(orderItems);
            List<ScannedItemInfo> items = BuildScannedItemsFromOrder(orderItems, out int skippedLines);
            if (items.Count == 0)
            {
                MessageBox.Show(DialogOwner, "订单明细无法转换为装箱物品，请检查订单文件尺寸或 ProductInfo。", "订单直读",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            lblMatchedOrderNoValue.Text = lookup.OrderNo;
            lblMatchedBoxCodeValue.Text = lookup.BoxCode;
            _orderMatchItems.Clear();
            _orderMatchItems.AddRange(orderItems);
            _orderInfoMatched = true;
            _currentOrderConfirmed = false;

            _testRecords.Clear();
            _testSequence = 1;
            foreach (ScannedItemInfo scanned in items)
            {
                var record = new ScanRecord
                {
                    Mode = "订单直读",
                    OrderNo = lookup.OrderNo,
                    BatchBoxCode = lookup.BoxCode,
                    Sequence = _testSequence++,
                    Barcode = scanned.Barcode,
                    Sku = scanned.Name,
                    Length = scanned.Dx.ToString(),
                    Width = scanned.Dy.ToString(),
                    Height = scanned.Dz.ToString(),
                    Status = "直读载入",
                    ScanCount = 1,
                    ScanTime = DateTime.Now,
                    ImagePath = string.Empty
                };
                _testRecords.Add(record);
            }

            ScanItemsCleared?.Invoke();
            foreach (ScannedItemInfo item in items)
            {
                ScannedItemAdded?.Invoke(item);
            }
            BatchCompleted?.Invoke(items);

            RefreshActiveScanTables();
            RefreshSummaryPanels();
            UpdateStatusLabels(null, "已载入装箱", "直读完成", null);
            AppendLog("订单直读：箱码 " + lookup.BoxCode + "，订单 " + lookup.OrderNo + "，已载入 " + items.Count + " 件到装箱算法。");
            if (skippedLines > 0)
            {
                AppendLog("订单直读：跳过 " + skippedLines + " 条缺少有效尺寸的明细。");
            }

            string skipNote = skippedLines > 0 ? "\n另有 " + skippedLines + " 条明细因尺寸无效被跳过。" : string.Empty;
            MessageBox.Show(DialogOwner,
                "已从箱码 " + lookup.BoxCode + " 直读 " + items.Count + " 件物品到待装列表。" + skipNote,
                "订单直读", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private List<ScannedItemInfo> BuildScannedItemsFromOrder(List<OrderMatchItem> orderItems, out int skippedLines)
        {
            skippedLines = 0;
            var items = new List<ScannedItemInfo>();
            if (orderItems == null)
            {
                return items;
            }

            foreach (OrderMatchItem orderItem in orderItems)
            {
                if (orderItem == null ||
                    (string.IsNullOrWhiteSpace(orderItem.Barcode) && string.IsNullOrWhiteSpace(orderItem.Sku)))
                {
                    continue;
                }

                ProductRecord product = ResolveProduct(orderItem.Barcode, orderItem.Sku);
                string barcode = FirstNonEmpty(
                    product == null ? string.Empty : product.Barcode,
                    orderItem.Barcode);
                string sku = FirstNonEmpty(orderItem.Sku, product == null ? string.Empty : product.GetValue("SKU", "Sku", "sku", "货号"));
                string length = FirstNonEmpty(orderItem.Length, GetProductLength(product));
                string width = FirstNonEmpty(orderItem.Width, GetProductWidth(product));
                string height = FirstNonEmpty(orderItem.Height, GetProductHeight(product));
                ScannedItemInfo template = BuildScannedItemInfo(product, sku, length, width, height, barcode);
                if (template.Dx <= 0 || template.Dy <= 0 || template.Dz <= 0)
                {
                    skippedLines++;
                    continue;
                }

                int qty = orderItem.OrderQuantity <= 0 ? 1 : orderItem.OrderQuantity;
                string baseName = !string.IsNullOrWhiteSpace(sku)
                    ? sku.Trim()
                    : (!string.IsNullOrWhiteSpace(template.Name) ? template.Name.Trim() : orderItem.Barcode);
                for (int i = 0; i < qty; i++)
                {
                    items.Add(new ScannedItemInfo
                    {
                        Name = qty > 1 ? baseName + "#" + (i + 1) : baseName,
                        Dimensions = template.Dimensions,
                        Barcode = template.Barcode,
                        Dx = template.Dx,
                        Dy = template.Dy,
                        Dz = template.Dz
                    });
                }
            }

            return items;
        }

        private void chkNoOrderTestMode_CheckedChanged(object sender, EventArgs e)
        {
            if (IsProductionMode)
            {
                return;
            }

            if (_batchActive)
            {
                EndBatchInternal(false);
            }

            if (IsNoOrderTestMode)
            {
                if (_chkOrderDirectReadMode != null && _chkOrderDirectReadMode.Checked)
                {
                    _chkOrderDirectReadMode.Checked = false;
                }
                ResetMatchedOrderState();
                _currentOrderConfirmed = false;
                _testRecords.Clear();
                _testSequence = 1;
                UpdateStatusLabels(null, "无订单测试", "等待开始批次", "等待检测");
                AppendLog("已切换为无订单测试：无需匹配订单，开始批次后即可扫码，批次结束将导出统计。");
            }
            else
            {
                _currentOrderConfirmed = false;
                UpdateStatusLabels(null, "未开始", null, null);
                AppendLog("已退出无订单测试，请匹配并确认订单后再开始批次。");
            }

            UpdateOrderActionState();
            ArrangeOrderActionButtons();
            RefreshActiveScanTables();
            RefreshSummaryPanels();
        }

        private void LoadProductCatalog()
        {
            string path = ResolveProductInfoPath();
            if (path == null)
            {
                AppendLog("未找到 ProductInfo.xls，产品信息匹配不可用。");
                return;
            }

            try
            {
                _productCatalog = ProductCatalog.Load(path);
                AppendLog("已加载产品信息：" + _productCatalog.Count + " 条，文件：" + path);
            }
            catch (Exception ex)
            {
                _productCatalog = null;
                AppendLog("加载产品信息失败：" + ex.Message);
                MessageBox.Show(DialogOwner, "加载 ProductInfo 失败：" + ex.Message, "产品信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static string ResolveProductInfoPath()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string[] names =
            {
                "ProductInfo.xls",
                "ProductInfo.xlsx",
                "ProductInfo.csv",
                "ProductInfo.tsv",
                "ProductInfo.txt"
            };

            foreach (string name in names)
            {
                string path = Path.Combine(baseDir, name);
                if (File.Exists(path))
                {
                    return path;
                }
            }

            string projectCopy = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "ProductInfo.xls"));
            return File.Exists(projectCopy) ? projectCopy : null;
        }

        private void ConfigureGridStyles()
        {
            ConfigureReadOnlyGrid(dgvProductScanList);
            ConfigureReadOnlyGrid(dgvScanMatchResult);
            ConfigureReadOnlyGrid(dgvHistoryScanList);
            ConfigureReadOnlyGrid(dgvHistoryMatchResult);
            ConfigureReadOnlyGrid(dgvTestRecords);
        }

        private static void ConfigureReadOnlyGrid(DataGridView grid)
        {
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.RowHeadersVisible = false;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.EnableHeadersVisualStyles = false;
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(236, 240, 245);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(55, 65, 81);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
            grid.ColumnHeadersHeight = 30;
            grid.DefaultCellStyle.Font = new Font("Microsoft YaHei UI", 9F);
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(214, 230, 248);
            grid.DefaultCellStyle.SelectionForeColor = Color.Black;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            grid.GridColor = Color.FromArgb(225, 230, 237);
            grid.RowTemplate.Height = 26;
        }

        private void btnConfirmOrderBox_Click(object sender, EventArgs e)
        {
            if (!_orderInfoMatched)
            {
                MessageBox.Show(DialogOwner, "请先点击“匹配信息”，从系统或订单文件中匹配订单与箱码。", "尚未匹配", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string orderNo = ReadMatchedLabel(lblMatchedOrderNoValue);
            string boxCode = ReadMatchedLabel(lblMatchedBoxCodeValue);
            if (orderNo.Length == 0 || boxCode.Length == 0)
            {
                MessageBox.Show(DialogOwner, "没有该订单。", "匹配失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_batchActive)
            {
                if (IsProductionMode)
                {
                    TryEndProductionBatch("切换订单");
                }
                else
                {
                    EndBatchInternal(false);
                }
            }

            _currentOrderConfirmed = true;
            if (IsProductionMode)
            {
                _currentProductionRecords.Clear();
                _nextSequence = 1;
                UpdateStatusLabels(null, "订单已确认", "等待机械臂", "等待检测");
                TryStartProductionBatch();
            }
            else
            {
                _testRecords.Clear();
                _testSequence = 1;
                UpdateStatusLabels(null, "订单已确认", "等待开始批次", "等待检测");
            }

            RefreshActiveScanTables();
            RefreshSummaryPanels();
            UpdateOrderActionState();
            AppendLog("确认订单编号 " + orderNo + "，箱码编号 " + boxCode + "。");
        }

        private void btnOpenOrderMatch_Click(object sender, EventArgs e)
        {
            string input = txtOrderBoxInput.Text.Trim();
            if (input.Length == 0)
            {
                MessageBox.Show(DialogOwner, "请输入订单编号或箱码编号后再匹配。", "匹配信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            ApplyOrderLookup(LookupOrderInfo(input, rdoInputByOrderNo.Checked));
        }

        private void ApplyOrderLookup(OrderLookupResult lookup)
        {
            if (lookup == null)
            {
                MessageBox.Show(DialogOwner, "没有该订单。", "匹配失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ResetMatchedOrderState();
                return;
            }

            lblMatchedOrderNoValue.Text = lookup.OrderNo;
            lblMatchedBoxCodeValue.Text = lookup.BoxCode;
            _orderMatchItems.Clear();
            _orderMatchItems.AddRange(lookup.Items);
            EnrichMatchItemsFromProductCatalog(_orderMatchItems);
            _orderInfoMatched = true;
            _currentOrderConfirmed = false;

            RefreshActiveScanTables();
            RefreshSummaryPanels();
            UpdateOrderActionState();
            if (_orderMatchItems.Count == 0)
            {
                AppendLog("匹配订单编号 " + lookup.OrderNo + "，箱码编号 " + lookup.BoxCode + "，但未找到物品明细。");
                MessageBox.Show(DialogOwner, "已匹配订单与箱码，但未找到该订单的物品明细。请检查订单文件或 ProductInfo。", "匹配信息", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            AppendLog("匹配订单编号 " + lookup.OrderNo + "，箱码编号 " + lookup.BoxCode + "，明细 " + _orderMatchItems.Count + " 条。");
        }

        private void ImportOrdersFromPath(string path)
        {
            try
            {
                MergeOrderPath(path, path);
                ResetMatchedOrderState();
            }
            catch (Exception ex)
            {
                MessageBox.Show(DialogOwner, "读取订单失败：" + ex.Message, "读取订单", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnBatchStart_Click(object sender, EventArgs e)
        {
            if (IsProductionMode)
            {
                return;
            }

            if (_batchActive)
            {
                AppendLog("当前批次已开始，请先结束当前批次。");
                return;
            }

            StartTestBatch();
        }

        private void btnBatchEnd_Click(object sender, EventArgs e)
        {
            if (IsProductionMode)
            {
                return;
            }

            if (!EndBatchInternal(true))
            {
                MessageBox.Show(DialogOwner, "当前没有进行中的批次。", "批次结束", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private bool StartTestBatch()
        {
            if (IsNoOrderTestMode)
            {
                _testRecords.Clear();
                _testSequence = 1;
                _batchFolderName = "test-no-order-" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
                UpdateStatusLabels(null, "批次进行中", null, null);
                return ActivateBatch();
            }

            if (!_currentOrderConfirmed)
            {
                MessageBox.Show(DialogOwner, "请先匹配并确认订单编号和箱码编号。", "测试批次", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            string orderNo = CurrentOrderNo;
            string boxCode = CurrentBoxCode;
            if (orderNo.Length == 0 || boxCode.Length == 0)
            {
                MessageBox.Show(DialogOwner, "当前订单信息无效，请重新匹配。", "测试批次", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            _batchFolderName = BuildProductionBatchName(orderNo, boxCode, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            return ActivateBatch();
        }

        private bool TryStartProductionBatch()
        {
            if (_batchActive || !IsProductionMode || !_currentOrderConfirmed)
            {
                return false;
            }

            string orderNo = CurrentOrderNo;
            string boxCode = CurrentBoxCode;
            if (orderNo.Length == 0 || boxCode.Length == 0)
            {
                return false;
            }

            _batchFolderName = BuildProductionBatchName(orderNo, boxCode, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            return ActivateBatch();
        }

        private bool ActivateBatch()
        {
            string imageRoot = GetImageRootDirectory();
            _batchImageDirectory = Path.Combine(imageRoot, _batchFolderName);
            Directory.CreateDirectory(_batchImageDirectory);
            _batchActive = true;
            _batchRecords.Clear();
            ScanItemsCleared?.Invoke();
            UpdateTestBatchButtonState();
            AppendLog("批次开始：" + _batchFolderName + "，图片目录：" + _batchImageDirectory);
            return true;
        }

        private void TryEndProductionBatch(string reason)
        {
            if (!_batchActive || !IsProductionMode)
            {
                return;
            }

            AppendLog("工作模式批次结束：" + reason + "。");
            EndBatchInternal(false);
        }

        private bool EndBatchInternal(bool showNoBatchMessage)
        {
            if (!_batchActive)
            {
                return !showNoBatchMessage;
            }

            _batchActive = false;
            UpdateTestBatchButtonState();

            string finishedBatchName = _batchFolderName;
            WarnIfBatchHasIssues(finishedBatchName);
            WriteBatchStatistics();
            RaiseBatchCompleted();
            _batchRecords.Clear();
            AppendLog("批次结束：" + finishedBatchName + "。");
            _batchFolderName = string.Empty;
            _batchImageDirectory = string.Empty;
            return true;
        }

        private void UpdateTestBatchButtonState()
        {
            if (_btnBatchStart == null || _btnBatchEnd == null || IsProductionMode)
            {
                return;
            }

            _btnBatchStart.Enabled = !_batchActive;
            _btnBatchEnd.Enabled = _batchActive;
        }

        /// <summary>
        /// 供外部信号调用：机械臂到位后开始采码。
        /// </summary>
        public void OnProductionRobotReadySignal()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(OnProductionRobotReadySignal));
                return;
            }

            HandleProductionRobotSignal();
        }

        /// <summary>
        /// 供外部信号调用：结束当前工作批次。
        /// </summary>
        public void OnProductionBatchEndSignal()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(OnProductionBatchEndSignal));
                return;
            }

            TryEndProductionBatch("收到批次结束信号");
            UpdateStatusLabels(null, "批次已完成", "等待机械臂", "等待检测");
        }

        private void HandleProductionRobotSignal()
        {
            if (!EnsureReadyForProductionScan())
            {
                return;
            }

            if (!_batchActive)
            {
                TryStartProductionBatch();
            }

            UpdateStatusLabels(null, "采码中", "机械臂已到位", "3D相机有物体");
            StartScanCycle("工作模式");
        }

        private bool IsOrderComplete(List<ScanRecord> records)
        {
            if (_orderMatchItems.Count == 0)
            {
                return false;
            }

            foreach (OrderMatchItem expected in _orderMatchItems)
            {
                if (CountRecords(records, expected.Barcode) < expected.OrderQuantity)
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsProductionOrderComplete()
        {
            return IsProductionMode && IsOrderComplete(_currentProductionRecords);
        }

        private string GetImageRootDirectory()
        {
            return string.IsNullOrWhiteSpace(_scannerSettings.ImageSavePath)
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scan_images")
                : _scannerSettings.ImageSavePath;
        }

        private static string BuildProductionBatchName(string orderNo, string boxCode, string timestamp)
        {
            return SanitizePathSegment(orderNo) + "-" + SanitizePathSegment(boxCode) + "-" + timestamp;
        }

        private static string SanitizePathSegment(string value)
        {
            string safe = string.IsNullOrWhiteSpace(value) ? "unknown" : value.Trim();
            foreach (char ch in Path.GetInvalidFileNameChars())
            {
                safe = safe.Replace(ch, '_');
            }
            return safe;
        }

        private void EnrichMatchItemsFromProductCatalog(List<OrderMatchItem> items)
        {
            foreach (OrderMatchItem item in items)
            {
                ProductRecord product = ResolveProduct(item.Barcode, item.Sku);
                if (product == null)
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(product.Barcode))
                {
                    item.Barcode = product.Barcode;
                }

                if (string.IsNullOrWhiteSpace(item.Sku))
                {
                    item.Sku = product.GetValue("SKU", "Sku", "sku", "货号");
                }
                if (string.IsNullOrWhiteSpace(item.Length))
                {
                    item.Length = GetProductLength(product);
                }
                if (string.IsNullOrWhiteSpace(item.Width))
                {
                    item.Width = GetProductWidth(product);
                }
                if (string.IsNullOrWhiteSpace(item.Height))
                {
                    item.Height = GetProductHeight(product);
                }
            }
        }

        private OrderLookupResult LookupOrderInfo(string input, bool inputIsOrderNo)
        {
            if (_orderCatalog != null)
            {
                OrderLookupResult catalogResult = _orderCatalog.Lookup(input, inputIsOrderNo);
                if (catalogResult != null)
                {
                    return catalogResult;
                }
            }

            OrderBoxIdentity identity = ResolveOrderBoxIdentity(input, inputIsOrderNo);
            if (identity == null || identity.OrderNo.Length == 0 || identity.BoxCode.Length == 0)
            {
                return null;
            }

            if (_orderCatalog != null && _orderCatalog.Count > 0)
            {
                OrderLookupResult boundResult = _orderCatalog.Lookup(identity.OrderNo, true);
                if (boundResult == null || !string.Equals(boundResult.BoxCode, identity.BoxCode, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                return boundResult;
            }

            return new OrderLookupResult(identity.OrderNo, identity.BoxCode, new List<OrderMatchItem>());
        }

        private void btnConnectReader_Click(object sender, EventArgs e)
        {
            try
            {
                if (_camera != null)
                {
                    _camera.Dispose();
                    _camera = null;
                }

                _camera = new IdmvsBarcodeCamera(_scannerSettings);
                _camera.BarcodeCaptured += Camera_BarcodeCaptured;
                _camera.Start();
                _readerConnected = true;
                UpdateStatusLabels("已连接真实扫码器", null, null, null);
                AppendLog("扫码器 SDK 连接成功，已进入软件触发采码模式。");
            }
            catch (Exception ex)
            {
                _readerConnected = false;
                UpdateStatusLabels("连接失败", null, null, null);
                AppendLog("扫码器连接失败：" + ex.Message);
                MessageBox.Show(DialogOwner, "扫码器连接失败：" + ex.Message, "扫码器", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnReaderSettings_Click(object sender, EventArgs e)
        {
            using (var dialog = new ScannerSetting(CloneSettings(_scannerSettings)))
            {
                if (dialog.ShowDialog(DialogOwner) != DialogResult.OK)
                {
                    return;
                }

                _scannerSettings = dialog.Settings;
                _scannerSettings.Normalize();
                ScannerSettingsStore.Save(_scannerSettings);
                _scanTimer.Interval = _scannerSettings.ScanIntervalMs;
                AppendLog("扫码器设置已保存：IP=" + _scannerSettings.ReaderIp + "，间隔=" + _scannerSettings.ScanIntervalMs + "ms，曝光=" + _scannerSettings.ExposureTimeUs + "us，自动对焦=" + (_scannerSettings.AutoFocus ? "是" : "否") + "。");
            }
        }

        private void btnWaitRobotSignal_Click(object sender, EventArgs e)
        {
            HandleProductionRobotSignal();
        }

        private void btnStartTestScan_Click(object sender, EventArgs e)
        {
            if (IsProductionMode)
            {
                MessageBox.Show(DialogOwner, "请先切换到测试模式。", "当前模式", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!EnsureReadyForTestScan())
            {
                return;
            }

            StartScanCycle("测试模式");
        }

        private bool EnsureReadyForTestScan()
        {
            if (!EnsureReaderConnected())
            {
                return false;
            }
            if (!_batchActive)
            {
                MessageBox.Show(DialogOwner, "请先开始批次。", "测试采码", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (IsNoOrderTestMode)
            {
                return true;
            }
            if (!_currentOrderConfirmed)
            {
                MessageBox.Show(DialogOwner, "请先匹配并确认订单编号和箱码编号，或勾选无订单测试。", "测试采码", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void btnStopCurrentScan_Click(object sender, EventArgs e)
        {
            if (IsProductionMode)
            {
                return;
            }

            StopScanCycle("人工停止");
        }

        private void btnSearchHistory_Click(object sender, EventArgs e)
        {
            string key = txtHistorySearchKey.Text.Trim();
            if (key.Length == 0)
            {
                MessageBox.Show(DialogOwner, "请输入订单编号或箱码编号后再查询。", "历史查询", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string orderNo = rdoSearchByOrderNo.Checked ? key : string.Empty;
            string boxCode = rdoSearchByBoxCode.Checked ? key : string.Empty;
            List<ScanRecord> records = _historyStore.Search(orderNo, boxCode)
                .Where(r => string.Equals(r.Mode, "工作模式", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(r.Mode, "生产模式", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(r.Mode, "测试模式", StringComparison.OrdinalIgnoreCase))
                .OrderBy(r => r.Sequence)
                .ToList();

            BindScanRecordGrid(dgvHistoryScanList, records);
            BindMatchGrid(dgvHistoryMatchResult, BuildMatchSummary(records, _orderMatchItems, false));
            lblHistoryResult.Text = records.Count == 0 ? "未找到该订单编号/箱码编号的采码历史。" : "查询到 " + records.Count + " 条采码记录。";
            AppendLog("历史查询：订单编号=" + orderNo + "，箱码编号=" + boxCode + "，结果=" + records.Count + " 条。");
        }

        private void rdoProductionMode_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoProductionMode.Checked)
            {
                OnModeChanged();
            }
        }

        private void rdoTestMode_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoTestMode.Checked)
            {
                OnModeChanged();
            }
        }

        private void OnModeChanged()
        {
            if (_scanActive)
            {
                StopScanCycle("切换运行模式");
            }

            if (_batchActive)
            {
                AppendLog("切换运行模式，自动结束当前批次。");
                EndBatchInternal(false);
            }

            UpdateModeState();
        }

        private void scanTimer_Tick(object sender, EventArgs e)
        {
            if (!_scanActive)
            {
                return;
            }

            try
            {
                if (_camera != null)
                {
                    _camera.TriggerOnce();
                }
            }
            catch (Exception ex)
            {
                StopScanCycle("触发失败");
                AppendLog("触发扫码失败：" + ex.Message);
                MessageBox.Show(DialogOwner, "触发扫码失败：" + ex.Message, "扫码器", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void dgvProductScanList_SelectionChanged(object sender, EventArgs e)
        {
            ScanRecord record = GetSelectedRecord(dgvProductScanList, _currentProductionRecords);
            if (record != null)
            {
                ShowRecord(record, "当前采码");
            }
        }

        private void dgvHistoryScanList_SelectionChanged(object sender, EventArgs e)
        {
            ScanRecord record = GetSelectedRecord(dgvHistoryScanList, null);
            if (record != null)
            {
                ShowRecord(record, "历史回溯");
            }
        }

        private void dgvTestRecords_SelectionChanged(object sender, EventArgs e)
        {
            ScanRecord record = GetSelectedRecord(dgvTestRecords, _testRecords);
            if (record != null)
            {
                ShowRecord(record, "测试记录");
            }
        }

        private bool EnsureReadyForProductionScan()
        {
            if (!EnsureReaderConnected())
            {
                return false;
            }
            if (!_currentOrderConfirmed)
            {
                MessageBox.Show(DialogOwner, "请先确认订单编号和箱码编号。", "工作采码", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private bool EnsureReaderConnected()
        {
            if (_readerConnected)
            {
                return true;
            }

            MessageBox.Show(DialogOwner, "请先连接扫码器。", "扫码器未连接", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private void StartScanCycle(string mode)
        {
            if (_scanActive)
            {
                AppendLog("当前已有扫码任务在运行。");
                return;
            }

            _scanActive = true;
            _scanTimer.Interval = _scannerSettings.ScanIntervalMs;
            _scanTimer.Start();
            if (!IsProductionMode)
            {
                btnStopCurrentScan.Enabled = true;
            }
            UpdateStatusLabels(null, "采码中", null, null);
            AppendLog(mode + "：开始扫码。");
        }

        private void StopScanCycle(string reason)
        {
            if (!_scanActive)
            {
                return;
            }

            _scanTimer.Stop();
            _scanActive = false;
            if (!IsProductionMode)
            {
                btnStopCurrentScan.Enabled = false;
            }
            if (_camera != null)
            {
                try
                {
                    _camera.Stop();
                }
                catch (Exception ex)
                {
                    AppendLog("停止采图失败：" + ex.Message);
                }
            }
            UpdateStatusLabels(null, "已停止", null, null);
            AppendLog("停止当前扫码：" + reason + "。");
        }

        private void Camera_BarcodeCaptured(BarcodeCapture capture)
        {
            if (capture == null || string.IsNullOrWhiteSpace(capture.Barcode))
            {
                return;
            }

            if (InvokeRequired)
            {
                BeginInvoke(new Action<BarcodeCapture>(Camera_BarcodeCaptured), capture);
                return;
            }

            if (!_scanActive)
            {
                return;
            }

            HandleEffectiveCapture(capture);
        }

        private void HandleEffectiveCapture(BarcodeCapture capture)
        {
            string barcode = ProductCatalog.NormalizeBarcode(capture.Barcode);
            if (string.IsNullOrWhiteSpace(barcode))
            {
                return;
            }

            DateTime readTime = DateTime.Now;
            ProductRecord product = FindProduct(barcode);
            OrderMatchItem matchedOrderItem = _orderMatchItems.FirstOrDefault(i => string.Equals(i.Barcode, barcode, StringComparison.OrdinalIgnoreCase));
            string sku = ResolveSku(barcode, product);
            string length = GetProductLength(product);
            string width = GetProductWidth(product);
            string height = GetProductHeight(product);
            if (matchedOrderItem != null)
            {
                length = FirstNonEmpty(length, matchedOrderItem.Length);
                width = FirstNonEmpty(width, matchedOrderItem.Width);
                height = FirstNonEmpty(height, matchedOrderItem.Height);
            }
            string imagePath = SaveCaptureImage(capture, barcode, readTime);

            StopScanCycle("有效结果");

            ScanRecord record;
            if (IsProductionMode)
            {
                record = new ScanRecord
                {
                    Mode = "工作模式",
                    OrderNo = CurrentOrderNo,
                    BatchBoxCode = CurrentBoxCode,
                    Sequence = _nextSequence++,
                    Barcode = barcode,
                    Sku = sku,
                    Length = length,
                    Width = width,
                    Height = height,
                    Status = ResolveProductionStatus(barcode, sku),
                    ScanCount = CountRecords(_currentProductionRecords, barcode) + 1,
                    ScanTime = readTime,
                    ImagePath = imagePath
                };
                _currentProductionRecords.Add(record);
                _historyStore.Append(record);
                AddRecordToActiveBatch(record);
                WriteCurrentStatistics();
                RefreshActiveScanTables();
                if (IsProductionOrderComplete())
                {
                    TryEndProductionBatch("订单数量已满足");
                    UpdateStatusLabels(null, "批次已完成", "等待机械臂", "等待检测");
                }
                else
                {
                    UpdateStatusLabels(null, "待下一信号", "等待机械臂", null);
                }
            }
            else
            {
                if (IsNoOrderTestMode)
                {
                    record = new ScanRecord
                    {
                        Mode = "测试模式",
                        OrderNo = string.Empty,
                        BatchBoxCode = string.Empty,
                        Sequence = _testSequence++,
                        Barcode = barcode,
                        Sku = sku,
                        Length = length,
                        Width = width,
                        Height = height,
                        Status = FindProduct(barcode) == null ? "未查询到该产品" : "测试记录",
                        ScanCount = CountRecords(_testRecords, barcode) + 1,
                        ScanTime = readTime,
                        ImagePath = imagePath
                    };
                }
                else
                {
                    record = new ScanRecord
                    {
                        Mode = "测试模式",
                        OrderNo = CurrentOrderNo,
                        BatchBoxCode = CurrentBoxCode,
                        Sequence = _testSequence++,
                        Barcode = barcode,
                        Sku = sku,
                        Length = length,
                        Width = width,
                        Height = height,
                        Status = ResolveOrderScanStatus(barcode, sku, _testRecords),
                        ScanCount = CountRecords(_testRecords, barcode) + 1,
                        ScanTime = readTime,
                        ImagePath = imagePath
                    };
                }

                _testRecords.Add(record);
                _historyStore.Append(record);
                AddRecordToActiveBatch(record);
                WriteCurrentStatistics();
                RefreshActiveScanTables();
                if (!IsNoOrderTestMode && IsOrderComplete(_testRecords))
                {
                    EndBatchInternal(false);
                    UpdateStatusLabels(null, "批次已完成", "等待开始批次", "等待检测");
                }
                else
                {
                    UpdateStatusLabels(null, "待下一次扫码", null, null);
                }
            }

            ShowRecord(record, IsProductionMode ? "当前采码" : "测试记录");
            RefreshSummaryPanels();
            ScannedItemAdded?.Invoke(BuildScannedItemInfo(product, sku, length, width, height, barcode));
            AppendLog("有效采码：" + barcode + "，已保存照片并停止扫码，数量+1。");
        }

        private OrderBoxIdentity ResolveOrderBoxIdentity(string input, bool inputIsOrderNo)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            OrderBoxIdentity parsed = ParseOrderBoxPayload(input);
            if (parsed != null && parsed.OrderNo.Length > 0 && parsed.BoxCode.Length > 0)
            {
                return parsed;
            }

            List<ScanRecord> history = _historyStore.LoadAll();
            ScanRecord found = inputIsOrderNo
                ? history.FirstOrDefault(r => string.Equals(r.OrderNo, input, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(r.BatchBoxCode))
                : history.FirstOrDefault(r => string.Equals(r.BatchBoxCode, input, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(r.OrderNo));

            if (found != null)
            {
                return new OrderBoxIdentity(found.OrderNo, found.BatchBoxCode);
            }

            return inputIsOrderNo
                ? new OrderBoxIdentity(input, parsed == null ? string.Empty : parsed.BoxCode)
                : new OrderBoxIdentity(parsed == null ? string.Empty : parsed.OrderNo, input);
        }

        private static OrderBoxIdentity ParseOrderBoxPayload(string input)
        {
            string order = string.Empty;
            string box = string.Empty;
            string[] parts = input.Split(new[] { '|', ';', ',', '&', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                int index = part.IndexOf('=');
                if (index < 0)
                {
                    index = part.IndexOf(':');
                }
                if (index <= 0)
                {
                    continue;
                }

                string key = part.Substring(0, index).Trim().ToLowerInvariant();
                string value = part.Substring(index + 1).Trim();
                if (key == "order" || key == "orderno" || key == "orderid" || key == "订单编号" || key == "订单号")
                {
                    order = value;
                }
                else if (key == "box" || key == "boxcode" || key == "carton" || key == "箱码编号" || key == "箱码号" || key == "箱码")
                {
                    box = value;
                }
            }

            return order.Length == 0 && box.Length == 0 ? null : new OrderBoxIdentity(order, box);
        }

        private ProductRecord FindProduct(string barcode)
        {
            return _productCatalog == null ? null : _productCatalog.Find(barcode);
        }

        private ProductRecord ResolveProduct(string barcode, string sku)
        {
            ProductRecord product = FindProduct(barcode);
            if (product == null && _productCatalog != null && !string.IsNullOrWhiteSpace(sku))
            {
                product = _productCatalog.FindBySku(sku);
            }

            return product;
        }

        private string ResolveSku(string barcode, ProductRecord product)
        {
            if (product != null)
            {
                return product.GetValue("SKU", "Sku", "sku", "货号");
            }

            OrderMatchItem item = _orderMatchItems.FirstOrDefault(i => string.Equals(i.Barcode, barcode, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                return item.Sku;
            }

            if (barcode.StartsWith("BC-", StringComparison.OrdinalIgnoreCase))
            {
                return "P-" + barcode.Substring(3);
            }

            return string.Empty;
        }

        private string ResolveOrderScanStatus(string barcode, string sku, List<ScanRecord> records)
        {
            OrderMatchItem item = _orderMatchItems.FirstOrDefault(i => string.Equals(i.Barcode, barcode, StringComparison.OrdinalIgnoreCase));
            if (_orderMatchItems.Count == 0)
            {
                return FindProduct(barcode) == null ? "未查询到该产品" : "未设置订单数量";
            }
            if (item == null)
            {
                return "未匹配";
            }
            if (FindProduct(barcode) == null &&
                string.IsNullOrWhiteSpace(item.Length) &&
                string.IsNullOrWhiteSpace(item.Width) &&
                string.IsNullOrWhiteSpace(item.Height))
            {
                return "未查询到该产品";
            }
            if (!string.IsNullOrWhiteSpace(item.Sku) && !string.Equals(item.Sku, sku, StringComparison.OrdinalIgnoreCase))
            {
                return "货号异常";
            }

            int actualAfterThisScan = CountRecords(records, barcode) + 1;
            if (actualAfterThisScan > item.OrderQuantity)
            {
                return "数量超出";
            }
            return "成功";
        }

        private string ResolveProductionStatus(string barcode, string sku)
        {
            return ResolveOrderScanStatus(barcode, sku, _currentProductionRecords);
        }

        private static string GetProductLength(ProductRecord product)
        {
            return product == null ? string.Empty : product.GetValue("Length_mm", "Length", "L", "长", "长度");
        }

        private static string GetProductWidth(ProductRecord product)
        {
            return product == null ? string.Empty : product.GetValue("Width_mm", "Width", "W", "宽", "宽度");
        }

        private static string GetProductHeight(ProductRecord product)
        {
            return product == null ? string.Empty : product.GetValue("Height_mm", "Height", "H", "高", "高度");
        }

        private static string ResolveProductName(ProductRecord product, string sku)
        {
            if (product != null)
            {
                string name = product.GetValue("名称", "Name", "产品名", "品名");
                if (!string.IsNullOrWhiteSpace(name))
                    return name.Trim();
            }

            return string.IsNullOrWhiteSpace(sku) ? "-" : sku.Trim();
        }

        private static string FormatDimensions(string length, string width, string height)
        {
            if (string.IsNullOrWhiteSpace(length) && string.IsNullOrWhiteSpace(width) && string.IsNullOrWhiteSpace(height))
                return "-";

            string l = string.IsNullOrWhiteSpace(length) ? "-" : length.Trim();
            string w = string.IsNullOrWhiteSpace(width) ? "-" : width.Trim();
            string h = string.IsNullOrWhiteSpace(height) ? "-" : height.Trim();
            return l + "×" + w + "×" + h;
        }

        private static ScannedItemInfo BuildScannedItemInfo(
            ProductRecord product, string sku, string length, string width, string height, string barcode)
        {
            var item = new ScannedItemInfo
            {
                Name = ResolveProductName(product, sku),
                Dimensions = FormatDimensions(length, width, height),
                Barcode = barcode ?? string.Empty
            };
            AssignPackedDimensions(item, length, width, height);
            return item;
        }

        private ScannedItemInfo BuildScannedItemInfoFromRecord(ScanRecord record)
        {
            if (record == null)
                return null;

            ProductRecord product = FindProduct(record.Barcode);
            string length = FirstNonEmpty(record.Length, GetProductLength(product));
            string width = FirstNonEmpty(record.Width, GetProductWidth(product));
            string height = FirstNonEmpty(record.Height, GetProductHeight(product));
            return BuildScannedItemInfo(product, record.Sku, length, width, height, record.Barcode);
        }

        private void RaiseBatchCompleted()
        {
            if (_batchRecords.Count == 0)
                return;

            var items = new List<ScannedItemInfo>(_batchRecords.Count);
            foreach (ScanRecord record in _batchRecords)
            {
                ScannedItemInfo item = BuildScannedItemInfoFromRecord(record);
                if (item != null)
                    items.Add(item);
            }

            if (items.Count > 0)
                BatchCompleted?.Invoke(items);
        }

        private static string FirstNonEmpty(string primary, string fallback)
        {
            return string.IsNullOrWhiteSpace(primary) ? (fallback ?? string.Empty) : primary.Trim();
        }

        private static bool TryParseDimensionMm(string text, out int mm)
        {
            mm = 0;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            text = text.Trim();
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) &&
                !double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
                return false;

            int rounded = (int)Math.Round(value);
            if (rounded <= 0)
                return false;

            // ProductInfo / 扫码尺寸为毫米，与装箱算法内部单位一致（容器表格同为 mm）
            mm = rounded;
            return true;
        }

        private static void AssignPackedDimensions(ScannedItemInfo item, string length, string width, string height)
        {
            if (item == null)
                return;

            TryParseDimensionMm(length, out int dx);
            TryParseDimensionMm(width, out int dy);
            TryParseDimensionMm(height, out int dz);
            item.Dx = dx;
            item.Dy = dy;
            item.Dz = dz;
        }

        private void AddRecordToActiveBatch(ScanRecord record)
        {
            if (!_batchActive || record == null)
            {
                return;
            }

            _batchRecords.Add(record);
        }

        private void WriteCurrentStatistics()
        {
            try
            {
                string fileStem = _batchActive && !string.IsNullOrWhiteSpace(_batchFolderName)
                    ? _batchFolderName
                    : BuildProductionBatchName(CurrentOrderNo, CurrentBoxCode, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                string path = _statisticsWriter.WriteBatch(fileStem, CurrentOrderNo, CurrentBoxCode, _currentProductionRecords, _productCatalog);
                AppendLog("已生成统计信息文件：" + path);
            }
            catch (Exception ex)
            {
                AppendLog("生成统计信息文件失败：" + ex.Message);
            }
        }

        private void WriteBatchStatistics()
        {
            if (_batchRecords.Count == 0)
            {
                AppendLog("当前批次没有采码记录，未生成统计文件。");
                return;
            }

            if (string.IsNullOrWhiteSpace(_batchFolderName))
            {
                AppendLog("批次名称无效，未生成统计文件。");
                return;
            }

            try
            {
                string orderNo = IsNoOrderTestMode
                    ? string.Empty
                    : (HasActiveOrderMatch() || _currentOrderConfirmed ? CurrentOrderNo : string.Empty);
                string boxCode = IsNoOrderTestMode
                    ? string.Empty
                    : (HasActiveOrderMatch() || _currentOrderConfirmed ? CurrentBoxCode : string.Empty);
                string path = _statisticsWriter.WriteBatch(_batchFolderName, orderNo, boxCode, _batchRecords, _productCatalog);
                AppendLog("已生成批次统计信息文件：" + path);
            }
            catch (Exception ex)
            {
                AppendLog("生成批次统计信息文件失败：" + ex.Message);
            }
        }

        private void RefreshCurrentTables()
        {
            RefreshActiveScanTables();
        }

        private void RefreshTestTables()
        {
            RefreshActiveScanTables();
        }

        private List<ScanRecord> GetActiveScanRecords()
        {
            return IsProductionMode ? _currentProductionRecords : _testRecords;
        }

        private bool HasActiveOrderMatch()
        {
            return !IsNoOrderTestMode && _orderInfoMatched && _orderMatchItems.Count > 0;
        }

        private void RefreshActiveScanTables()
        {
            BindScanRecordGrid(dgvProductScanList, GetActiveScanRecords());
            RefreshVisibleMatchGrid();
        }

        private void RefreshVisibleMatchGrid()
        {
            List<ScanRecord> activeRecords = GetActiveScanRecords();
            if (HasActiveOrderMatch())
            {
                BindMatchGrid(dgvScanMatchResult, BuildMatchSummary(activeRecords, _orderMatchItems, false));
                return;
            }

            BindMatchGrid(dgvScanMatchResult, BuildMatchSummary(activeRecords, new List<OrderMatchItem>(), !IsProductionMode));
        }

        private void RefreshSummaryPanels()
        {
            List<ScanRecord> activeRecords = GetActiveScanRecords();
            List<MatchSummaryRow> summary = HasActiveOrderMatch()
                ? BuildMatchSummary(activeRecords, _orderMatchItems, false)
                : BuildMatchSummary(activeRecords, new List<OrderMatchItem>(), !IsProductionMode);

            int actualTotal = summary.Sum(r => r.ActualQuantity);
            int abnormal = activeRecords.Count(r => IsAbnormalScanStatus(r.Status));
            lblActualTotalValue.Text = actualTotal.ToString();
            lblAbnormalTotalValue.Text = abnormal.ToString();
            lblTestTotalValue.Text = _testRecords.Count.ToString();
            lblCurrentStateValue.Text = _scanActive ? "采码中" : (IsProductionMode ? "工作模式" : "测试模式");
            lblFooterStatus.Text = "订单编号: " + ValueOrDash(CurrentOrderNo) +
                " | 箱码编号: " + ValueOrDash(CurrentBoxCode) +
                " | 实扫: " + actualTotal +
                " | 异常: " + abnormal +
                " | 当前: " + (IsProductionMode ? "工作模式" : "测试模式");
        }

        private static string ValueOrDash(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value.Trim();
        }

        private void BindScanRecordGrid(DataGridView grid, List<ScanRecord> records)
        {
            grid.Rows.Clear();
            foreach (ScanRecord record in records)
            {
                int index = grid.Rows.Add(record.OrderNo, record.Sequence, record.Barcode, record.Sku, record.Status, record.ScanCount);
                grid.Rows[index].Tag = record;
                ApplyStatusColor(grid.Rows[index], record.Status);
            }
        }

        private void BindMatchGrid(DataGridView grid, List<MatchSummaryRow> rows)
        {
            grid.Rows.Clear();
            foreach (MatchSummaryRow row in rows)
            {
                int index = grid.Rows.Add(row.Barcode, row.Sku, row.OrderQuantity.HasValue ? row.OrderQuantity.Value.ToString() : string.Empty, row.ActualQuantity, row.Status);
                ApplyStatusColor(grid.Rows[index], row.Status);
            }
        }

        private static void ApplyStatusColor(DataGridViewRow row, string status)
        {
            if (status == null)
            {
                return;
            }

            if (status.IndexOf("正常", StringComparison.OrdinalIgnoreCase) >= 0 || status.IndexOf("成功", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(232, 246, 234);
            }
            else if (status.IndexOf("测试", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(238, 244, 255);
            }
            else if (status.IndexOf("不足", StringComparison.OrdinalIgnoreCase) >= 0 ||
                status.IndexOf("超出", StringComparison.OrdinalIgnoreCase) >= 0 ||
                status.IndexOf("异常", StringComparison.OrdinalIgnoreCase) >= 0 ||
                status.IndexOf("未匹配", StringComparison.OrdinalIgnoreCase) >= 0 ||
                status.IndexOf("未查询到", StringComparison.OrdinalIgnoreCase) >= 0 ||
                status.IndexOf("未设置", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(255, 228, 228);
            }
            else
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(255, 245, 224);
            }
        }

        private static List<MatchSummaryRow> BuildMatchSummary(List<ScanRecord> records, List<OrderMatchItem> expectedItems, bool testMode)
        {
            var result = new List<MatchSummaryRow>();
            var actualGroups = records
                .GroupBy(r => r.Barcode ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            if (testMode)
            {
                foreach (var pair in actualGroups.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
                {
                    ScanRecord first = pair.Value.First();
                    result.Add(new MatchSummaryRow
                    {
                        Barcode = pair.Key,
                        Sku = first.Sku,
                        OrderQuantity = null,
                        ActualQuantity = pair.Value.Count,
                        Status = ResolveGroupStatus(pair.Value, "测试模式")
                    });
                }
                return result;
            }

            foreach (OrderMatchItem expected in expectedItems)
            {
                List<ScanRecord> actual;
                actualGroups.TryGetValue(expected.Barcode, out actual);
                int actualCount = actual == null ? 0 : actual.Count;
                string status = "正常";
                if (actual != null && actual.Any(r => string.Equals(r.Status, "未查询到该产品", StringComparison.OrdinalIgnoreCase)))
                {
                    status = "未查询到该产品";
                }
                else if (actualCount < expected.OrderQuantity)
                {
                    status = "数量不足";
                }
                else if (actualCount > expected.OrderQuantity)
                {
                    status = "数量超出";
                }

                result.Add(new MatchSummaryRow
                {
                    Barcode = expected.Barcode,
                    Sku = expected.Sku,
                    OrderQuantity = expected.OrderQuantity,
                    ActualQuantity = actualCount,
                    Status = status
                });
            }

            foreach (var pair in actualGroups)
            {
                if (expectedItems.Any(i => string.Equals(i.Barcode, pair.Key, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                ScanRecord first = pair.Value.First();
                result.Add(new MatchSummaryRow
                {
                    Barcode = pair.Key,
                    Sku = first.Sku,
                    OrderQuantity = null,
                    ActualQuantity = pair.Value.Count,
                    Status = expectedItems.Count == 0 ? "未设置订单数量" : "未匹配"
                });
            }

            return result.OrderBy(r => r.Barcode, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private string SaveCaptureImage(BarcodeCapture capture, string barcode, DateTime readTime)
        {
            if (capture.ImageBytes == null || capture.ImageBytes.Length == 0)
            {
                AppendLog("相机回调未返回可保存的采图数据，照片路径留空。");
                return string.Empty;
            }

            string imageDirectory = _batchActive && !string.IsNullOrWhiteSpace(_batchImageDirectory)
                ? _batchImageDirectory
                : GetImageRootDirectory();
            Directory.CreateDirectory(imageDirectory);
            string extension = string.IsNullOrWhiteSpace(capture.ImageExtension) ? ".jpg" : capture.ImageExtension;
            if (!extension.StartsWith(".", StringComparison.Ordinal))
            {
                extension = "." + extension;
            }
            string fileName = readTime.ToString("yyyyMMdd_HHmmss_fff") + "_" + SanitizeFileName(barcode) + extension;
            string path = Path.Combine(imageDirectory, fileName);
            File.WriteAllBytes(path, capture.ImageBytes);
            return path;
        }

        private static string SanitizeFileName(string value)
        {
            return SanitizePathSegment(value);
        }

        private void ShowRecord(ScanRecord record, string source)
        {
            lblResultOrderNoValue.Text = record.OrderNo;
            lblResultBatchBoxValue.Text = record.BatchBoxCode;
            lblResultSequenceValue.Text = record.Sequence.ToString();
            lblResultBarcodeValue.Text = record.Barcode;
            lblResultSkuValue.Text = record.Sku;
            lblResultStatusValue.Text = record.Status;
            lblResultScanCountValue.Text = record.ScanCount.ToString();
            lblResultScanTimeValue.Text = record.ScanTime == DateTime.MinValue ? string.Empty : record.ScanTime.ToString("yyyy-MM-dd HH:mm:ss");

            lblPhotoBarcodeValue.Text = record.Barcode;
            lblPhotoSkuValue.Text = record.Sku;
            lblPhotoSourceValue.Text = source;

            LoadImage(record.ImagePath);
        }

        private void LoadImage(string path)
        {
            Image old = picScanImage.Image;
            picScanImage.Image = null;
            if (old != null)
            {
                old.Dispose();
            }

            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return;
            }

            using (var source = Image.FromFile(path))
            {
                picScanImage.Image = new Bitmap(source);
            }
        }

        private ScanRecord GetSelectedRecord(DataGridView grid, List<ScanRecord> fallback)
        {
            if (grid.SelectedRows.Count == 0)
            {
                return null;
            }
            DataGridViewRow row = grid.SelectedRows[0];
            ScanRecord tagged = row.Tag as ScanRecord;
            if (tagged != null)
            {
                return tagged;
            }

            if (fallback == null)
            {
                return null;
            }
            int index = row.Index;
            return index >= 0 && index < fallback.Count ? fallback[index] : null;
        }

        private void UpdateModeState()
        {
            bool production = IsProductionMode;
            bool test = !production;

            rdoInputByOrderNo.Enabled = !IsNoOrderTestMode && !IsOrderDirectReadMode;
            UpdateOrderActionState();

            if (_chkNoOrderTestMode != null)
            {
                _chkNoOrderTestMode.Visible = test;
                if (production && _chkNoOrderTestMode.Checked)
                {
                    _chkNoOrderTestMode.Checked = false;
                }
            }

            if (_chkOrderDirectReadMode != null)
            {
                _chkOrderDirectReadMode.Visible = test;
                if (production && _chkOrderDirectReadMode.Checked)
                {
                    _chkOrderDirectReadMode.Checked = false;
                }
            }

            bool showScanBatch = test && !IsNoOrderTestMode && !IsOrderDirectReadMode;
            btnStartTestScan.Visible = showScanBatch;
            btnStartTestScan.Enabled = showScanBatch;
            btnStopCurrentScan.Visible = showScanBatch;
            if (_btnBatchStart != null)
            {
                _btnBatchStart.Visible = showScanBatch;
            }
            if (_btnBatchEnd != null)
            {
                _btnBatchEnd.Visible = showScanBatch;
            }

            btnWaitRobotSignal.Visible = production;
            btnWaitRobotSignal.Enabled = production && _currentOrderConfirmed;

            grpDeviceActions.Text = production ? "扫码器与信号" : "扫码器与测试";
            grpCurrentOrder.Visible = true;
            grpWorkState.Visible = production;
            SetModeTabs(production);
            lblModeHint.Text = production
                ? "工作模式：匹配并确认订单/箱码后，由机械臂与3D相机信号驱动批次与采码。"
                : "测试模式：可匹配订单扫码、无订单测试，或勾选订单直读后输入箱码直接载入装箱。";
            lblTestTotalTitle.Visible = test;
            lblTestTotalValue.Visible = test;

            UpdateTestBatchButtonState();
            ArrangeDeviceActionButtons();
            ArrangeOrderActionButtons();
            RefreshVisibleMatchGrid();
            RefreshSummaryPanels();
        }

        private void SetModeTabs(bool production)
        {
            tabMain.SuspendLayout();
            tabMain.TabPages.Clear();
            tabMain.TabPages.Add(tabCurrentOrder);
            tabMain.TabPages.Add(tabHistory);
            tabMain.TabPages.Add(tabLog);
            tabMain.ResumeLayout();
        }

        private void UpdateStatusLabels(string reader, string scan, string robot, string camera3d)
        {
            if (reader != null)
            {
                lblReaderStateValue.Text = reader;
            }
            if (scan != null)
            {
                lblScanStateValue.Text = scan;
            }
            if (robot != null)
            {
                lblRobotStateValue.Text = robot;
            }
            if (camera3d != null)
            {
                lblCamera3DStateValue.Text = camera3d;
            }
        }

        private void AppendLog(string message)
        {
            txtLog.AppendText(DateTime.Now.ToString("HH:mm:ss") + "  " + message + Environment.NewLine);
        }

        private static int CountRecords(List<ScanRecord> records, string barcode)
        {
            return records.Count(r => string.Equals(r.Barcode, barcode, StringComparison.OrdinalIgnoreCase));
        }

        private static bool IsAbnormalScanStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return false;
            }

            return status.IndexOf("未查询到", StringComparison.OrdinalIgnoreCase) >= 0 ||
                status.IndexOf("异常", StringComparison.OrdinalIgnoreCase) >= 0 ||
                status.IndexOf("未匹配", StringComparison.OrdinalIgnoreCase) >= 0 ||
                status.IndexOf("超出", StringComparison.OrdinalIgnoreCase) >= 0 ||
                status.IndexOf("不足", StringComparison.OrdinalIgnoreCase) >= 0 ||
                status.IndexOf("未设置", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string ResolveGroupStatus(IEnumerable<ScanRecord> records, string normalStatus)
        {
            foreach (ScanRecord record in records)
            {
                if (IsAbnormalScanStatus(record.Status))
                {
                    return record.Status;
                }
            }

            return normalStatus;
        }

        private void WarnIfBatchHasIssues(string batchName)
        {
            if (_batchRecords.Count == 0)
            {
                return;
            }

            var issueLines = new List<string>();
            var reportedBarcodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (IGrouping<string, ScanRecord> group in _batchRecords
                .Where(r => IsAbnormalScanStatus(r.Status))
                .GroupBy(r => r.Barcode ?? string.Empty, StringComparer.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(group.Key))
                {
                    continue;
                }

                ScanRecord first = group.First();
                issueLines.Add(string.Format("条码 {0}：{1}（{2} 次）", group.Key, first.Status, group.Count()));
                reportedBarcodes.Add(group.Key);
            }

            if (HasActiveOrderMatch())
            {
                foreach (MatchSummaryRow row in BuildMatchSummary(_batchRecords, _orderMatchItems, false))
                {
                    if (row.Status == "正常" || reportedBarcodes.Contains(row.Barcode))
                    {
                        continue;
                    }

                    string expected = row.OrderQuantity.HasValue ? row.OrderQuantity.Value.ToString() : "-";
                    issueLines.Add(string.Format("条码 {0}：{1}（实扫 {2}，应扫 {3}）", row.Barcode, row.Status, row.ActualQuantity, expected));
                    reportedBarcodes.Add(row.Barcode);
                }
            }

            if (issueLines.Count == 0)
            {
                return;
            }

            string header = IsProductionMode
                ? string.Format("订单 {0} 箱码 {1} 批次「{2}」扫码存在以下问题：", CurrentOrderNo, CurrentBoxCode, batchName)
                : string.Format("测试订单 {0} 箱码 {1} 批次「{2}」扫码存在以下问题：", CurrentOrderNo, CurrentBoxCode, batchName);
            AppendLog("批次结束警告：" + string.Join("；", issueLines));
            MessageBox.Show(DialogOwner, header + Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, issueLines),
                "批次扫码警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private static ScannerSettingsData CloneSettings(ScannerSettingsData settings)
        {
            return new ScannerSettingsData
            {
                ReaderIp = settings.ReaderIp,
                ReaderPort = settings.ReaderPort,
                DeviceIndex = settings.DeviceIndex,
                ScanIntervalMs = settings.ScanIntervalMs,
                AutoFocus = settings.AutoFocus,
                AutoReconnect = settings.AutoReconnect,
                SaveRawImage = settings.SaveRawImage,
                ImageSavePath = settings.ImageSavePath,
                LightMode = settings.LightMode,
                ExposureTimeUs = settings.ExposureTimeUs,
                GainDb = settings.GainDb,
                UseAutoPacketSize = settings.UseAutoPacketSize,
                GevSCPSPacketSize = settings.GevSCPSPacketSize,
                GevHeartbeatTimeoutMs = settings.GevHeartbeatTimeoutMs,
                JpegQuality = settings.JpegQuality,
                AutoFocusCommand = settings.AutoFocusCommand
            };
        }

        private sealed class OrderBoxIdentity
        {
            public string OrderNo { get; private set; }
            public string BoxCode { get; private set; }

            public OrderBoxIdentity(string orderNo, string boxCode)
            {
                OrderNo = orderNo ?? string.Empty;
                BoxCode = boxCode ?? string.Empty;
            }
        }
    }
}
