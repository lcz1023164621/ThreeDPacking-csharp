using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        private readonly Timer _signalSendRetryTimer = new Timer();
        private int _signalSendRetryValue = -1;
        private int _signalSendRetrySentCount;
        private bool _signalSendRetryUntilStopped;
        private readonly List<ScanRecord> _currentProductionRecords = new List<ScanRecord>();
        private readonly List<ScanRecord> _testRecords = new List<ScanRecord>();
        private readonly List<OrderMatchItem> _orderMatchItems = new List<OrderMatchItem>();
        private readonly Dictionary<string, Queue<ScannedItemInfo>> _packingInfoBySku =
            new Dictionary<string, Queue<ScannedItemInfo>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Queue<ScannedItemInfo>> _packingInfoByBarcode =
            new Dictionary<string, Queue<ScannedItemInfo>>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _consumedPackingBoxIds =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly List<OrderIndexEntry> _orderIndex = new List<OrderIndexEntry>();
        private readonly HistoryStore _historyStore;
        private readonly BatchStatisticsWriter _statisticsWriter;
        private IBarcodeCamera _camera;
        private ProductionSignalClient _productionSignalClient;
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
        private Button _btnDeleteOrderIndex;
        private Button _btnClearOrderList;
        private DataGridView _dgvOrderIndex;
        private ContextMenuStrip _importOrderMenu;
        private CheckBox _chkNoOrderTestMode;
        private CheckBox _chkOrderDirectReadMode;
        private Button _btnDirectReadForPacking;
        private bool _scanActive;
        private int _nextSequence = 1;
        private int _testSequence = 1;
        private SplitContainer _splitMain;
        private Panel _pnlReaderStateIndicator;
        private Panel _pnlScanStateIndicator;
        private Panel _pnlRobotStateIndicator;
        private Panel _pnlCamera3DStateIndicator;
        private Button _btnRefreshWorkState;
        private Button _btnScanReset;
        private TabPage _tabSignalLog;
        private TextBox _txtSignalLog;
        private bool _productionAwaitingCapture;
        private bool _productionFailureNotified;
        private bool _productionScanFailedWaitingRetry;
        private bool _robotPackingInProgress;
        private bool _productionBatchEnded;
        private ScanRecord _pendingProductionRecord;

        public event Action<ScannedItemInfo> ScannedItemAdded;
        public event Action ScanItemsCleared;
        public event Action<IReadOnlyList<ScannedItemInfo>> BatchCompleted;
        public event Action<IReadOnlyList<ScannedItemInfo>> OrderMatchedForPacking;
        public event Action OrderMatchCleared;
        public event Action<IReadOnlyList<ScannedItemInfo>> OrderConfirmedForPacking;

        public ScanTestControl()
        {
            InitializeComponent();
            ApplyCompactLayout();
            InitBatchButtons();
            InitOrderImportButton();
            InitOrderIndexGrid();
            InitNoOrderTestModeControl();
            InitOrderDirectReadModeControl();
            InitWorkStateRefreshButton();
            InitScanResetButton();
            InitSignalLogTab();
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
            _signalSendRetryTimer.Tick += signalSendRetryTimer_Tick;
            _signalSendRetryTimer.Interval = _scannerSettings.SignalSendRetryIntervalMs;

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
            if (_btnScanReset != null)
            {
                StyleActionButton(_btnScanReset, Color.FromArgb(180, 60, 60), Color.White);
            }
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

            InitWorkStateIndicators();
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

        private enum WorkStateVisual
        {
            Neutral,
            Active,
            Success,
            Warning,
            Error
        }

        private void InitWorkStateIndicators()
        {
            _pnlReaderStateIndicator = CreateWorkStateIndicatorPanel();
            _pnlScanStateIndicator = CreateWorkStateIndicatorPanel();
            _pnlRobotStateIndicator = CreateWorkStateIndicatorPanel();
            _pnlCamera3DStateIndicator = CreateWorkStateIndicatorPanel();

            grpWorkState.Controls.Add(_pnlReaderStateIndicator);
            grpWorkState.Controls.Add(_pnlScanStateIndicator);
            grpWorkState.Controls.Add(_pnlRobotStateIndicator);
            grpWorkState.Controls.Add(_pnlCamera3DStateIndicator);

            PrepareWorkStateTextLabel(lblReaderStateValue);
            PrepareWorkStateTextLabel(lblScanStateValue);
            PrepareWorkStateTextLabel(lblRobotStateValue);
            PrepareWorkStateTextLabel(lblCamera3DStateValue);

            lblCamera3DStateTitle.Visible = false;
            lblCamera3DStateValue.Visible = false;
            _pnlCamera3DStateIndicator.Visible = false;

            grpWorkState.Resize += (s, e) => LayoutWorkStateIndicators();
            LayoutWorkStateIndicators();
        }

        private static void PrepareWorkStateTextLabel(Label label)
        {
            if (label == null)
            {
                return;
            }

            label.AutoSize = true;
            label.BackColor = Color.Transparent;
            label.BorderStyle = BorderStyle.None;
            label.Padding = new Padding(0);
        }

        private Panel CreateWorkStateIndicatorPanel()
        {
            var panel = new Panel
            {
                Size = new Size(16, 16),
                BackColor = Color.Transparent,
                Tag = WorkStateVisual.Neutral
            };
            panel.Paint += WorkStateIndicator_Paint;
            return panel;
        }

        private void LayoutWorkStateIndicators()
        {
            if (_pnlReaderStateIndicator == null)
            {
                return;
            }

            const int dotX = 58;
            const int textX = 78;
            LayoutWorkStateRow(lblReaderStateTitle, _pnlReaderStateIndicator, lblReaderStateValue, 19, dotX, textX);
            LayoutWorkStateRow(lblScanStateTitle, _pnlScanStateIndicator, lblScanStateValue, 40, dotX, textX);
            LayoutWorkStateRow(lblRobotStateTitle, _pnlRobotStateIndicator, lblRobotStateValue, 61, dotX, textX);
            if (_btnRefreshWorkState != null)
            {
                _btnRefreshWorkState.Location = new Point(10, 82);
                _btnRefreshWorkState.Size = new Size(Math.Max(grpWorkState.ClientSize.Width - 20, 120), 24);
            }
        }

        private void InitWorkStateRefreshButton()
        {
            _btnRefreshWorkState = new Button
            {
                Name = "btnRefreshWorkState",
                Text = "刷新状态",
                UseVisualStyleBackColor = true
            };
            _btnRefreshWorkState.Click += btnRefreshWorkState_Click;
            StyleActionButton(_btnRefreshWorkState, Color.FromArgb(70, 85, 105), Color.White);
            grpWorkState.Controls.Add(_btnRefreshWorkState);
            grpWorkState.Resize += (s, e) => LayoutWorkStateIndicators();
            LayoutWorkStateIndicators();
        }

        private void InitScanResetButton()
        {
            _btnScanReset = new Button
            {
                Name = "btnScanReset",
                Text = "扫码归位",
                UseVisualStyleBackColor = true,
                Visible = false,
                Enabled = false
            };
            _btnScanReset.Click += btnScanReset_Click;
            grpDeviceActions.Controls.Add(_btnScanReset);
        }

        private void InitSignalLogTab()
        {
            _tabSignalLog = new TabPage
            {
                Name = "tabSignalLog",
                Text = "数据传输日志",
                Padding = new Padding(6)
            };
            _txtSignalLog = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                WordWrap = false,
                Font = new Font("Consolas", 9F)
            };
            _tabSignalLog.Controls.Add(_txtSignalLog);
        }

        private void btnRefreshWorkState_Click(object sender, EventArgs e)
        {
            RefreshWorkState();
        }

        private void RefreshWorkState()
        {
            if (IsProductionMode)
            {
                SyncProductionWorkStateDisplay();
            }
            else
            {
                string reader = _readerConnected ? "已连接" : "未连接";
                string scan = _scanActive ? "正在采码" : "未开始";
                string robot = "未到位";
                UpdateStatusLabels(reader, scan, robot);
            }

            string readerText = lblReaderStateValue.Text;
            string scanText = lblScanStateValue.Text;
            string robotText = lblRobotStateValue.Text;
            AppendLog("已刷新工作状态：扫码器=" + readerText + "，采码=" + scanText + "，机械臂=" + robotText + "。");
        }

        private void SetScanWorkStatus(string status)
        {
            UpdateStatusLabels(null, status, null);
        }

        private void SetRobotArmWorkStatus(string status)
        {
            UpdateStatusLabels(null, null, status);
        }

        private void SyncProductionWorkStateDisplay()
        {
            if (!IsProductionMode)
            {
                return;
            }

            string reader = _readerConnected ? "已连接" : "未连接";
            if (_productionSignalClient != null)
            {
                reader += " · 发" + (_productionSignalClient.IsSendConnected ? "已连" : "断开")
                    + "/收" + (_productionSignalClient.IsReceiveConnected ? "已连" : "断开");
            }

            string scan;
            if (_productionBatchEnded)
            {
                scan = "批次已结束";
            }
            else if (_scanActive)
            {
                scan = _productionScanFailedWaitingRetry ? "持续采码" : "正在采码";
            }
            else if (_batchActive && _productionScanFailedWaitingRetry)
            {
                scan = "持续采码";
            }
            else if (_batchActive)
            {
                scan = "等待采码";
            }
            else
            {
                scan = "未开始";
            }

            string robot;
            if (_productionBatchEnded)
            {
                robot = "空闲";
            }
            else if (_scanActive)
            {
                robot = _productionScanFailedWaitingRetry ? "换姿中" : "已到位";
            }
            else if (_robotPackingInProgress)
            {
                robot = "待下轮0";
            }
            else if (_batchActive && _productionScanFailedWaitingRetry)
            {
                robot = "换姿中";
            }
            else if (_batchActive)
            {
                robot = "待到位";
            }
            else
            {
                robot = "未到位";
            }

            UpdateStatusLabels(reader, scan, robot);
            UpdateOrderActionState();
        }

        private void EnterProductionRobotReadyState()
        {
            _productionBatchEnded = false;
            _robotPackingInProgress = false;
            _productionScanFailedWaitingRetry = false;
            SyncProductionWorkStateDisplay();
        }

        private void EnterProductionWaitingNextCycleState()
        {
            _robotPackingInProgress = true;
            _productionScanFailedWaitingRetry = false;
            SyncProductionWorkStateDisplay();
        }

        private void EnterProductionBatchEndedState()
        {
            StopSignalSendRetry();
            _productionBatchEnded = true;
            _robotPackingInProgress = false;
            _productionScanFailedWaitingRetry = false;
            _scanTimer.Stop();
            _scanActive = false;
            SyncProductionWorkStateDisplay();
        }

        private void EnterProductionWaitingScanState()
        {
            _robotPackingInProgress = false;
            _productionScanFailedWaitingRetry = true;
            SyncProductionWorkStateDisplay();
        }

        private void EnterProductionNotInPositionState()
        {
            _robotPackingInProgress = false;
            _productionScanFailedWaitingRetry = false;
            SyncProductionWorkStateDisplay();
        }

        private void AppendSignalLog(string message)
        {
            if (_txtSignalLog == null)
            {
                return;
            }

            _txtSignalLog.AppendText(DateTime.Now.ToString("HH:mm:ss.fff") + "  " + message + Environment.NewLine);
        }

        private static void LayoutWorkStateRow(Label title, Panel dot, Label value, int top, int dotX, int textX)
        {
            if (title == null || dot == null || value == null)
            {
                return;
            }

            dot.Location = new Point(dotX, top + 1);
            value.Location = new Point(textX, top);
        }

        private void WorkStateIndicator_Paint(object sender, PaintEventArgs e)
        {
            var panel = (Panel)sender;
            WorkStateVisual visual = panel.Tag is WorkStateVisual
                ? (WorkStateVisual)panel.Tag
                : WorkStateVisual.Neutral;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var brush = new SolidBrush(GetWorkStateDotColor(visual)))
            {
                e.Graphics.FillEllipse(brush, 1, 1, panel.Width - 2, panel.Height - 2);
            }
        }

        private static Color GetWorkStateDotColor(WorkStateVisual visual)
        {
            switch (visual)
            {
                case WorkStateVisual.Success:
                    return Color.FromArgb(46, 160, 67);
                case WorkStateVisual.Active:
                    return Color.FromArgb(32, 95, 178);
                case WorkStateVisual.Warning:
                    return Color.FromArgb(220, 140, 40);
                case WorkStateVisual.Error:
                    return Color.FromArgb(220, 60, 60);
                default:
                    return Color.FromArgb(150, 150, 150);
            }
        }

        private static Color GetWorkStateTextColor(WorkStateVisual visual)
        {
            switch (visual)
            {
                case WorkStateVisual.Success:
                    return Color.FromArgb(46, 160, 67);
                case WorkStateVisual.Active:
                    return Color.FromArgb(32, 95, 178);
                case WorkStateVisual.Warning:
                    return Color.FromArgb(180, 110, 30);
                case WorkStateVisual.Error:
                    return Color.FromArgb(180, 60, 60);
                default:
                    return Color.FromArgb(110, 110, 110);
            }
        }

        private static WorkStateVisual InferReaderStateVisual(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return WorkStateVisual.Neutral;
            }

            if (text.IndexOf("已连接", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (text.IndexOf("信号断开", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    text.IndexOf("发断开", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    text.IndexOf("收断开", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return WorkStateVisual.Warning;
                }

                return WorkStateVisual.Success;
            }

            if (text.IndexOf("失败", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("未连接", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return WorkStateVisual.Error;
            }

            return WorkStateVisual.Neutral;
        }

        private static WorkStateVisual InferScanStateVisual(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return WorkStateVisual.Neutral;
            }

            if (text.IndexOf("正在采码", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("持续采码", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("采码中", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return WorkStateVisual.Success;
            }

            if (text.IndexOf("等待采码", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("等待信号0", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("待重扫", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("批次进行中", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return WorkStateVisual.Active;
            }

            if (text.IndexOf("批次已结束", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("已载入", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("直读完成", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return WorkStateVisual.Success;
            }

            if (text.IndexOf("未开始", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return WorkStateVisual.Neutral;
            }

            return WorkStateVisual.Warning;
        }

        private static WorkStateVisual InferRobotStateVisual(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return WorkStateVisual.Neutral;
            }

            if (text.IndexOf("已到位", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return WorkStateVisual.Success;
            }

            if (text.IndexOf("待下轮0", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return WorkStateVisual.Active;
            }

            if (text.IndexOf("换姿中", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return WorkStateVisual.Warning;
            }

            if (text.IndexOf("待到位", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("未到位", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return WorkStateVisual.Warning;
            }

            if (text.IndexOf("空闲", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("批次已结束", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return WorkStateVisual.Neutral;
            }

            return WorkStateVisual.Neutral;
        }

        private static WorkStateVisual InferCamera3DStateVisual(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return WorkStateVisual.Neutral;
            }

            if (text.IndexOf("有物体", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.IndexOf("已检测", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return WorkStateVisual.Success;
            }

            return WorkStateVisual.Neutral;
        }

        private void ApplyWorkState(Label label, Panel indicator, string text, WorkStateVisual visual)
        {
            if (label == null || indicator == null)
            {
                return;
            }

            label.Text = text;
            label.ForeColor = GetWorkStateTextColor(visual);
            indicator.Tag = visual;
            indicator.Invalidate();
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

        private void InitOrderIndexGrid()
        {
            lblMatchedOrderNoTitle.Visible = false;
            lblMatchedOrderNoValue.Visible = false;
            lblMatchedBoxCodeTitle.Visible = false;
            lblMatchedBoxCodeValue.Visible = false;

            _dgvOrderIndex = new DataGridView
            {
                Name = "dgvOrderIndex",
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Height = 72
            };
            _dgvOrderIndex.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colOrderIndexOrderNo",
                HeaderText = "订单编号",
                FillWeight = 40
            });
            _dgvOrderIndex.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colOrderIndexBoxCode",
                HeaderText = "箱码编号",
                FillWeight = 35
            });
            _dgvOrderIndex.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colOrderIndexStatus",
                HeaderText = "订单状态",
                FillWeight = 32
            });
            _dgvOrderIndex.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colOrderIndexStatusDetail",
                HeaderText = "异常/明细",
                FillWeight = 60
            });
            _dgvOrderIndex.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colOrderIndexUpdatedAt",
                HeaderText = "更新时间",
                FillWeight = 45
            });
            _dgvOrderIndex.CellDoubleClick += dgvOrderIndex_CellDoubleClick;

            _btnDeleteOrderIndex = new Button
            {
                Name = "btnDeleteOrderIndex",
                Text = "删除选中",
                UseVisualStyleBackColor = true
            };
            _btnDeleteOrderIndex.Click += btnDeleteOrderIndex_Click;

            _btnClearOrderList = new Button
            {
                Name = "btnClearOrderList",
                Text = "清空列表",
                UseVisualStyleBackColor = true
            };
            _btnClearOrderList.Click += btnClearOrderList_Click;

            grpCurrentOrder.Controls.Add(_dgvOrderIndex);
            grpCurrentOrder.Controls.Add(_btnDeleteOrderIndex);
            grpCurrentOrder.Controls.Add(_btnClearOrderList);
        }

        private void dgvOrderIndex_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _orderIndex.Count)
            {
                return;
            }

            OrderIndexEntry entry = _orderIndex[e.RowIndex];
            txtOrderBoxInput.Text = entry.OrderNo;
            rdoInputByOrderNo.Checked = true;
            ApplyOrderLookup(LookupOrderInfo(entry.OrderNo, true));
        }

        private void btnDeleteOrderIndex_Click(object sender, EventArgs e)
        {
            if (_dgvOrderIndex == null || _dgvOrderIndex.SelectedRows.Count == 0)
            {
                MessageBox.Show(DialogOwner, "请先在订单列表中选择要删除的项。", "删除选中", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int rowIndex = _dgvOrderIndex.SelectedRows[0].Index;
            if (rowIndex < 0 || rowIndex >= _orderIndex.Count)
            {
                return;
            }

            OrderIndexEntry entry = _orderIndex[rowIndex];
            if (MessageBox.Show(
                DialogOwner,
                "确定删除订单 " + entry.OrderNo + " / 箱码 " + entry.BoxCode + " 吗？",
                "删除选中",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }

            RemoveOrderBinding(entry.OrderNo, entry.BoxCode);
            AppendLog("已从列表删除订单 " + entry.OrderNo + "，箱码 " + entry.BoxCode + "。");
        }

        private void btnClearOrderList_Click(object sender, EventArgs e)
        {
            if (_orderIndex.Count == 0)
            {
                return;
            }

            if (MessageBox.Show(
                DialogOwner,
                "确定清空全部订单列表吗？此操作将删除本地保存的订单数据。",
                "清空列表",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            foreach (OrderIndexEntry entry in _orderIndex.ToList())
            {
                OrderJsonStore.Delete(entry.OrderNo, entry.BoxCode);
            }

            _orderCatalog = OrderCatalog.Empty();
            _orderIndex.Clear();
            ResetMatchedOrderState();
            RefreshOrderIndexGrid();
            AppendLog("已清空订单列表。");
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
            btnConfirmOrderBox.SetBounds(btnX, 66, btnW, 26);

            const int actionRowY = 92;
            const int actionBtnW = 72;
            int actionGap = 6;
            int actionCount = 3;
            int actionTotalW = actionBtnW * actionCount + actionGap * (actionCount - 1);
            int actionStartX = Math.Max(left, clientW - actionTotalW - 10);

            if (_btnImportOrder != null)
            {
                _btnImportOrder.SetBounds(actionStartX, actionRowY, actionBtnW, 26);
            }

            if (_btnDeleteOrderIndex != null)
            {
                _btnDeleteOrderIndex.SetBounds(actionStartX + actionBtnW + actionGap, actionRowY, actionBtnW, 26);
            }

            if (_btnClearOrderList != null)
            {
                _btnClearOrderList.SetBounds(actionStartX + (actionBtnW + actionGap) * 2, actionRowY, actionBtnW, 26);
            }

            if (_dgvOrderIndex != null)
            {
                _dgvOrderIndex.SetBounds(left, 124, clientW - left - 10, 72);
            }

            int checkboxY = 206;
            if (_btnDirectReadForPacking != null)
            {
                _btnDirectReadForPacking.SetBounds(btnX, actionRowY, btnW, 26);
                _btnDirectReadForPacking.Visible = IsOrderDirectReadMode;
            }

            if (_chkNoOrderTestMode != null)
            {
                _chkNoOrderTestMode.AutoSize = true;
                _chkNoOrderTestMode.Location = new Point(left, checkboxY);
                _chkNoOrderTestMode.MaximumSize = new Size(clientW - left * 2, 0);
                checkboxY += 26;
            }

            if (_chkOrderDirectReadMode != null)
            {
                _chkOrderDirectReadMode.AutoSize = true;
                _chkOrderDirectReadMode.Location = new Point(left, checkboxY);
                _chkOrderDirectReadMode.MaximumSize = new Size(clientW - left * 2, 0);
            }

            btnOpenOrderMatch.Visible = !IsOrderDirectReadMode;
            btnConfirmOrderBox.Visible = !IsOrderDirectReadMode;
            if (_btnDeleteOrderIndex != null)
            {
                _btnDeleteOrderIndex.Visible = !IsOrderDirectReadMode;
            }
            if (_btnClearOrderList != null)
            {
                _btnClearOrderList.Visible = !IsOrderDirectReadMode;
            }
            if (_dgvOrderIndex != null)
            {
                _dgvOrderIndex.Visible = !IsOrderDirectReadMode;
            }

            const int requiredHeight = 262;
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
                btnWaitRobotSignal.SetBounds(left, top + rowHeight * row, colWidth, 29);
                if (_btnScanReset != null)
                {
                    _btnScanReset.SetBounds(left + colWidth + gap, top + rowHeight * row, colWidth, 29);
                }
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
                StopSignalSendRetry();
                _signalSendRetryTimer.Dispose();
                if (_camera != null)
                {
                    _camera.Dispose();
                    _camera = null;
                }
                StopProductionSignalClient();
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
            UpdateStatusLabels("未连接", "未开始", "未到位");
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
                string ordersDir = OrderJsonStore.GetOrdersDirectory();
                Directory.CreateDirectory(ordersDir);
                _orderCatalog = OrderCatalog.LoadOrdersDirectory(ordersDir);
                RebuildOrderIndexFromStorage();
                if (_orderIndex.Count > 0)
                {
                    AppendLog("已加载订单列表：" + _orderIndex.Count + " 条。");
                }
            }
            catch (Exception ex)
            {
                _orderCatalog = OrderCatalog.Empty();
                _orderIndex.Clear();
                AppendLog("加载订单列表失败：" + ex.Message);
            }
        }

        private void RebuildOrderIndexFromStorage()
        {
            _orderIndex.Clear();
            if (_orderCatalog == null || _orderCatalog.Count == 0)
            {
                RefreshOrderIndexGrid();
                return;
            }

            foreach (OrderBoxPair pair in _orderCatalog.GetDistinctBindings())
            {
                DateTime updatedAt = OrderJsonStore.ReadUpdatedAt(pair.OrderNo, pair.BoxCode);
                OrderJsonDocument document = OrderJsonStore.LoadByBinding(pair.OrderNo, pair.BoxCode);
                _orderIndex.Add(new OrderIndexEntry
                {
                    OrderNo = pair.OrderNo,
                    BoxCode = pair.BoxCode,
                    UpdatedAt = updatedAt == DateTime.MinValue ? DateTime.Now : updatedAt,
                    Status = document == null ? string.Empty : document.Status,
                    StatusDetail = document == null ? string.Empty : document.StatusDetail
                });
            }

            _orderIndex.Sort((a, b) => b.UpdatedAt.CompareTo(a.UpdatedAt));
            RefreshOrderIndexGrid();
        }

        private void RefreshOrderIndexGrid()
        {
            if (_dgvOrderIndex == null)
            {
                return;
            }

            _dgvOrderIndex.Rows.Clear();
            foreach (OrderIndexEntry entry in _orderIndex)
            {
                _dgvOrderIndex.Rows.Add(
                    entry.OrderNo,
                    entry.BoxCode,
                    string.IsNullOrWhiteSpace(entry.Status) ? "未完成" : entry.Status,
                    entry.StatusDetail ?? string.Empty,
                    entry.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }

        private void UpsertOrderIndexEntry(string orderNo, string boxCode, DateTime updatedAt)
        {
            OrderJsonDocument document = OrderJsonStore.LoadByBinding(orderNo, boxCode);
            UpsertOrderIndexEntry(
                orderNo,
                boxCode,
                updatedAt,
                document == null ? string.Empty : document.Status,
                document == null ? string.Empty : document.StatusDetail);
        }

        private void UpsertOrderIndexEntry(string orderNo, string boxCode, DateTime updatedAt, string status, string statusDetail)
        {
            _orderIndex.RemoveAll(e =>
                string.Equals(e.OrderNo, orderNo, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(e.BoxCode, boxCode, StringComparison.OrdinalIgnoreCase));
            _orderIndex.Add(new OrderIndexEntry
            {
                OrderNo = orderNo,
                BoxCode = boxCode,
                UpdatedAt = updatedAt,
                Status = status ?? string.Empty,
                StatusDetail = statusDetail ?? string.Empty
            });
            _orderIndex.Sort((a, b) => b.UpdatedAt.CompareTo(a.UpdatedAt));
            RefreshOrderIndexGrid();
        }

        private void PersistOrderBinding(OrderLookupResult lookup)
        {
            if (lookup == null)
            {
                return;
            }

            OrderJsonDocument existing = OrderJsonStore.LoadByBinding(lookup.OrderNo, lookup.BoxCode);
            OrderJsonDocument document = OrderJsonStore.FromLookupResult(lookup);
            if (existing != null)
            {
                document.Status = existing.Status;
                document.StatusDetail = existing.StatusDetail;
                document.CompletedAt = existing.CompletedAt;
            }
            document.UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            OrderJsonStore.Save(document);
            UpsertOrderIndexEntry(lookup.OrderNo, lookup.BoxCode, DateTime.Now);
        }

        private void RemoveOrderBinding(string orderNo, string boxCode)
        {
            OrderJsonStore.Delete(orderNo, boxCode);
            _orderCatalog = _orderCatalog.RemoveBinding(orderNo, boxCode);
            _orderIndex.RemoveAll(e =>
                string.Equals(e.OrderNo, orderNo, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(e.BoxCode, boxCode, StringComparison.OrdinalIgnoreCase));
            if (_orderInfoMatched &&
                (string.Equals(ReadMatchedLabel(lblMatchedOrderNoValue), orderNo, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(ReadMatchedLabel(lblMatchedBoxCodeValue), boxCode, StringComparison.OrdinalIgnoreCase)))
            {
                ResetMatchedOrderState();
            }
            RefreshOrderIndexGrid();
        }

        private bool ConfirmOverwriteOrderBinding(string orderNo, string boxCode, string sourceDescription)
        {
            OrderIndexEntry byOrder = _orderIndex.FirstOrDefault(e =>
                string.Equals(e.OrderNo, orderNo, StringComparison.OrdinalIgnoreCase));
            OrderIndexEntry byBox = _orderIndex.FirstOrDefault(e =>
                string.Equals(e.BoxCode, boxCode, StringComparison.OrdinalIgnoreCase));

            if (byOrder == null && byBox == null)
            {
                return true;
            }

            var lines = new List<string>();
            if (byOrder != null)
            {
                lines.Add("订单号 " + byOrder.OrderNo + " 已绑定箱码 " + byOrder.BoxCode);
            }
            if (byBox != null && (byOrder == null || !string.Equals(byBox.OrderNo, byOrder.OrderNo, StringComparison.OrdinalIgnoreCase)))
            {
                lines.Add("箱码 " + byBox.BoxCode + " 已绑定订单 " + byBox.OrderNo);
            }

            string message = "检测到与现有订单列表冲突：" +
                Environment.NewLine + Environment.NewLine +
                string.Join(Environment.NewLine, lines) +
                Environment.NewLine + Environment.NewLine +
                "新记录：订单 " + orderNo + " / 箱码 " + boxCode +
                Environment.NewLine + "来源：" + sourceDescription +
                Environment.NewLine + Environment.NewLine +
                "是否覆盖已有订单信息？";
            return MessageBox.Show(
                DialogOwner,
                message,
                "覆盖订单",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) == DialogResult.Yes;
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

            int imported = 0;
            int skipped = 0;
            foreach (OrderBoxPair pair in incoming.GetDistinctBindings())
            {
                OrderLookupResult lookup = incoming.Lookup(pair.OrderNo, true);
                if (lookup == null)
                {
                    continue;
                }

                if (!ConfirmOverwriteOrderBinding(lookup.OrderNo, lookup.BoxCode, sourceDescription))
                {
                    skipped++;
                    continue;
                }

                RemoveOrderBinding(lookup.OrderNo, lookup.BoxCode);
                OrderCatalog bindingCatalog = OrderCatalog.FromLookupResult(lookup);
                _orderCatalog = OrderCatalog.Merge(_orderCatalog, bindingCatalog, true);
                PersistOrderBinding(lookup);
                imported++;
            }

            if (imported > 0)
            {
                AppendLog("已读取订单：" + sourceDescription + "，导入 " + imported + " 条绑定。");
            }
            if (skipped > 0)
            {
                AppendLog("已跳过 " + skipped + " 条冲突或未确认的订单绑定。");
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
            OrderMatchCleared?.Invoke();
            RefreshActiveScanTables();
            RefreshSummaryPanels();
            UpdateOrderActionState();
        }

        private void UpdateOrderActionState()
        {
            bool orderControlsEnabled = !IsNoOrderTestMode && !IsOrderDirectReadMode;
            bool directReadControlsEnabled = IsOrderDirectReadMode;
            btnOpenOrderMatch.Enabled = orderControlsEnabled;
            if (_btnDeleteOrderIndex != null)
            {
                _btnDeleteOrderIndex.Enabled = orderControlsEnabled;
            }
            if (_btnClearOrderList != null)
            {
                _btnClearOrderList.Enabled = orderControlsEnabled;
            }
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

            if (IsProductionMode)
            {
                btnWaitRobotSignal.Enabled = _currentOrderConfirmed;
                if (_btnScanReset != null)
                {
                    _btnScanReset.Enabled = CanResetProductionScanWorkflow();
                }
            }
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
                UpdateStatusLabels(null, "未开始", "未到位");
                AppendLog("已切换为订单直读：输入箱码后点击「直读取装箱」，订单明细将直接作为装箱输入。");
            }
            else
            {
                UpdateStatusLabels(null, "未开始", null);
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
            UpdateStatusLabels(null, "未开始", "未到位");
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
                        Sku = sku ?? string.Empty,
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
                UpdateStatusLabels(null, "未开始", "未到位");
                AppendLog("已切换为无订单测试：无需匹配订单，开始批次后即可扫码，批次结束将导出统计。");
            }
            else
            {
                _currentOrderConfirmed = false;
                UpdateStatusLabels(null, "未开始", null);
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
            if (_dgvOrderIndex != null)
            {
                ConfigureReadOnlyGrid(_dgvOrderIndex);
            }
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
                _productionBatchEnded = false;
                EnterProductionNotInPositionState();
                TryStartProductionBatch();
            }
            else
            {
                _testRecords.Clear();
                _testSequence = 1;
                UpdateStatusLabels(null, "未开始", "未到位");
            }

            RefreshActiveScanTables();
            RefreshSummaryPanels();
            UpdateOrderActionState();
            AppendLog("确认订单编号 " + orderNo + "，箱码编号 " + boxCode + "。");
            RaiseOrderConfirmedForPacking();
        }

        private void RaiseOrderConfirmedForPacking()
        {
            if (!_orderInfoMatched || _orderMatchItems.Count == 0)
            {
                return;
            }

            List<ScannedItemInfo> items = BuildScannedItemsFromOrder(_orderMatchItems, out int skippedLines);
            if (items.Count == 0)
            {
                AppendLog("匹配结果无法转换为装箱物品，请检查订单明细尺寸或 ProductInfo。");
                return;
            }

            OrderConfirmedForPacking?.Invoke(items);
            if (IsProductionMode)
            {
                AppendLog("已确认订单，批次已开始，等待机械臂发送信号0。");
            }
            else
            {
                AppendLog("已确认订单，可开始采码。");
            }
            if (skippedLines > 0)
            {
                AppendLog("同步装箱时跳过 " + skippedLines + " 条缺少有效尺寸的明细。");
            }
        }

        private void btnOpenOrderMatch_Click(object sender, EventArgs e)
        {
            string input = txtOrderBoxInput.Text.Trim();
            if (input.Length == 0 && !_orderInfoMatched)
            {
                MessageBox.Show(DialogOwner, "请输入订单编号或箱码编号后再匹配。", "匹配信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (input.Length > 0)
            {
                ApplyOrderLookup(LookupOrderInfo(input, rdoInputByOrderNo.Checked));
            }

            if (_orderInfoMatched)
            {
                OpenOrderMatchEditDialog();
            }
        }

        private void OpenOrderMatchEditDialog()
        {
            using (var dialog = new OrderMatchDialog(_productCatalog))
            {
                dialog.Items = _orderMatchItems
                    .Select(i => new OrderMatchItem
                    {
                        Barcode = i.Barcode,
                        Sku = i.Sku,
                        OrderQuantity = i.OrderQuantity,
                        Length = i.Length,
                        Width = i.Width,
                        Height = i.Height
                    })
                    .ToList();

                if (dialog.ShowDialog(DialogOwner) != DialogResult.OK)
                {
                    return;
                }

                List<OrderMatchItem> editedItems = dialog.Items;
                EnrichMatchItemsFromProductCatalog(editedItems);
                _orderMatchItems.Clear();
                _orderMatchItems.AddRange(editedItems);

                string orderNo = CurrentOrderNo;
                string boxCode = CurrentBoxCode;
                var lookup = new OrderLookupResult(orderNo, boxCode, _orderMatchItems.ToList());
                RemoveOrderBinding(orderNo, boxCode);
                _orderCatalog = OrderCatalog.Merge(_orderCatalog, OrderCatalog.FromLookupResult(lookup), true);
                PersistOrderBinding(lookup);
                ApplyOrderLookup(lookup);
                AppendLog("已保存订单明细修改并覆盖订单文件：订单 " + orderNo + " / 箱码 " + boxCode + "。");
            }
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
            RaiseOrderMatchedForPacking();
        }

        private void RaiseOrderMatchedForPacking()
        {
            if (!_orderInfoMatched || _orderMatchItems.Count == 0)
            {
                return;
            }

            List<ScannedItemInfo> items = BuildScannedItemsFromOrder(_orderMatchItems, out int skippedLines);
            if (items.Count == 0)
            {
                AppendLog("匹配结果无法转换为装箱物品，请检查订单明细尺寸或 ProductInfo。");
                return;
            }

            OrderMatchedForPacking?.Invoke(items);
            AppendLog("已将匹配订单 " + items.Count + " 件物品同步到装箱算法。");
            if (skippedLines > 0)
            {
                AppendLog("同步装箱时跳过 " + skippedLines + " 条缺少有效尺寸的明细。");
            }
        }

        public void UpdateOrderPackingResults(IReadOnlyList<ScannedItemInfo> packingItems)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<IReadOnlyList<ScannedItemInfo>>(UpdateOrderPackingResults), packingItems);
                return;
            }

            _packingInfoBySku.Clear();
            _packingInfoByBarcode.Clear();
            _consumedPackingBoxIds.Clear();

            foreach (OrderMatchItem item in _orderMatchItems)
            {
                item.PackingSequences = string.Empty;
            }

            if (packingItems == null || packingItems.Count == 0)
            {
                RefreshVisibleMatchGrid();
                return;
            }

            foreach (ScannedItemInfo item in packingItems.OrderBy(i => i.PackingSequence))
            {
                if (item == null || item.PackingSequence <= 0)
                {
                    continue;
                }

                EnqueuePackingInfo(_packingInfoBySku, item.Sku, item);
                EnqueuePackingInfo(_packingInfoByBarcode, item.Barcode, item);
            }

            foreach (OrderMatchItem orderItem in _orderMatchItems)
            {
                var sequences = packingItems
                    .Where(i => i != null && i.PackingSequence > 0 &&
                        (string.Equals(i.Barcode, orderItem.Barcode, StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(i.Sku, orderItem.Sku, StringComparison.OrdinalIgnoreCase)))
                    .Select(i => i.PackingSequence)
                    .Distinct()
                    .OrderBy(i => i)
                    .Select(i => i.ToString())
                    .ToList();
                orderItem.PackingSequences = string.Join(",", sequences);
            }

            RefreshVisibleMatchGrid();
            AppendLog("装箱结果已回写订单匹配信息，更新装箱次序。");
        }

        private static void EnqueuePackingInfo(Dictionary<string, Queue<ScannedItemInfo>> map, string key, ScannedItemInfo item)
        {
            if (map == null || item == null || string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            key = key.Trim();
            Queue<ScannedItemInfo> queue;
            if (!map.TryGetValue(key, out queue))
            {
                queue = new Queue<ScannedItemInfo>();
                map[key] = queue;
            }

            queue.Enqueue(item);
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
                UpdateStatusLabels(null, "正在采码", null);
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
            if (IsProductionMode)
            {
                _productionBatchEnded = false;
                SyncProductionWorkStateDisplay();
                AppendLog("批次开始：" + _batchFolderName + "，等待机械臂信号0开始扫码。");
            }
            else
            {
                AppendLog("批次开始：" + _batchFolderName + "，图片目录：" + _batchImageDirectory);
            }
            return true;
        }

        private void TryEndProductionBatch(string reason)
        {
            if (!_batchActive || !IsProductionMode)
            {
                AppendLog("收到批次结束请求但未生成结果：批次激活=" + _batchActive +
                    "，工作模式=" + IsProductionMode +
                    "，订单=" + ValueOrDash(CurrentOrderNo) +
                    "，箱码=" + ValueOrDash(CurrentBoxCode) +
                    "，记录数=" + _batchRecords.Count + "。");
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
            PersistOrderStatusFromBatch(finishedBatchName);
            RefreshActiveScanTables();
            RaiseBatchCompleted();
            _batchRecords.Clear();
            AppendLog("批次结束：" + finishedBatchName + "，已生成统计文件并更新匹配结果。");
            _batchFolderName = string.Empty;
            _batchImageDirectory = string.Empty;
            return true;
        }

        private void PersistOrderStatusFromBatch(string batchName)
        {
            if (!IsProductionMode || !_orderInfoMatched)
            {
                return;
            }

            string orderNo = CurrentOrderNo;
            string boxCode = CurrentBoxCode;
            if (string.IsNullOrWhiteSpace(orderNo) || string.IsNullOrWhiteSpace(boxCode))
            {
                return;
            }

            string status;
            string detail;
            EvaluateOrderBatchStatus(_batchRecords, _orderMatchItems, out status, out detail);

            OrderJsonDocument document = OrderJsonStore.LoadByBinding(orderNo, boxCode);
            if (document == null)
            {
                document = OrderJsonStore.FromLookupResult(new OrderLookupResult(orderNo, boxCode, _orderMatchItems.ToList()));
            }

            document.Status = status;
            document.StatusDetail = detail;
            document.CompletedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            document.UpdatedAt = document.CompletedAt;
            OrderJsonStore.Save(document);
            UpsertOrderIndexEntry(orderNo, boxCode, DateTime.Now, status, detail);
            AppendLog("订单状态已更新：" + orderNo + " / " + boxCode + " = " + status +
                (string.IsNullOrWhiteSpace(detail) ? string.Empty : "（" + detail + "）") +
                "，批次=" + batchName + "。");
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

            StopSignalSendRetry();
            _productionBatchEnded = true;

            if (_scanActive)
            {
                StopScanCycle("批次结束信号");
            }

            if (_pendingProductionRecord != null)
            {
                AppendLog("工作模式：批次结束时存在未收到信号3确认的扫码记录，未计入数量：" + _pendingProductionRecord.Barcode + "。");
                _pendingProductionRecord = null;
            }

            TryEndProductionBatch("收到批次结束信号4");
            _currentOrderConfirmed = false;
            EnterProductionBatchEndedState();
            UpdateOrderActionState();
            RefreshActiveScanTables();
            RefreshSummaryPanels();
        }

        /// <summary>
        /// 供外部信号调用：机械臂已收到扫码成功信号，停止重发 1 并提交本件记录。
        /// </summary>
        public void OnProductionRobotAcknowledgedSignal()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(OnProductionRobotAcknowledgedSignal));
                return;
            }

            if (_signalSendRetryValue == ProductionSignalClient.SignalScanSuccess)
            {
                StopSignalSendRetry();
                AppendLog("工作模式：机械臂已确认收到信号1，停止重发并提交本件扫码记录。");
                CommitPendingProductionRecord();
            }
            else if (_pendingProductionRecord != null)
            {
                AppendLog("工作模式：收到信号3，提交本件待确认扫码记录。");
                CommitPendingProductionRecord();
            }
            else
            {
                AppendLog("工作模式：收到信号3确认，但当前没有待确认的信号1。");
            }
        }

        /// <summary>
        /// 供外部信号调用：机械臂已收到未扫到信号，停止重发 2，继续本轮扫码。
        /// </summary>
        public void OnProductionScanFailedAcknowledgedSignal()
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(OnProductionScanFailedAcknowledgedSignal));
                return;
            }

            if (_signalSendRetryValue == ProductionSignalClient.SignalScanFailed)
            {
                StopSignalSendRetry();
                _productionAwaitingCapture = false;
                _productionFailureNotified = false;
                AppendLog("工作模式：机械臂已确认收到信号2，停止本轮信号2重发，继续扫码；若仍未扫到码将再次发送信号2。");
            }
            else
            {
                AppendLog("工作模式：收到信号5确认，但当前没有待停止的信号2重发。");
            }
        }

        private void HandleProductionRobotSignal()
        {
            if (_productionBatchEnded)
            {
                AppendLog("工作模式：批次已结束，忽略信号0，请重新确认订单后开始下一批次。");
                return;
            }

            if (!EnsureReadyForProductionScan())
            {
                return;
            }

            if (_scanActive)
            {
                if (_productionFailureNotified)
                {
                    if (_signalSendRetryValue == ProductionSignalClient.SignalScanFailed)
                    {
                        AppendLog("工作模式：收到信号0，当前仍在等待发送信号2，尝试恢复发送。");
                        ResumePendingSignalSend();
                        return;
                    }

                    if (_productionSignalClient != null && _productionSignalClient.IsSendConnected)
                    {
                        AppendLog("工作模式：收到信号0，恢复未发送成功的信号2。");
                        BeginSignalSendRetry(ProductionSignalClient.SignalScanFailed, true);
                        return;
                    }
                }

                AppendLog("工作模式：收到信号0，但当前已在采码中，忽略重复请求。");
                return;
            }

            if (_pendingProductionRecord != null)
            {
                AppendLog("工作模式：上一件扫码结果仍等待信号3确认，忽略新的信号0。");
                return;
            }

            if (!_batchActive)
            {
                TryStartProductionBatch();
            }

            if (_productionSignalClient != null)
            {
                _productionSignalClient.ResetSendState();
            }

            StopSignalSendRetry();
            EnterProductionRobotReadyState();
            StartScanCycle("工作模式");
            AppendLog("工作模式：机械臂已到位，开始扫码。");
            RefreshSummaryPanels();
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
                if (IsProductionMode)
                {
                    SyncProductionWorkStateDisplay();
                    StartProductionSignalClient();
                }
                else
                {
                    UpdateStatusLabels("已连接", null, null);
                }
                AppendLog("扫码器 SDK 连接成功，已进入软件触发采码模式。");
            }
            catch (Exception ex)
            {
                _readerConnected = false;
                UpdateStatusLabels("未连接", null, null);
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
                if (IsProductionMode)
                {
                    StartProductionSignalClient();
                }
                _signalSendRetryTimer.Interval = _scannerSettings.SignalSendRetryIntervalMs;
                AppendLog("扫码器设置已保存：IP=" + _scannerSettings.ReaderIp
                    + "，采码频率=" + _scannerSettings.ScanIntervalMs + "ms"
                    + "，信号发送=" + _scannerSettings.SignalServerIp + ":" + _scannerSettings.SignalServerPort
                    + "，信号接收=" + _scannerSettings.SignalReceiveServerIp + ":" + _scannerSettings.SignalReceiveServerPort
                    + "，信号重发=" + FormatSignalSendRetryDescription()
                    + "，曝光=" + (_scannerSettings.ExposureAuto ? "自适应" : _scannerSettings.ExposureTimeUs + "us")
                    + "，增益=" + (_scannerSettings.GainAuto ? "自适应" : _scannerSettings.GainDb + "dB")
                    + "，自动对焦=" + (_scannerSettings.AutoFocus ? _scannerSettings.AutoFocusCommand : "关") + "。");
            }
        }

        private void btnWaitRobotSignal_Click(object sender, EventArgs e)
        {
            HandleProductionRobotSignal();
        }

        private void btnScanReset_Click(object sender, EventArgs e)
        {
            if (!IsProductionMode)
            {
                return;
            }

            if (!CanResetProductionScanWorkflow())
            {
                MessageBox.Show(DialogOwner, "当前没有进行中的工作批次或采码流程，无需归位。", "扫码归位",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult confirm = MessageBox.Show(DialogOwner,
                "确定要扫码归位吗？\n\n将停止当前采码、作废本批次（不生成统计），并重置工作流程。\n需重新确认订单后再开始新批次。",
                "扫码归位", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            ResetProductionScanWorkflow("人工扫码归位");
        }

        private bool CanResetProductionScanWorkflow()
        {
            return _batchActive || _scanActive || _currentOrderConfirmed || _robotPackingInProgress || _productionBatchEnded || _pendingProductionRecord != null;
        }

        private void ResetProductionScanWorkflow(string reason)
        {
            if (_scanActive)
            {
                StopScanCycle("扫码归位");
            }

            _productionAwaitingCapture = false;
            _productionFailureNotified = false;
            _productionScanFailedWaitingRetry = false;
            _robotPackingInProgress = false;
            _productionBatchEnded = false;
            _pendingProductionRecord = null;
            StopSignalSendRetry();

            if (_productionSignalClient != null)
            {
                _productionSignalClient.ResetSendState();
            }

            string abortedBatchName = _batchFolderName;
            if (_batchActive)
            {
                try
                {
                    _statisticsWriter.DeleteBatchIfExists(abortedBatchName);
                }
                catch (Exception ex)
                {
                    AppendLog("删除中断批次统计文件失败：" + ex.Message);
                }

                _batchActive = false;
                _batchFolderName = string.Empty;
                _batchImageDirectory = string.Empty;
                _batchRecords.Clear();
            }

            _currentProductionRecords.Clear();
            _nextSequence = 1;
            _currentOrderConfirmed = false;

            ScanItemsCleared?.Invoke();
            ClearScanResultDisplay();
            EnterProductionNotInPositionState();
            UpdateOrderActionState();
            RefreshActiveScanTables();
            RefreshSummaryPanels();

            string batchNote = string.IsNullOrWhiteSpace(abortedBatchName)
                ? string.Empty
                : "，已作废批次 " + abortedBatchName;
            AppendLog("扫码归位：" + reason + batchNote + "，工作流程已重置（不生成统计）。");
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
                .OrderBy(r => r.ScanTime)
                .ThenBy(r => r.Sequence)
                .ToList();
            records = SelectLatestHistoryVersion(records);
            records = records
                .OrderBy(r => r.Sequence)
                .ThenBy(r => r.ScanTime)
                .ToList();

            BindScanRecordGrid(dgvHistoryScanList, records);
            BindMatchGrid(dgvHistoryMatchResult, BuildMatchSummary(records, _orderMatchItems, false));
            lblHistoryResult.Text = records.Count == 0 ? "未找到该订单编号/箱码编号的采码历史。" : "查询到 " + records.Count + " 条采码记录。";
            AppendLog("历史查询：订单编号=" + orderNo + "，箱码编号=" + boxCode + "，结果=" + records.Count + " 条。");
        }

        private static List<ScanRecord> SelectLatestHistoryVersion(List<ScanRecord> records)
        {
            if (records == null || records.Count == 0)
            {
                return new List<ScanRecord>();
            }

            var latest = new List<ScanRecord>();
            int previousSequence = 0;
            foreach (ScanRecord record in records)
            {
                int sequence = record == null ? 0 : record.Sequence;
                if (latest.Count == 0 || sequence <= 1 || (previousSequence > 0 && sequence <= previousSequence))
                {
                    latest.Clear();
                }

                if (record != null)
                {
                    latest.Add(record);
                    previousSequence = sequence;
                }
            }

            return latest;
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

            if (IsProductionMode)
            {
                StartProductionSignalClient();
            }
            else
            {
                StopProductionSignalClient();
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

                if (IsProductionMode)
                {
                    if (_productionAwaitingCapture && !_productionFailureNotified)
                    {
                        BeginProductionScanFailureNotification();
                    }

                    _productionAwaitingCapture = true;
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
            _productionAwaitingCapture = false;
            _productionFailureNotified = false;
            StopSignalSendRetry();
            if (IsProductionMode && _productionSignalClient != null)
            {
                _productionSignalClient.ResetSendState();
            }
            _scanTimer.Interval = _scannerSettings.ScanIntervalMs;
            _scanTimer.Start();
            if (!IsProductionMode)
            {
                btnStopCurrentScan.Enabled = true;
            }
            if (IsProductionMode)
            {
                SyncProductionWorkStateDisplay();
            }
            else
            {
                UpdateStatusLabels(null, "正在采码", null);
            }
            AppendLog(mode + "：开始扫码。");
        }

        private void StopScanCycle(string reason)
        {
            bool wasActive = _scanActive;
            _scanTimer.Stop();
            StopSignalSendRetry();
            _scanActive = false;
            if (!IsProductionMode)
            {
                btnStopCurrentScan.Enabled = false;
            }
            if (_camera != null && wasActive)
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

            if (wasActive)
            {
                AppendLog("停止当前扫码：" + reason + "。");
            }

            if (wasActive)
            {
                if (IsProductionMode)
                {
                    SyncProductionWorkStateDisplay();
                }
                else
                {
                    SetScanWorkStatus("未开始");
                }
            }
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
            if (IsProductionMode)
            {
                _scanTimer.Stop();
                StopSignalSendRetry();
                _productionAwaitingCapture = false;
                _productionFailureNotified = false;
                _productionScanFailedWaitingRetry = false;
            }

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
            if (IsProductionMode)
            {
                if (_productionBatchEnded)
                {
                    AppendLog("工作模式：批次已结束，忽略本次扫码结果的信号1发送。");
                    return;
                }

                if (_productionSignalClient != null)
                {
                    _productionSignalClient.ResetSendState();
                }

                BeginSignalSendRetry(
                    ProductionSignalClient.SignalScanSuccess,
                    true);
                EnterProductionWaitingNextCycleState();
                AppendLog("工作模式：本轮采码成功，已关闭扫码器并发送信号1；等待信号3确认和下一轮信号0。" + FormatSignalSendRetryHint(false));
                RefreshSummaryPanels();
            }

            ScanRecord record;
            if (IsProductionMode)
            {
                _pendingProductionRecord = new ScanRecord
                {
                    Mode = "工作模式",
                    OrderNo = CurrentOrderNo,
                    BatchBoxCode = CurrentBoxCode,
                    Sequence = _nextSequence,
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
                ShowRecord(_pendingProductionRecord, "待确认采码");
                RefreshSummaryPanels();
                AppendLog("有效采码：" + barcode + "，已保存照片并停止扫码，等待机械臂返回信号3后更新数量和订单信息。");
                return;
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
                    UpdateStatusLabels(null, "未开始", "未到位");
                }
                else
                {
                    UpdateStatusLabels(null, "未开始", null);
                }
            }

            ShowRecord(record, IsProductionMode ? "当前采码" : "测试记录");
            RefreshSummaryPanels();
            ScannedItemAdded?.Invoke(BuildScannedItemInfoFromRecord(record));
            AppendLog("有效采码：" + barcode + "，已保存照片并停止扫码，数量+1。");
        }

        private void CommitPendingProductionRecord()
        {
            if (_pendingProductionRecord == null)
            {
                AppendLog("工作模式：没有待确认的扫码记录可提交。");
                return;
            }

            ScanRecord record = _pendingProductionRecord;
            _pendingProductionRecord = null;
            record.Sequence = _nextSequence++;
            record.Status = ResolveProductionStatus(record.Barcode, record.Sku);
            record.ScanCount = CountRecords(_currentProductionRecords, record.Barcode) + 1;
            ApplyPackingInfoToRecord(record);

            _currentProductionRecords.Add(record);
            _historyStore.Append(record);
            AddRecordToActiveBatch(record);
            WriteCurrentStatistics();
            RefreshActiveScanTables();
            ShowRecord(record, "当前采码");
            RefreshSummaryPanels();
            ScannedItemAdded?.Invoke(BuildScannedItemInfoFromRecord(record));

            AppendLog("工作模式：扫码记录已确认入账：" + record.Barcode + "，数量+1。");
            SendStraightPlacementCommandIfNeeded(record);
            if (IsProductionOrderComplete())
            {
                AppendLog("工作模式：订单采码数量已满足，等待机械臂发送信号4结束批次。");
            }
        }

        private void ApplyPackingInfoToRecord(ScanRecord record)
        {
            if (record == null)
            {
                return;
            }

            ScannedItemInfo packingInfo;
            if (!TryDequeuePackingInfo(_packingInfoBySku, record.Sku, out packingInfo) &&
                !TryDequeuePackingInfo(_packingInfoByBarcode, record.Barcode, out packingInfo))
            {
                return;
            }

            record.PackingSequence = packingInfo.PackingSequence;
            record.PackingBoxId = packingInfo.PackingBoxId ?? string.Empty;
            record.IsPackingLongShortSwapped = packingInfo.IsPackingLongShortSwapped;
            if (!string.IsNullOrWhiteSpace(record.PackingBoxId))
            {
                _consumedPackingBoxIds.Add(record.PackingBoxId);
            }
        }

        private bool TryDequeuePackingInfo(
            Dictionary<string, Queue<ScannedItemInfo>> map,
            string key,
            out ScannedItemInfo packingInfo)
        {
            packingInfo = null;
            if (map == null || string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            Queue<ScannedItemInfo> queue;
            if (!map.TryGetValue(key.Trim(), out queue) || queue.Count == 0)
            {
                return false;
            }

            while (queue.Count > 0)
            {
                ScannedItemInfo candidate = queue.Dequeue();
                string boxId = candidate == null ? string.Empty : candidate.PackingBoxId;
                if (string.IsNullOrWhiteSpace(boxId) || !_consumedPackingBoxIds.Contains(boxId))
                {
                    packingInfo = candidate;
                    return packingInfo != null;
                }
            }

            return false;
        }

        private void SendStraightPlacementCommandIfNeeded(ScanRecord record)
        {
            if (record == null || record.PackingSequence <= 0)
            {
                AppendLog("工作模式：未找到本件装箱位姿信息，跳过信号6/7。");
                return;
            }

            if (record.IsPackingLongShortSwapped)
            {
                AppendLog("工作模式：本件装箱需长短边转换，向机械臂发送信号7（位姿变换）。");
                SendProductionSignalDirect(ProductionSignalClient.SignalLongShortSwapped);
                return;
            }

            AppendLog("工作模式：本件装箱无需换边，向机械臂发送信号6（位姿变换）。");
            SendProductionSignalDirect(ProductionSignalClient.SignalStraightPlacement);
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
                Barcode = barcode ?? string.Empty,
                Sku = sku ?? string.Empty
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
            ScannedItemInfo item = BuildScannedItemInfo(product, record.Sku, length, width, height, record.Barcode);
            if (item != null)
            {
                item.ScanSequence = record.Sequence;
                item.PackingSequence = record.PackingSequence;
                item.PackingBoxId = record.PackingBoxId ?? string.Empty;
                item.IsPackingLongShortSwapped = record.IsPackingLongShortSwapped;
            }
            return item;
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
            if (_productionBatchEnded)
            {
                lblCurrentStateValue.Text = "批次已结束";
                lblCurrentStateValue.ForeColor = Color.FromArgb(46, 160, 67);
            }
            else if (_scanActive)
            {
                lblCurrentStateValue.Text = _productionScanFailedWaitingRetry ? "持续采码" : "正在采码";
                lblCurrentStateValue.ForeColor = Color.FromArgb(32, 95, 178);
            }
            else if (_robotPackingInProgress)
            {
                lblCurrentStateValue.Text = "等待下轮0";
                lblCurrentStateValue.ForeColor = Color.FromArgb(220, 140, 40);
            }
            else if (_batchActive && IsProductionMode && _productionScanFailedWaitingRetry)
            {
                lblCurrentStateValue.Text = "持续采码";
                lblCurrentStateValue.ForeColor = Color.FromArgb(32, 95, 178);
            }
            else if (_batchActive && IsProductionMode)
            {
                lblCurrentStateValue.Text = "批次进行中";
                lblCurrentStateValue.ForeColor = Color.FromArgb(32, 95, 178);
            }
            else
            {
                lblCurrentStateValue.Text = IsProductionMode ? "工作模式" : "测试模式";
                lblCurrentStateValue.ForeColor = Color.DarkOrange;
            }
            string batchHint = _batchActive && IsProductionMode ? " · 批次进行中" : string.Empty;
            if (_productionScanFailedWaitingRetry && IsProductionMode)
            {
                batchHint += " · 换姿重扫";
            }
            lblFooterStatus.Text = "订单编号: " + ValueOrDash(CurrentOrderNo) +
                " | 箱码编号: " + ValueOrDash(CurrentBoxCode) +
                " | 实扫: " + actualTotal +
                " | 异常: " + abnormal +
                " | 当前: " + (IsProductionMode ? "工作模式" : "测试模式") + batchHint;
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
            EnsurePackingSequenceColumn(grid);
            grid.Rows.Clear();
            foreach (MatchSummaryRow row in rows)
            {
                int index = grid.Rows.Add(
                    row.Barcode,
                    row.Sku,
                    row.OrderQuantity.HasValue ? row.OrderQuantity.Value.ToString() : string.Empty,
                    row.ActualQuantity,
                    row.Status,
                    string.IsNullOrWhiteSpace(row.PackingSequences) ? "-" : row.PackingSequences);
                ApplyStatusColor(grid.Rows[index], row.Status);
            }
        }

        private static void EnsurePackingSequenceColumn(DataGridView grid)
        {
            if (grid == null || grid.Columns.Contains("colMatchPackingSequence"))
            {
                return;
            }

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colMatchPackingSequence",
                HeaderText = "装箱次序"
            });
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
                        Status = ResolveGroupStatus(pair.Value, "测试模式"),
                        PackingSequences = string.Empty
                    });
                }
                return result;
            }

            foreach (OrderMatchItem expected in expectedItems)
            {
                List<ScanRecord> actual;
                actualGroups.TryGetValue(expected.Barcode, out actual);
                int actualCount = actual == null ? 0 : actual.Count;
                string status = actualCount == 0 ? "待扫码" : "正常";
                if (actual != null && actual.Any(r => string.Equals(r.Status, "未查询到该产品", StringComparison.OrdinalIgnoreCase)))
                {
                    status = "未查询到该产品";
                }
                else if (actualCount > 0 && actualCount < expected.OrderQuantity)
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
                    Status = status,
                    PackingSequences = expected.PackingSequences
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
                    Status = expectedItems.Count == 0 ? "未设置订单数量" : "未匹配",
                    PackingSequences = string.Empty
                });
            }

            return result.OrderBy(r => r.Barcode, StringComparer.OrdinalIgnoreCase).ToList();
        }

        private static void EvaluateOrderBatchStatus(List<ScanRecord> records, List<OrderMatchItem> expectedItems, out string status, out string detail)
        {
            List<MatchSummaryRow> summary = BuildMatchSummary(records ?? new List<ScanRecord>(), expectedItems ?? new List<OrderMatchItem>(), false);
            var missing = new List<string>();
            var over = new List<string>();
            var extra = new List<string>();
            var unknown = new List<string>();

            foreach (MatchSummaryRow row in summary)
            {
                string barcode = string.IsNullOrWhiteSpace(row.Barcode) ? "(空条码)" : row.Barcode;
                if (row.Status == "正常")
                {
                    continue;
                }

                if (row.Status == "待扫码" || row.Status == "数量不足")
                {
                    int expected = row.OrderQuantity.HasValue ? row.OrderQuantity.Value : 0;
                    int lack = Math.Max(0, expected - row.ActualQuantity);
                    missing.Add(barcode + " 缺少" + lack + "件");
                }
                else if (row.Status == "数量超出")
                {
                    int expected = row.OrderQuantity.HasValue ? row.OrderQuantity.Value : 0;
                    int more = Math.Max(0, row.ActualQuantity - expected);
                    over.Add(barcode + " 超出" + more + "件");
                }
                else if (row.Status == "未查询到该产品")
                {
                    unknown.Add(barcode + " 未查询到产品");
                }
                else
                {
                    extra.Add(barcode + " 数量" + row.ActualQuantity);
                }
            }

            var parts = new List<string>();
            if (missing.Count > 0)
            {
                parts.Add("缺少：" + string.Join("，", missing));
            }
            if (over.Count > 0)
            {
                parts.Add("数量超出：" + string.Join("，", over));
            }
            if (extra.Count > 0)
            {
                parts.Add("额外物体：" + string.Join("，", extra));
            }
            if (unknown.Count > 0)
            {
                parts.Add("未查询产品：" + string.Join("，", unknown));
            }

            if (summary.Count == 0)
            {
                status = "未完成";
                detail = "没有订单明细或采码记录";
                return;
            }

            status = parts.Count == 0 ? "已完成" : "异常";
            detail = string.Join("；", parts);
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

        private void ClearScanResultDisplay()
        {
            lblResultOrderNoValue.Text = "-";
            lblResultBatchBoxValue.Text = "-";
            lblResultSequenceValue.Text = "-";
            lblResultBarcodeValue.Text = "-";
            lblResultSkuValue.Text = "-";
            lblResultStatusValue.Text = "-";
            lblResultScanCountValue.Text = "-";
            lblResultScanTimeValue.Text = "-";
            lblPhotoBarcodeValue.Text = "-";
            lblPhotoSkuValue.Text = "-";
            lblPhotoSourceValue.Text = "-";
            LoadImage(string.Empty);

            if (dgvProductScanList != null)
            {
                dgvProductScanList.ClearSelection();
            }
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
            if (_btnScanReset != null)
            {
                _btnScanReset.Visible = production;
            }

            grpDeviceActions.Text = production ? "扫码器与信号" : "扫码器与测试";
            grpCurrentOrder.Visible = true;
            grpWorkState.Visible = production;
            SetModeTabs(production);
            lblModeHint.Text = production
                ? "工作模式：确认订单后开始批次；信号0开启持续采码；扫到发1并等信号3确认后入账；未扫到发2并等信号5停止重发；信号4批次结束并生成统计；异常可点「扫码归位」。"
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
            if (production && _tabSignalLog != null)
            {
                tabMain.TabPages.Add(_tabSignalLog);
            }
            tabMain.ResumeLayout();
        }

        private void UpdateStatusLabels(string reader, string scan, string robot)
        {
            if (reader != null)
            {
                ApplyWorkState(lblReaderStateValue, _pnlReaderStateIndicator, reader, InferReaderStateVisual(reader));
            }
            if (scan != null)
            {
                ApplyWorkState(lblScanStateValue, _pnlScanStateIndicator, scan, InferScanStateVisual(scan));
            }
            if (robot != null)
            {
                ApplyWorkState(lblRobotStateValue, _pnlRobotStateIndicator, robot, InferRobotStateVisual(robot));
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
                ExposureAuto = settings.ExposureAuto,
                GainAuto = settings.GainAuto,
                AutoReconnect = settings.AutoReconnect,
                SaveRawImage = settings.SaveRawImage,
                ImageSavePath = settings.ImageSavePath,
                LightMode = settings.LightMode,
                ExposureTimeUs = settings.ExposureTimeUs,
                GainDb = settings.GainDb,
                AcquisitionFrameRate = settings.AcquisitionFrameRate,
                UseAutoPacketSize = settings.UseAutoPacketSize,
                GevSCPSPacketSize = settings.GevSCPSPacketSize,
                GevHeartbeatTimeoutMs = settings.GevHeartbeatTimeoutMs,
                JpegQuality = settings.JpegQuality,
                ImageSaveFormat = settings.ImageSaveFormat,
                AutoFocusCommand = settings.AutoFocusCommand,
                SignalServerIp = settings.SignalServerIp,
                SignalServerPort = settings.SignalServerPort,
                SignalReceiveServerIp = settings.SignalReceiveServerIp,
                SignalReceiveServerPort = settings.SignalReceiveServerPort,
                SignalSendRetryIntervalMs = settings.SignalSendRetryIntervalMs,
                SignalSendRetryMaxCount = settings.SignalSendRetryMaxCount,
                SignalScanSuccessUntilStopped = settings.SignalScanSuccessUntilStopped
            };
        }

        private string FormatSignalSendRetryDescription()
        {
            return "间隔" + _scannerSettings.SignalSendRetryIntervalMs + "ms，信号1最多"
                + GetSignalSendRetryLimit(ProductionSignalClient.SignalScanSuccess, true)
                + "次并等3确认，信号2持续到5";
        }

        private string FormatSignalSendRetryHint(bool untilStopped)
        {
            int limit = GetSignalSendRetryLimit(_signalSendRetryValue, untilStopped);
            if (limit == int.MaxValue)
            {
                return " 将按间隔持续重发直至收到信号5确认，期间继续采码。";
            }

            if (limit <= 1)
            {
                return string.Empty;
            }

            return " 将按间隔重发，最多 " + limit + " 次。";
        }

        private int GetSignalSendRetryLimit(int signal, bool untilStopped)
        {
            if (signal == ProductionSignalClient.SignalScanFailed)
            {
                return int.MaxValue;
            }

            if (_scannerSettings.SignalSendRetryMaxCount > 0)
            {
                return _scannerSettings.SignalSendRetryMaxCount;
            }

            return untilStopped ? int.MaxValue : 300;
        }

        private bool ShouldContinueSignalSendRetry()
        {
            return _signalSendRetrySentCount < GetSignalSendRetryLimit(_signalSendRetryValue, _signalSendRetryUntilStopped);
        }

        private void BeginProductionScanFailureNotification()
        {
            _productionFailureNotified = true;
            EnterProductionWaitingScanState();
            BeginSignalSendRetry(ProductionSignalClient.SignalScanFailed, true);
            AppendLog("工作模式：未扫到有效条码，已发送信号2，机械臂换姿中，持续采码等待有效条码。" + FormatSignalSendRetryHint(true));
        }

        private void BeginSignalSendRetry(int signal, bool untilStopped)
        {
            StopSignalSendRetry();
            if (IsProductionMode && _productionBatchEnded)
            {
                AppendLog("工作模式：批次已结束，取消发送信号 " + signal + "。");
                return;
            }

            _signalSendRetryValue = signal;
            _signalSendRetryUntilStopped = untilStopped;
            _signalSendRetrySentCount = 0;

            if (!SendProductionSignalDirect(signal))
            {
                AppendLog("工作模式：信号 " + signal + " 暂未能发送，等待发送通道恢复后自动重试。");
                return;
            }

            _signalSendRetrySentCount = 1;
            StartSignalSendRetryTimerIfNeeded();
        }

        private void StopSignalSendRetry()
        {
            _signalSendRetryTimer.Stop();
            _signalSendRetryValue = -1;
            _signalSendRetrySentCount = 0;
            _signalSendRetryUntilStopped = false;
        }

        private void PauseSignalSendRetry()
        {
            _signalSendRetryTimer.Stop();
        }

        private void StartSignalSendRetryTimerIfNeeded()
        {
            if (!ShouldContinueSignalSendRetry())
            {
                return;
            }

            _signalSendRetryTimer.Interval = _scannerSettings.SignalSendRetryIntervalMs;
            _signalSendRetryTimer.Start();
        }

        private void ResumePendingSignalSend()
        {
            if (!IsProductionMode || _productionBatchEnded)
            {
                return;
            }

            if (_productionSignalClient == null || !_productionSignalClient.IsSendConnected)
            {
                return;
            }

            if (_signalSendRetryValue < 0)
            {
                if (_scanActive && _productionFailureNotified)
                {
                    BeginSignalSendRetry(ProductionSignalClient.SignalScanFailed, true);
                }

                return;
            }

            if (_signalSendRetrySentCount == 0)
            {
                if (!SendProductionSignalDirect(_signalSendRetryValue))
                {
                    return;
                }

                _signalSendRetrySentCount = 1;
            }

            StartSignalSendRetryTimerIfNeeded();
        }

        private void signalSendRetryTimer_Tick(object sender, EventArgs e)
        {
            if (IsProductionMode && _productionBatchEnded)
            {
                StopSignalSendRetry();
                return;
            }

            if (_signalSendRetryValue < 0)
            {
                StopSignalSendRetry();
                return;
            }

            if (_signalSendRetryUntilStopped &&
                _signalSendRetryValue == ProductionSignalClient.SignalScanFailed &&
                (!_scanActive || !_productionFailureNotified))
            {
                StopSignalSendRetry();
                return;
            }

            if (!ShouldContinueSignalSendRetry())
            {
                int limit = GetSignalSendRetryLimit(_signalSendRetryValue, _signalSendRetryUntilStopped);
                int signalValue = _signalSendRetryValue;
                StopSignalSendRetry();
                if (signalValue == ProductionSignalClient.SignalScanFailed)
                {
                    AppendLog("工作模式：信号2已达最大重发次数 " + limit + "，停止重发，持续采码等待有效条码。");
                }
                else
                {
                    AppendLog("工作模式：信号" + signalValue + "已达最大重发次数 " + limit + "。");
                }
                return;
            }

            if (!SendProductionSignalDirect(_signalSendRetryValue))
            {
                PauseSignalSendRetry();
                return;
            }

            _signalSendRetrySentCount++;
        }

        private void StartProductionSignalClient()
        {
            if (!IsProductionMode)
            {
                return;
            }

            if (_productionSignalClient == null)
            {
                _productionSignalClient = new ProductionSignalClient();
                _productionSignalClient.SignalReceived += ProductionSignalClient_SignalReceived;
                _productionSignalClient.ConnectionChanged += ProductionSignalClient_ConnectionChanged;
                _productionSignalClient.DataTransmitted += ProductionSignalClient_DataTransmitted;
            }

            AppendLog("正在连接生产信号服务器：发送通道 " + _scannerSettings.SignalServerIp + ":" + _scannerSettings.SignalServerPort
                + "，接收通道 " + _scannerSettings.SignalReceiveServerIp + ":" + _scannerSettings.SignalReceiveServerPort + "。");
            AppendSignalLog("连接发送通道 " + _scannerSettings.SignalServerIp + ":" + _scannerSettings.SignalServerPort + " ...");
            AppendSignalLog("连接接收通道 " + _scannerSettings.SignalReceiveServerIp + ":" + _scannerSettings.SignalReceiveServerPort + " ...");
            _productionSignalClient.Start(
                _scannerSettings.SignalServerIp,
                _scannerSettings.SignalServerPort,
                _scannerSettings.SignalReceiveServerIp,
                _scannerSettings.SignalReceiveServerPort,
                _scannerSettings.AutoReconnect);
        }

        private void StopProductionSignalClient()
        {
            if (_productionSignalClient == null)
            {
                return;
            }

            _productionSignalClient.Stop();
            EnterProductionNotInPositionState();
        }

        private void ProductionSignalClient_SignalReceived(int signal)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<int>(ProductionSignalClient_SignalReceived), signal);
                return;
            }

            AppendLog("收到生产信号：" + signal + "。");
            switch (signal)
            {
                case ProductionSignalClient.SignalRobotReady:
                    AppendLog("机械臂已到位，开始扫码。");
                    HandleProductionRobotSignal();
                    break;
                case ProductionSignalClient.SignalRobotAcknowledged:
                    AppendLog("机械臂确认已收到扫码成功信号，停止信号1重发并提交本件记录。");
                    OnProductionRobotAcknowledgedSignal();
                    break;
                case ProductionSignalClient.SignalBatchComplete:
                    AppendLog("机械臂通知本批次货物采集完毕，批次结束。");
                    OnProductionBatchEndSignal();
                    break;
                case ProductionSignalClient.SignalScanFailedAcknowledged:
                    AppendLog("机械臂确认已收到未扫到信号，停止信号2重发并继续扫码。");
                    OnProductionScanFailedAcknowledgedSignal();
                    break;
                default:
                    AppendLog("忽略未知生产信号：" + signal + "。");
                    break;
            }
        }

        private void ProductionSignalClient_DataTransmitted(bool sent, int value, string detail)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<bool, int, string>(ProductionSignalClient_DataTransmitted), sent, value, detail);
                return;
            }

            string direction = sent ? "发送 →" : "接收 ←";
            string dataText = value >= 0 ? "信号 " + value : "字符串数据";
            AppendSignalLog(direction + " " + dataText + "  (" + detail + ")");
        }

        private void ProductionSignalClient_ConnectionChanged(bool sendConnected, bool receiveConnected)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<bool, bool>(ProductionSignalClient_ConnectionChanged), sendConnected, receiveConnected);
                return;
            }

            if (sendConnected && receiveConnected)
            {
                AppendLog("生产信号双通道已连接：发送 " + _scannerSettings.SignalServerIp + ":" + _scannerSettings.SignalServerPort
                    + "，接收 " + _scannerSettings.SignalReceiveServerIp + ":" + _scannerSettings.SignalReceiveServerPort + "。");
                AppendSignalLog("信号双通道已建立：发送 " + _scannerSettings.SignalServerIp + ":" + _scannerSettings.SignalServerPort
                    + "，接收 " + _scannerSettings.SignalReceiveServerIp + ":" + _scannerSettings.SignalReceiveServerPort);
                ResumePendingSignalSend();
                SyncProductionWorkStateDisplay();
            }
            else
            {
                AppendSignalLog("信号通道状态：发送=" + (sendConnected ? "已连接" : "断开")
                    + "，接收=" + (receiveConnected ? "已连接" : "断开"));
                if (!sendConnected)
                {
                    PauseSignalSendRetry();
                    AppendLog("生产信号发送通道未连接，已暂停信号重发。");
                }
                else
                {
                    ResumePendingSignalSend();
                }

                if (!receiveConnected)
                {
                    AppendLog("生产信号接收通道断开。");
                    if (_scanActive)
                    {
                        StopScanCycle("信号接收通道断开");
                    }

                    EnterProductionNotInPositionState();
                }
                else
                {
                    SyncProductionWorkStateDisplay();
                }
            }
        }

        private bool SendProductionSignalDirect(int signal)
        {
            if (IsProductionMode && _productionBatchEnded)
            {
                AppendLog("发送信号 " + signal + " 已取消：批次已结束。");
                AppendSignalLog("发送取消：信号 " + signal + "，原因=批次已结束");
                return false;
            }

            if (_productionSignalClient == null || !_productionSignalClient.IsSendConnected)
            {
                AppendLog("发送信号 " + signal + " 失败：信号发送通道未连接。");
                AppendSignalLog("发送失败：信号 " + signal + "，原因=发送通道未连接");
                return false;
            }

            bool sent = _productionSignalClient.Send(signal, true);
            if (sent)
            {
                AppendLog("已向机械臂发送信号 " + signal + "。");
            }
            else
            {
                AppendLog("向机械臂发送信号 " + signal + " 失败。");
            }
            return sent;
        }

        private bool SendProductionStringDirect(string text)
        {
            if (IsProductionMode && _productionBatchEnded)
            {
                AppendLog("发送字符串 " + text + " 已取消：批次已结束。");
                AppendSignalLog("发送取消：字符串 " + text + "，原因=批次已结束");
                return false;
            }

            if (_productionSignalClient == null || !_productionSignalClient.IsSendConnected)
            {
                AppendLog("发送字符串 " + text + " 失败：信号发送通道未连接。");
                AppendSignalLog("发送失败：字符串 " + text + "，原因=发送通道未连接");
                return false;
            }

            bool sent = _productionSignalClient.SendString(text);
            if (sent)
            {
                AppendLog("已向机械臂发送字符串 " + text + "。");
            }
            else
            {
                AppendLog("向机械臂发送字符串 " + text + " 失败。");
            }
            return sent;
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
