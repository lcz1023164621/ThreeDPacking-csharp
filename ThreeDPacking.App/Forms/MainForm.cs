using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using ThreeDPacking.App.Communication;
using ThreeDPacking.App.Rendering;
using ThreeDPacking.Core.IO;
using ThreeDPacking.Core.Packers;
using WindowsFormsApp1;
using Container = ThreeDPacking.Core.Models.Container;
using Placement = ThreeDPacking.Core.Models.Placement;

namespace ThreeDPacking.App.Forms
{
    public partial class MainForm : Form
    {
        private readonly CameraOrbit _camera = new CameraOrbit();
        private readonly PackingRenderer _renderer = new PackingRenderer();

        private bool _glInitialized;
        private bool _mouseRotating;
        private bool _mousePanning;
        private Point _lastMouse;

        private List<ItemCandidate> _allLoadedItems = new List<ItemCandidate>();
        private List<ItemCandidate> _loadedItems = new List<ItemCandidate>();
        private List<Core.Models.Container> _packedContainers = new List<Core.Models.Container>();

        private Button btnInstanceSelect;
        private Button btnProbabilitySelect;

        private ComboBox cmbPaddingStrategy;
        private Label lblPaddingStrategy;
        private NumericUpDown numPaddingMinWidth;
        private Label lblPaddingMinWidth;
        private NumericUpDown numContainerSafetyDistance;
        private Label lblContainerSafetyDistance;
        private NumericUpDown numItemSafetyDistance;
        private Label lblItemSafetyDistance;
        private NumericUpDown numPaddingPaperSafetyDistance;
        private Label lblPaddingPaperSafetyDistance;
        private Button btnOpenExcel;
        private Button btnStartPacking;

        private GroupBox grpArmConnection;
        private Label lblArmAddress;
        private TextBox txtArmAddress;
        private Label lblArmPort;
        private NumericUpDown numArmPort;
        private Button btnArmConnect;
        private Button btnArmDisconnect;
        private Button btnArmSendCoordinate;
        private Panel pnlConnectionIndicator;
        private Label lblConnectionStatus;
        private RoboticArmClient _armClient;
        private bool _armConnecting;
        private bool _armSendingCoordinate;
        private CancellationTokenSource _armConnectCts;

        private GroupBox grpPackingPositions;
        private Label lblPackingPositionsInfo;
        private ListView lvPackingPositions;
        private const int PackingPositionsVisibleRows = 5;
        private const int PackingPositionsListHeaderHeight = 24;

        private GroupBox grpPackingPoints;
        private Label lblPackingPointsInfo;
        private ListView lvPackingPoints;
        private readonly List<string> _packingPointStrings = new List<string>();

        private Panel pnlSendStatusIndicator;
        private Label lblSendStatus;
        private Label lblSentPayload;
        private bool? _packingPointSendSuccess;

        private GroupBox grpScanInfo;
        private Label lblScanInfoInfo;
        private ListView lvScanInfo;
        private readonly List<ScannedItemInfo> _scannedItems = new List<ScannedItemInfo>();
        private readonly Dictionary<string, PackingSourceInfo> _packingSourceByBoxId =
            new Dictionary<string, PackingSourceInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Queue<PackingOrderEntry>> _packingOrderBySku =
            new Dictionary<string, Queue<PackingOrderEntry>>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Queue<PackingOrderEntry>> _packingOrderByBarcode =
            new Dictionary<string, Queue<PackingOrderEntry>>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _assignedPackingBoxIds =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private PaddingPaperFillStrategy _paddingPaperFillStrategy = PaddingPaperFillStrategy.MaxUtilization;
        private int _paddingPaperMinWidth = ThreeDPacking.Core.Models.PaddingPaper.DefaultWidth;
        private int _containerSafetyDistance = 0;
        private int _itemSafetyDistance = 0;
        private int _paddingPaperSafetyDistance = 0;
        private bool _selectionDirty = true;
        private bool _isRestoringState = false;
        private LastRunState _lastRunState;
        private ScanTestControl _scanTestControl;

        private sealed class PackingSourceInfo
        {
            public string Sku { get; set; }
            public string Barcode { get; set; }
            public int Dx { get; set; }
            public int Dy { get; set; }
        }

        private sealed class PackingOrderEntry
        {
            public int Sequence { get; set; }
            public string BoxId { get; set; }
            public string ContainerName { get; set; }
            public bool IsLongShortSwapped { get; set; }
        }

        public MainForm()
        {
            InitializeComponent();
            InitScanTestControl();
            InitInstanceButton();
            InitProbabilityButton();
            InitPaddingStrategyCombo();
            InitPaddingMinWidthControl();
            InitSafetyDistanceControls();
            InitFileRunButtons();
            InitArmConnectionControls();
            InitPackingPositionsControls();
            InitPackingPointsControls();
            InitSendStatusControls();
            InitScanInfoControls();
            WireEvents();
            menuStrip.Visible = false;
            AddDefaultContainer();
            btnRandomSelect.Enabled = false;
            if (btnInstanceSelect != null)
                btnInstanceSelect.Enabled = false;
            if (btnProbabilitySelect != null)
                btnProbabilitySelect.Enabled = false;
        }

        private void InitScanTestControl()
        {
            _scanTestControl = new ScanTestControl
            {
                Dock = DockStyle.Fill
            };
            _scanTestControl.ScannedItemAdded += ScanTestControl_ScannedItemAdded;
            _scanTestControl.ScanItemsCleared += ScanTestControl_ScanItemsCleared;
            _scanTestControl.BatchCompleted += ScanTestControl_BatchCompleted;
            _scanTestControl.OrderMatchedForPacking += ScanTestControl_OrderMatchedForPacking;
            _scanTestControl.OrderMatchCleared += ScanTestControl_OrderMatchCleared;
            _scanTestControl.OrderConfirmedForPacking += ScanTestControl_OrderConfirmedForPacking;
            panelScanTest.Controls.Add(_scanTestControl);
        }

        private void InitInstanceButton()
        {
            btnInstanceSelect = new Button();
            btnInstanceSelect.Name = "btnInstanceSelect";
            btnInstanceSelect.Text = "实例选择";
            btnInstanceSelect.UseVisualStyleBackColor = true;
            btnInstanceSelect.TabIndex = 4;
            btnInstanceSelect.Size = new Size(68, 23);
            btnInstanceSelect.Click += BtnInstanceSelect_Click;
            grpRandomSelect.Controls.Add(btnInstanceSelect);
        }

        private void InitProbabilityButton()
        {
            btnProbabilitySelect = new Button();
            btnProbabilitySelect.Name = "btnProbabilitySelect";
            btnProbabilitySelect.Text = "概率选择";
            btnProbabilitySelect.UseVisualStyleBackColor = true;
            btnProbabilitySelect.TabIndex = 5;
            btnProbabilitySelect.Size = new Size(68, 23);
            btnProbabilitySelect.Click += BtnProbabilitySelect_Click;
            grpRandomSelect.Controls.Add(btnProbabilitySelect);
        }

        private void InitPaddingStrategyCombo()
        {
            lblPaddingStrategy = new Label();
            lblPaddingStrategy.Text = "牛皮纸填充策略";
            lblPaddingStrategy.AutoSize = false;
            lblPaddingStrategy.TextAlign = ContentAlignment.MiddleLeft;

            cmbPaddingStrategy = new ComboBox();
            cmbPaddingStrategy.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPaddingStrategy.Items.Add("MaxUtilization（利用率优先）");
            cmbPaddingStrategy.Items.Add("StableLayerFill（稳定分层优先）");
            cmbPaddingStrategy.Items.Add("客户实际需求装填");
            cmbPaddingStrategy.SelectedIndex = 0;
            cmbPaddingStrategy.SelectedIndexChanged += CmbPaddingStrategy_SelectedIndexChanged;

            grpRandomSelect.Controls.Add(lblPaddingStrategy);
            grpRandomSelect.Controls.Add(cmbPaddingStrategy);
        }

        private void InitPaddingMinWidthControl()
        {
            lblPaddingMinWidth = new Label();
            lblPaddingMinWidth.Text = "牛皮纸最小宽度";
            lblPaddingMinWidth.AutoSize = false;
            lblPaddingMinWidth.TextAlign = ContentAlignment.MiddleLeft;

            numPaddingMinWidth = new NumericUpDown();
            numPaddingMinWidth.Minimum = ThreeDPacking.Core.Models.PaddingPaper.MinSize;
            numPaddingMinWidth.Maximum = 1000;
            numPaddingMinWidth.Value = _paddingPaperMinWidth;
            numPaddingMinWidth.Increment = 5;
            numPaddingMinWidth.TextAlign = HorizontalAlignment.Right;
            numPaddingMinWidth.ValueChanged += NumPaddingMinWidth_ValueChanged;

            grpRandomSelect.Controls.Add(lblPaddingMinWidth);
            grpRandomSelect.Controls.Add(numPaddingMinWidth);
        }

        private void InitSafetyDistanceControls()
        {
            lblContainerSafetyDistance = new Label();
            lblContainerSafetyDistance.Text = "容器边界安全距离";
            lblContainerSafetyDistance.AutoSize = false;
            lblContainerSafetyDistance.TextAlign = ContentAlignment.MiddleLeft;

            numContainerSafetyDistance = new NumericUpDown();
            numContainerSafetyDistance.Minimum = 0;
            numContainerSafetyDistance.Maximum = 1000;
            numContainerSafetyDistance.Value = _containerSafetyDistance;
            numContainerSafetyDistance.Increment = 1;
            numContainerSafetyDistance.TextAlign = HorizontalAlignment.Right;
            numContainerSafetyDistance.ValueChanged += NumContainerSafetyDistance_ValueChanged;

            lblItemSafetyDistance = new Label();
            lblItemSafetyDistance.Text = "物体间安全距离";
            lblItemSafetyDistance.AutoSize = false;
            lblItemSafetyDistance.TextAlign = ContentAlignment.MiddleLeft;

            numItemSafetyDistance = new NumericUpDown();
            numItemSafetyDistance.Minimum = 0;
            numItemSafetyDistance.Maximum = 1000;
            numItemSafetyDistance.Value = _itemSafetyDistance;
            numItemSafetyDistance.Increment = 1;
            numItemSafetyDistance.TextAlign = HorizontalAlignment.Right;
            numItemSafetyDistance.ValueChanged += NumItemSafetyDistance_ValueChanged;

            lblPaddingPaperSafetyDistance = new Label();
            lblPaddingPaperSafetyDistance.Text = "牛皮纸安全距离";
            lblPaddingPaperSafetyDistance.AutoSize = false;
            lblPaddingPaperSafetyDistance.TextAlign = ContentAlignment.MiddleLeft;

            numPaddingPaperSafetyDistance = new NumericUpDown();
            numPaddingPaperSafetyDistance.Minimum = 0;
            numPaddingPaperSafetyDistance.Maximum = 1000;
            numPaddingPaperSafetyDistance.Value = _paddingPaperSafetyDistance;
            numPaddingPaperSafetyDistance.Increment = 1;
            numPaddingPaperSafetyDistance.TextAlign = HorizontalAlignment.Right;
            numPaddingPaperSafetyDistance.ValueChanged += NumPaddingPaperSafetyDistance_ValueChanged;

            grpRandomSelect.Controls.Add(lblContainerSafetyDistance);
            grpRandomSelect.Controls.Add(numContainerSafetyDistance);
            grpRandomSelect.Controls.Add(lblItemSafetyDistance);
            grpRandomSelect.Controls.Add(numItemSafetyDistance);
            grpRandomSelect.Controls.Add(lblPaddingPaperSafetyDistance);
            grpRandomSelect.Controls.Add(numPaddingPaperSafetyDistance);
        }

        private void InitFileRunButtons()
        {
            btnOpenExcel = new Button();
            btnOpenExcel.Name = "btnOpenExcel";
            btnOpenExcel.Text = "选取文件";
            btnOpenExcel.UseVisualStyleBackColor = true;
            btnOpenExcel.Size = new Size(68, 23);
            btnOpenExcel.Click += MenuOpenExcel_Click;

            btnStartPacking = new Button();
            btnStartPacking.Name = "btnStartPacking";
            btnStartPacking.Text = "运行";
            btnStartPacking.UseVisualStyleBackColor = true;
            btnStartPacking.Enabled = false;
            btnStartPacking.Size = new Size(68, 23);
            btnStartPacking.Click += MenuStartPacking_Click;

            grpRandomSelect.Controls.Add(btnOpenExcel);
            grpRandomSelect.Controls.Add(btnStartPacking);
        }

        private void InitArmConnectionControls()
        {
            grpArmConnection = new GroupBox();
            grpArmConnection.Text = "机械臂连接";
            grpArmConnection.TabStop = false;

            lblArmAddress = new Label();
            lblArmAddress.Text = "地址：";
            lblArmAddress.AutoSize = true;

            txtArmAddress = new TextBox();
            txtArmAddress.Text = "192.168.0.200";

            lblArmPort = new Label();
            lblArmPort.Text = "端口：";
            lblArmPort.AutoSize = true;

            numArmPort = new NumericUpDown();
            numArmPort.Minimum = 1;
            numArmPort.Maximum = 65535;
            numArmPort.Value = 8055;
            numArmPort.TextAlign = HorizontalAlignment.Right;

            btnArmConnect = new Button();
            btnArmConnect.Text = "连接";
            btnArmConnect.UseVisualStyleBackColor = true;
            btnArmConnect.Click += BtnArmConnect_Click;

            btnArmDisconnect = new Button();
            btnArmDisconnect.Text = "断开";
            btnArmDisconnect.UseVisualStyleBackColor = true;
            btnArmDisconnect.Enabled = false;
            btnArmDisconnect.Click += BtnArmDisconnect_Click;

            btnArmSendCoordinate = new Button();
            btnArmSendCoordinate.Text = "发送坐标";
            btnArmSendCoordinate.UseVisualStyleBackColor = true;
            btnArmSendCoordinate.Enabled = false;
            btnArmSendCoordinate.Click += BtnArmSendCoordinate_Click;

            pnlConnectionIndicator = new Panel();
            pnlConnectionIndicator.Size = new Size(16, 16);
            pnlConnectionIndicator.BackColor = Color.Transparent;
            pnlConnectionIndicator.Paint += PnlConnectionIndicator_Paint;

            lblConnectionStatus = new Label();
            lblConnectionStatus.Text = "未连接";
            lblConnectionStatus.AutoSize = true;
            lblConnectionStatus.ForeColor = Color.FromArgb(180, 60, 60);

            grpArmConnection.Controls.Add(lblArmAddress);
            grpArmConnection.Controls.Add(txtArmAddress);
            grpArmConnection.Controls.Add(lblArmPort);
            grpArmConnection.Controls.Add(numArmPort);
            grpArmConnection.Controls.Add(btnArmConnect);
            grpArmConnection.Controls.Add(btnArmDisconnect);
            grpArmConnection.Controls.Add(btnArmSendCoordinate);
            grpArmConnection.Controls.Add(pnlConnectionIndicator);
            grpArmConnection.Controls.Add(lblConnectionStatus);

            panelActualPacking.Controls.Add(grpArmConnection);

            _armClient = new RoboticArmClient();
            _armClient.ConnectionChanged += ArmClient_ConnectionChanged;

            UpdateConnectionStatus(false);
        }

        private void InitPackingPositionsControls()
        {
            panelActualPacking.AutoScroll = true;

            grpPackingPositions = new GroupBox();
            grpPackingPositions.Text = "装箱位置";
            grpPackingPositions.TabStop = false;

            lblPackingPositionsInfo = new Label();
            lblPackingPositionsInfo.Text = "暂无装箱结果";
            lblPackingPositionsInfo.AutoSize = false;
            lblPackingPositionsInfo.Height = 18;

            lvPackingPositions = new ListView();
            lvPackingPositions.View = View.Details;
            lvPackingPositions.FullRowSelect = true;
            lvPackingPositions.GridLines = true;
            lvPackingPositions.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lvPackingPositions.Columns.Add("序号", 42, HorizontalAlignment.Center);
            lvPackingPositions.Columns.Add("容器", 72, HorizontalAlignment.Left);
            lvPackingPositions.Columns.Add("物品", 72, HorizontalAlignment.Left);
            lvPackingPositions.Columns.Add("顶点中心点", 110, HorizontalAlignment.Left);

            grpPackingPositions.Controls.Add(lblPackingPositionsInfo);
            grpPackingPositions.Controls.Add(lvPackingPositions);
            panelActualPacking.Controls.Add(grpPackingPositions);
        }

        private void InitPackingPointsControls()
        {
            grpPackingPoints = new GroupBox();
            grpPackingPoints.Text = "装箱点位";
            grpPackingPoints.TabStop = false;

            lblPackingPointsInfo = new Label();
            lblPackingPointsInfo.Text = "暂无装箱点位";
            lblPackingPointsInfo.AutoSize = false;
            lblPackingPointsInfo.Height = 18;

            lvPackingPoints = new ListView();
            lvPackingPoints.View = View.Details;
            lvPackingPoints.FullRowSelect = true;
            lvPackingPoints.GridLines = true;
            lvPackingPoints.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lvPackingPoints.Columns.Add("序号", 42, HorizontalAlignment.Center);
            lvPackingPoints.Columns.Add("物品", 72, HorizontalAlignment.Left);
            lvPackingPoints.Columns.Add("点位", 120, HorizontalAlignment.Left);
            lvPackingPoints.Columns.Add("下放点位", 120, HorizontalAlignment.Left);

            grpPackingPoints.Controls.Add(lblPackingPointsInfo);
            grpPackingPoints.Controls.Add(lvPackingPoints);
            panelActualPacking.Controls.Add(grpPackingPoints);
        }

        private void InitSendStatusControls()
        {
            pnlSendStatusIndicator = new Panel();
            pnlSendStatusIndicator.Size = new Size(16, 16);
            pnlSendStatusIndicator.BackColor = Color.Transparent;
            pnlSendStatusIndicator.Paint += PnlSendStatusIndicator_Paint;

            lblSendStatus = new Label();
            lblSendStatus.AutoSize = true;
            lblSendStatus.Text = "未传送";

            lblSentPayload = new Label();
            lblSentPayload.AutoSize = true;
            lblSentPayload.Visible = false;
            lblSentPayload.ForeColor = Color.FromArgb(64, 64, 64);

            grpPackingPoints.Controls.Add(pnlSendStatusIndicator);
            grpPackingPoints.Controls.Add(lblSendStatus);
            grpPackingPoints.Controls.Add(lblSentPayload);

            UpdateSendStatus(null);
        }

        private void InitScanInfoControls()
        {
            grpScanInfo = new GroupBox();
            grpScanInfo.Text = "扫码信息";
            grpScanInfo.TabStop = false;

            lblScanInfoInfo = new Label();
            lblScanInfoInfo.Text = "暂无扫码信息";
            lblScanInfoInfo.AutoSize = false;
            lblScanInfoInfo.Height = 18;

            lvScanInfo = new ListView();
            lvScanInfo.View = View.Details;
            lvScanInfo.FullRowSelect = true;
            lvScanInfo.GridLines = true;
            lvScanInfo.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lvScanInfo.Columns.Add("序号", 42, HorizontalAlignment.Center);
            lvScanInfo.Columns.Add("名称", 64, HorizontalAlignment.Left);
            lvScanInfo.Columns.Add("货号", 72, HorizontalAlignment.Left);
            lvScanInfo.Columns.Add("尺寸", 90, HorizontalAlignment.Left);
            lvScanInfo.Columns.Add("条码信息", 120, HorizontalAlignment.Left);
            lvScanInfo.Columns.Add("装箱顺序", 70, HorizontalAlignment.Center);

            grpScanInfo.Controls.Add(lblScanInfoInfo);
            grpScanInfo.Controls.Add(lvScanInfo);
            panelActualPacking.Controls.Add(grpScanInfo);
        }

        private void ScanTestControl_ScannedItemAdded(ScannedItemInfo item)
        {
            if (item == null)
                return;

            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => ScanTestControl_ScannedItemAdded(item)));
                return;
            }

            AssignPackingSequenceToScannedItem(item);
            _scannedItems.Add(item);
            RefreshScanInfoList();
        }

        private void ScanTestControl_ScanItemsCleared()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ScanTestControl_ScanItemsCleared);
                return;
            }

            _scannedItems.Clear();
            RefreshPackingPositionsList();
        }

        private void ScanTestControl_BatchCompleted(IReadOnlyList<ScannedItemInfo> items)
        {
            if (items == null || items.Count == 0)
                return;

            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => ScanTestControl_BatchCompleted(items)));
                return;
            }

            AppendLog($"[批次结束] 本批次共采码 {items.Count} 件，待装列表保持订单匹配结果。");
        }

        private void ScanTestControl_OrderMatchedForPacking(IReadOnlyList<ScannedItemInfo> items)
        {
            if (items == null || items.Count == 0)
                return;

            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => ScanTestControl_OrderMatchedForPacking(items)));
                return;
            }

            ApplyScannedItemsToPacking(items, "订单匹配");
        }

        private void ScanTestControl_OrderMatchCleared()
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)ScanTestControl_OrderMatchCleared);
                return;
            }

            _loadedItems.Clear();
            _allLoadedItems.Clear();
            _packingSourceByBoxId.Clear();
            _selectionDirty = true;
            ClearPackingResults();
            RefreshItemsGrid();
            SetStartPackingEnabled(false);
            btnRandomSelect.Enabled = false;
            if (btnInstanceSelect != null)
                btnInstanceSelect.Enabled = false;
            if (btnProbabilitySelect != null)
                btnProbabilitySelect.Enabled = false;
            lblRandomInfo.Text = "匹配订单后自动同步到待装列表";
        }

        private void ScanTestControl_OrderConfirmedForPacking(IReadOnlyList<ScannedItemInfo> items)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => ScanTestControl_OrderConfirmedForPacking(items)));
                return;
            }

            AppendLog("[订单确认] 已确认订单，可开始采码。");
        }

        private static bool TryConvertScannedItem(
            ScannedItemInfo scanned,
            IReadOnlyList<ItemCandidate> existing,
            out ItemCandidate candidate)
        {
            candidate = null;
            if (scanned == null || scanned.Dx <= 0 || scanned.Dy <= 0 || scanned.Dz <= 0)
                return false;

            int index = existing == null ? 0 : existing.Count;
            string name = ResolveDistinctPackingItemName(scanned, existing, index);
            candidate = new ItemCandidate(name, scanned.Dx, scanned.Dy, scanned.Dz, index + 1);
            return true;
        }

        private static string ResolveDistinctPackingItemName(
            ScannedItemInfo scanned,
            IReadOnlyList<ItemCandidate> existing,
            int index)
        {
            string baseName = ResolvePackingItemName(scanned, index);
            if (existing == null || existing.Count == 0)
                return baseName;

            int sameNameCount = 0;
            for (int i = 0; i < existing.Count; i++)
            {
                string existingName = existing[i].Name ?? string.Empty;
                if (string.Equals(existingName, baseName, StringComparison.OrdinalIgnoreCase) ||
                    existingName.StartsWith(baseName + "#", StringComparison.OrdinalIgnoreCase))
                {
                    sameNameCount++;
                }
            }

            return sameNameCount > 0 ? baseName + "#" + (sameNameCount + 1) : baseName;
        }

        private static string ResolvePackingItemName(ScannedItemInfo scanned, int index)
        {
            if (!string.IsNullOrWhiteSpace(scanned.Name))
                return scanned.Name.Trim();

            if (!string.IsNullOrWhiteSpace(scanned.Barcode))
                return scanned.Barcode.Trim();

            return "Item" + (index + 1);
        }

        private void ApplyScannedItemsToPacking(IReadOnlyList<ScannedItemInfo> items, string source)
        {
            var loaded = new List<ItemCandidate>();
            var skipped = new List<string>();
            var sourceMap = new Dictionary<string, PackingSourceInfo>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < items.Count; i++)
            {
                ItemCandidate candidate;
                var scanned = items[i];
                if (!TryConvertScannedItem(scanned, loaded, out candidate))
                {
                    if (scanned != null)
                    {
                        skipped.Add(string.IsNullOrWhiteSpace(scanned.Barcode)
                            ? (scanned.Name ?? $"#{i + 1}")
                            : scanned.Barcode);
                    }
                    continue;
                }

                loaded.Add(candidate);
                sourceMap[BuildPackingBoxId(candidate)] = new PackingSourceInfo
                {
                    Sku = scanned.Sku ?? string.Empty,
                    Barcode = scanned.Barcode ?? string.Empty,
                    Dx = candidate.Dx,
                    Dy = candidate.Dy
                };
            }

            if (loaded.Count == 0)
            {
                MessageBox.Show("订单物品无法转换为待装物品，请检查订单明细或 ProductInfo 中的尺寸信息。", "订单同步",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _loadedItems = loaded;
            _allLoadedItems = new List<ItemCandidate>(loaded);
            _packingSourceByBoxId.Clear();
            foreach (var pair in sourceMap)
                _packingSourceByBoxId[pair.Key] = pair.Value;
            _selectionDirty = true;
            ClearPackingResults();

            RefreshItemsGrid();
            SetStartPackingEnabled(true);
            btnRandomSelect.Enabled = loaded.Count > 0;
            if (btnInstanceSelect != null)
                btnInstanceSelect.Enabled = loaded.Count > 0;
            if (btnProbabilitySelect != null)
                btnProbabilitySelect.Enabled = loaded.Count > 0;
            lblRandomInfo.Text = $"已同步订单物品 {loaded.Count} 件";

            statusLabel.Text = $"已同步 {loaded.Count} 件订单物品到待装列表";
            AppendLog($"[订单同步] {source}，已将 {loaded.Count} 件物品同步到待装列表。");
            if (skipped.Count > 0)
                AppendLog($"[订单同步] 跳过 {skipped.Count} 件（尺寸无效）：{string.Join(", ", skipped)}");

            if (skipped.Count > 0)
            {
                MessageBox.Show(
                    $"已同步 {loaded.Count} 件物品到待装列表。\n另有 {skipped.Count} 件因尺寸无效被跳过。",
                    "订单同步", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ClearPackingResults()
        {
            _packedContainers.Clear();
            lstResults.Items.Clear();
            menuExportJson.Enabled = false;

            _renderer.Container = null;
            _renderer.Containers = new List<Container>();
            _renderer.SelectedPlacement = null;
            _renderer.CurrentStep = 0;
            trackStep.Maximum = 0;
            trackStep.Value = 0;
            lblStepInfo.Text = "0 / 0";
            lblSelectedInfo.Text = "无";
            statusUtilization.Text = string.Empty;
            statusTime.Text = string.Empty;

            RefreshPackingPositionsList();
            glControl.Invalidate();
        }

        private void RefreshScanInfoList()
        {
            if (lvScanInfo == null)
                return;

            lvScanInfo.BeginUpdate();
            lvScanInfo.Items.Clear();

            for (int i = 0; i < _scannedItems.Count; i++)
            {
                var item = _scannedItems[i];
                var listItem = new ListViewItem((i + 1).ToString());
                listItem.SubItems.Add(string.IsNullOrWhiteSpace(item.Name) ? "-" : item.Name);
                listItem.SubItems.Add(string.IsNullOrWhiteSpace(item.Sku) ? "-" : item.Sku);
                listItem.SubItems.Add(string.IsNullOrWhiteSpace(item.Dimensions) ? "-" : item.Dimensions);
                listItem.SubItems.Add(string.IsNullOrWhiteSpace(item.Barcode) ? "-" : item.Barcode);
                listItem.SubItems.Add(item.PackingSequence > 0 ? item.PackingSequence.ToString() : "-");
                lvScanInfo.Items.Add(listItem);
            }

            lvScanInfo.EndUpdate();

            if (lblScanInfoInfo != null)
            {
                int matchedPackingCount = _scannedItems.Count(i => i.PackingSequence > 0);
                lblScanInfoInfo.Text = _scannedItems.Count > 0
                    ? $"已扫码 {_scannedItems.Count} 件，已匹配装箱顺序 {matchedPackingCount} 件"
                    : "暂无扫码信息";
            }

            LayoutActualPackingPanel();
        }

        private void PnlSendStatusIndicator_Paint(object sender, PaintEventArgs e)
        {
            var panel = (Panel)sender;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var color = _packingPointSendSuccess == true
                ? Color.FromArgb(46, 160, 67)
                : Color.FromArgb(220, 60, 60);
            using (var brush = new SolidBrush(color))
            {
                e.Graphics.FillEllipse(brush, 1, 1, panel.Width - 2, panel.Height - 2);
            }
        }

        private void UpdateSendStatus(bool? success, string sentPayload = null)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => UpdateSendStatus(success, sentPayload)));
                return;
            }

            _packingPointSendSuccess = success;
            if (success == true)
            {
                lblSendStatus.Text = "传送成功";
                lblSendStatus.ForeColor = Color.FromArgb(46, 160, 67);
                if (!string.IsNullOrEmpty(sentPayload))
                {
                    lblSentPayload.Text = sentPayload;
                    lblSentPayload.Visible = true;
                }
            }
            else if (success == false)
            {
                lblSendStatus.Text = "传送失败";
                lblSendStatus.ForeColor = Color.FromArgb(180, 60, 60);
                lblSentPayload.Text = string.Empty;
                lblSentPayload.Visible = false;
            }
            else
            {
                lblSendStatus.Text = "未传送";
                lblSendStatus.ForeColor = Color.FromArgb(180, 60, 60);
                lblSentPayload.Text = string.Empty;
                lblSentPayload.Visible = false;
            }

            pnlSendStatusIndicator?.Invalidate();
            LayoutActualPackingPanel();
        }

        /// <summary>
        /// 按装箱顺序拼接所有下放点位，格式如 [x,y,z,0,0,0],[x,y,z,0,0,0]。
        /// </summary>
        private string FormatAllArmCoordinatesPayload()
        {
            return string.Join(",", _packingPointStrings);
        }

        private void UpdateArmSendButtonState()
        {
            if (btnArmSendCoordinate == null)
                return;

            btnArmSendCoordinate.Enabled = _armClient != null
                && _armClient.IsConnected
                && _packingPointStrings.Count > 0
                && !_armSendingCoordinate
                && !_armConnecting;
        }

        private async void BtnArmSendCoordinate_Click(object sender, EventArgs e)
        {
            if (_armClient == null || !_armClient.IsConnected)
            {
                MessageBox.Show("请先连接机械臂。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_packingPointStrings.Count == 0)
            {
                MessageBox.Show("请先运行装箱生成点位。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string payload = FormatAllArmCoordinatesPayload();

            _armSendingCoordinate = true;
            UpdateArmSendButtonState();
            try
            {
                await _armClient.SendCoordinateAsync(payload).ConfigureAwait(true);
                UpdateSendStatus(true, payload);

                statusLabel.Text = $"已发送 {_packingPointStrings.Count} 个装箱点位";
                AppendLog($"[机械臂] 已发送 {_packingPointStrings.Count} 个点位: {payload}（请在机械臂端立即点击接收）");
            }
            catch (Exception ex)
            {
                UpdateSendStatus(false);
                AppendLog($"[机械臂] 发送失败: {ex.Message}");
                MessageBox.Show("发送失败:\n" + ex.Message, "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _armSendingCoordinate = false;
                UpdateArmSendButtonState();
            }
        }

        private void PnlConnectionIndicator_Paint(object sender, PaintEventArgs e)
        {
            var panel = (Panel)sender;
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var color = _armClient != null && _armClient.IsConnected
                ? Color.FromArgb(46, 160, 67)
                : Color.FromArgb(220, 60, 60);
            using (var brush = new SolidBrush(color))
            {
                e.Graphics.FillEllipse(brush, 1, 1, panel.Width - 2, panel.Height - 2);
            }
        }

        private void UpdateConnectionStatus(bool connected)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => UpdateConnectionStatus(connected)));
                return;
            }

            lblConnectionStatus.Text = connected ? "已连接" : "未连接";
            lblConnectionStatus.ForeColor = connected
                ? Color.FromArgb(46, 160, 67)
                : Color.FromArgb(180, 60, 60);
            pnlConnectionIndicator.Invalidate();

            bool busy = _armConnecting;
            txtArmAddress.Enabled = !connected && !busy;
            numArmPort.Enabled = !connected && !busy;
            btnArmConnect.Enabled = !connected && !busy;
            btnArmDisconnect.Enabled = connected && !busy;
            UpdateArmSendButtonState();
        }

        private void ArmClient_ConnectionChanged(object sender, bool connected)
        {
            UpdateConnectionStatus(connected);
            if (!connected)
                UpdateSendStatus(null);
            else if (_packingPointStrings.Count > 0)
                AppendLog("[机械臂] 已连接，点击「发送坐标」按顺序发送全部下放点位。");
        }

        private async void BtnArmConnect_Click(object sender, EventArgs e)
        {
            string host = txtArmAddress.Text.Trim();
            if (string.IsNullOrEmpty(host))
            {
                MessageBox.Show("请输入机械臂地址。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int port = (int)numArmPort.Value;
            _armConnecting = true;
            UpdateConnectionStatus(false);
            btnArmConnect.Text = "连接中...";
            btnArmConnect.Enabled = false;

            _armConnectCts?.Cancel();
            _armConnectCts?.Dispose();
            _armConnectCts = new CancellationTokenSource();

            try
            {
                await _armClient.ConnectAsync(host, port, _armConnectCts.Token);
                statusLabel.Text = $"机械臂已连接: {host}:{port}";
            }
            catch (OperationCanceledException)
            {
                UpdateConnectionStatus(_armClient.IsConnected);
            }
            catch (Exception ex)
            {
                UpdateConnectionStatus(false);
                MessageBox.Show("连接失败:\n" + ex.Message, "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _armConnecting = false;
                btnArmConnect.Text = "连接";
                UpdateConnectionStatus(_armClient.IsConnected);
            }
        }

        private void BtnArmDisconnect_Click(object sender, EventArgs e)
        {
            _armConnectCts?.Cancel();
            _armClient.Disconnect();
            statusLabel.Text = "机械臂已断开";
            UpdateConnectionStatus(false);
            UpdateSendStatus(null);
        }

        private void LayoutActualPackingPanel()
        {
            if (grpArmConnection == null)
                return;

            int w = panelActualPacking.ClientSize.Width - 12;
            grpArmConnection.Location = new Point(6, 6);
            grpArmConnection.Size = new Size(Math.Max(200, w), 138);

            lblArmAddress.Location = new Point(12, 26);
            txtArmAddress.Location = new Point(52, 23);
            txtArmAddress.Size = new Size(Math.Max(80, w - 200), 21);

            lblArmPort.Location = new Point(12, 56);
            numArmPort.Location = new Point(52, 53);
            numArmPort.Size = new Size(80, 21);

            btnArmConnect.Location = new Point(145, 52);
            btnArmConnect.Size = new Size(60, 23);
            btnArmDisconnect.Location = new Point(211, 52);
            btnArmDisconnect.Size = new Size(60, 23);

            btnArmSendCoordinate.Location = new Point(12, 82);
            btnArmSendCoordinate.Size = new Size(90, 23);

            pnlConnectionIndicator.Location = new Point(110, 86);
            lblConnectionStatus.Location = new Point(132, 86);

            if (grpPackingPositions != null)
            {
                int top = grpArmConnection.Bottom + 8;
                int listHeight = GetFixedListViewHeight(lvPackingPositions);
                int groupHeight = 52 + listHeight;

                grpPackingPositions.Location = new Point(6, top);
                grpPackingPositions.Size = new Size(Math.Max(200, w), groupHeight);

                lblPackingPositionsInfo.Location = new Point(12, 22);
                lblPackingPositionsInfo.Width = grpPackingPositions.ClientSize.Width - 24;

                lvPackingPositions.Location = new Point(12, 44);
                lvPackingPositions.Size = new Size(grpPackingPositions.ClientSize.Width - 24, listHeight);

                LayoutPackingPositionsColumns();
            }

            if (grpPackingPoints != null)
            {
                int top = grpPackingPositions != null
                    ? grpPackingPositions.Bottom + 8
                    : grpArmConnection.Bottom + 8;
                int listHeight = GetFixedListViewHeight(lvPackingPoints);
                const int sendStatusBaseHeight = 22;
                int payloadLeft = 100;
                int payloadWidth = Math.Max(120, grpPackingPoints.ClientSize.Width - payloadLeft - 12);
                int sentPayloadHeight = 0;
                if (lblSentPayload != null && lblSentPayload.Visible)
                {
                    lblSentPayload.MaximumSize = new Size(payloadWidth, 0);
                    sentPayloadHeight = Math.Max(0, lblSentPayload.Height - sendStatusBaseHeight + 4);
                }

                int groupHeight = 52 + listHeight + sendStatusBaseHeight + sentPayloadHeight + 8;

                grpPackingPoints.Location = new Point(6, top);
                grpPackingPoints.Size = new Size(Math.Max(200, w), groupHeight);

                lblPackingPointsInfo.Location = new Point(12, 22);
                lblPackingPointsInfo.Width = grpPackingPoints.ClientSize.Width - 24;

                lvPackingPoints.Location = new Point(12, 44);
                lvPackingPoints.Size = new Size(grpPackingPoints.ClientSize.Width - 24, listHeight);

                int statusTop = 44 + listHeight + 6;
                pnlSendStatusIndicator.Location = new Point(12, statusTop + 2);
                lblSendStatus.Location = new Point(34, statusTop);

                if (lblSentPayload != null)
                {
                    lblSentPayload.Location = new Point(payloadLeft, statusTop);
                }

                LayoutPackingPointsColumns();
            }

            if (grpScanInfo != null)
            {
                int top = grpPackingPoints != null
                    ? grpPackingPoints.Bottom + 8
                    : grpPackingPositions != null
                        ? grpPackingPositions.Bottom + 8
                        : grpArmConnection.Bottom + 8;
                int listHeight = GetFixedListViewHeight(lvScanInfo);
                int groupHeight = 52 + listHeight;

                grpScanInfo.Location = new Point(6, top);
                grpScanInfo.Size = new Size(Math.Max(200, w), groupHeight);

                lblScanInfoInfo.Location = new Point(12, 22);
                lblScanInfoInfo.Width = grpScanInfo.ClientSize.Width - 24;

                lvScanInfo.Location = new Point(12, 44);
                lvScanInfo.Size = new Size(grpScanInfo.ClientSize.Width - 24, listHeight);

                LayoutScanInfoColumns();
            }
        }

        private void LayoutPackingPositionsColumns()
        {
            if (lvPackingPositions == null || lvPackingPositions.Columns.Count < 4)
                return;

            int listW = lvPackingPositions.ClientSize.Width - 4;
            lvPackingPositions.Columns[0].Width = 42;
            lvPackingPositions.Columns[3].Width = Math.Max(90, listW - 42 - 72 - 72);
            lvPackingPositions.Columns[1].Width = 72;
            lvPackingPositions.Columns[2].Width = Math.Max(60, listW - 42 - 72 - lvPackingPositions.Columns[3].Width);
        }

        private void LayoutPackingPointsColumns()
        {
            if (lvPackingPoints == null || lvPackingPoints.Columns.Count < 4)
                return;

            int listW = lvPackingPoints.ClientSize.Width - 4;
            int pointWidth = Math.Max(90, (listW - 42 - 72) / 2);
            lvPackingPoints.Columns[0].Width = 42;
            lvPackingPoints.Columns[1].Width = 72;
            lvPackingPoints.Columns[2].Width = pointWidth;
            lvPackingPoints.Columns[3].Width = Math.Max(90, listW - 42 - 72 - pointWidth);
        }

        private void LayoutScanInfoColumns()
        {
            if (lvScanInfo == null || lvScanInfo.Columns.Count < 6)
                return;

            int listW = lvScanInfo.ClientSize.Width - 4;
            lvScanInfo.Columns[0].Width = 42;
            lvScanInfo.Columns[1].Width = 64;
            lvScanInfo.Columns[2].Width = 72;
            lvScanInfo.Columns[3].Width = 90;
            lvScanInfo.Columns[5].Width = 70;
            lvScanInfo.Columns[4].Width = Math.Max(100, listW - 42 - 64 - 72 - 90 - 70);
        }

        private int GetFixedListViewHeight(ListView listView)
        {
            if (listView == null)
                return PackingPositionsListHeaderHeight + 20 * PackingPositionsVisibleRows + 2;

            int rowHeight = listView.Font.Height + 5;
            if (listView.Items.Count > 0)
            {
                var rect = listView.GetItemRect(0, ItemBoundsPortion.Entire);
                if (rect.Height > 0)
                    rowHeight = rect.Height;
            }

            return PackingPositionsListHeaderHeight + rowHeight * PackingPositionsVisibleRows + 2;
        }

        private void SetOpenExcelEnabled(bool enabled)
        {
            menuOpenExcel.Enabled = enabled;
            if (btnOpenExcel != null)
                btnOpenExcel.Enabled = enabled;
        }

        private void SetStartPackingEnabled(bool enabled)
        {
            menuStartPacking.Enabled = enabled;
            if (btnStartPacking != null)
                btnStartPacking.Enabled = enabled;
        }

        private void CmbPaddingStrategy_SelectedIndexChanged(object sender, EventArgs e)
        {
            // 仅在用户主动修改时标记 dirty：确保重启后仍按上次的策略复现
            switch (cmbPaddingStrategy.SelectedIndex)
            {
                case 1:
                    _paddingPaperFillStrategy = PaddingPaperFillStrategy.StableLayerFill;
                    break;
                case 2:
                    _paddingPaperFillStrategy = PaddingPaperFillStrategy.CustomerDemandFill;
                    break;
                default:
                    _paddingPaperFillStrategy = PaddingPaperFillStrategy.MaxUtilization;
                    break;
            }

            if (!_isRestoringState)
                _selectionDirty = true;
        }

        private void NumPaddingMinWidth_ValueChanged(object sender, EventArgs e)
        {
            _paddingPaperMinWidth = (int)numPaddingMinWidth.Value;
            if (!_isRestoringState)
                _selectionDirty = true;
        }

        private void NumContainerSafetyDistance_ValueChanged(object sender, EventArgs e)
        {
            _containerSafetyDistance = (int)numContainerSafetyDistance.Value;
            if (!_isRestoringState)
                _selectionDirty = true;
        }

        private void NumItemSafetyDistance_ValueChanged(object sender, EventArgs e)
        {
            _itemSafetyDistance = (int)numItemSafetyDistance.Value;
            if (!_isRestoringState)
                _selectionDirty = true;
        }

        private void NumPaddingPaperSafetyDistance_ValueChanged(object sender, EventArgs e)
        {
            _paddingPaperSafetyDistance = (int)numPaddingPaperSafetyDistance.Value;
            if (!_isRestoringState)
                _selectionDirty = true;
        }

        private void WireEvents()
        {
            this.Load += MainForm_Load;

            menuOpenExcel.Click += MenuOpenExcel_Click;
            menuExportJson.Click += MenuExportJson_Click;
            menuExit.Click += (s, e) => Close();
            menuStartPacking.Click += MenuStartPacking_Click;
            btnRandomSelect.Click += BtnRandomSelect_Click;
            // btnProbabilitySelect is created dynamically in InitProbabilityButton()
            btnAddContainer.Click += BtnAddContainer_Click;
            btnRemoveContainer.Click += BtnRemoveContainer_Click;

            glControl.Load += GlControl_Load;
            glControl.Paint += GlControl_Paint;
            glControl.Resize += GlControl_Resize;
            glControl.MouseDown += GlControl_MouseDown;
            glControl.MouseUp += GlControl_MouseUp;
            glControl.MouseMove += GlControl_MouseMove;
            glControl.MouseWheel += GlControl_MouseWheel;
            glControl.MouseClick += GlControl_MouseClick;

            trackStep.ValueChanged += TrackStep_ValueChanged;
            lstResults.SelectedIndexChanged += LstResults_SelectedIndexChanged;

            this.Resize += (s, e) =>
            {
                LayoutLeftPanel();
                LayoutActualPackingPanel();
            };
            this.FormClosing += MainForm_FormClosing;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _armConnectCts?.Cancel();
            _armClient?.Dispose();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LayoutLeftPanel();
            LayoutActualPackingPanel();
            TryRestoreLastRunState();
        }

        private void ApplyMainSplitLayout()
        {
            if (splitMain.Width <= 0)
                return;

            int available = splitMain.ClientSize.Width - splitMain.SplitterWidth;
            int target = available / 3;
            target = Math.Max(splitMain.Panel1MinSize, target);
            target = Math.Min(target, available - splitMain.Panel2MinSize);

            if (splitMain.SplitterDistance != target)
                splitMain.SplitterDistance = target;
        }

        private void LayoutLeftPanel()
        {
            ApplyMainSplitLayout();

            int w = panelLeft.ClientSize.Width - 12;
            int y = 6;

            lblContainers.Location = new Point(6, y);
            lblContainers.Width = w;
            y += 22;

            dgvContainers.Location = new Point(6, y);
            dgvContainers.Size = new Size(w, 95);
            y += 101;

            btnAddContainer.Location = new Point(6, y);
            btnRemoveContainer.Location = new Point(87, y);
            y += 29;

            lblItems.Location = new Point(6, y);
            lblItems.Width = w;
            y += 22;

            dgvItems.Location = new Point(6, y);
            dgvItems.Size = new Size(w, 120);
            y += 126;

            // 随机选择区域
            grpRandomSelect.Location = new Point(6, y);
            grpRandomSelect.Size = new Size(w, 248);
            lblRandomMin.Location = new Point(8, 20);
            numRandomMin.Location = new Point(48, 16);
            lblRandomMax.Location = new Point(115, 20);
            numRandomMax.Location = new Point(155, 16);
            int btnW = 68;
            int btnH = 23;
            int gap = 6;
            int btnX = Math.Max(0, w - (btnW * 2 + gap));
            btnProbabilitySelect.Location = new Point(btnX, 15);
            btnProbabilitySelect.Size = new Size(btnW, btnH);
            btnRandomSelect.Location = new Point(btnX + btnW + gap, 15);
            btnRandomSelect.Size = new Size(btnW, btnH);
            lblRandomInfo.Location = new Point(8, 46);
            btnInstanceSelect.Location = new Point(btnX, 43);
            btnInstanceSelect.Size = new Size(btnW, btnH);

            if (lblPaddingMinWidth != null && numPaddingMinWidth != null)
            {
                lblPaddingMinWidth.Location = new Point(8, 90);
                lblPaddingMinWidth.Size = new Size(120, 20);
                numPaddingMinWidth.Location = new Point(130, 88);
                numPaddingMinWidth.Size = new Size(68, 23);
            }

            if (lblContainerSafetyDistance != null && numContainerSafetyDistance != null)
            {
                lblContainerSafetyDistance.Location = new Point(8, 114);
                lblContainerSafetyDistance.Size = new Size(120, 20);
                numContainerSafetyDistance.Location = new Point(130, 112);
                numContainerSafetyDistance.Size = new Size(68, 23);
            }

            if (lblItemSafetyDistance != null && numItemSafetyDistance != null)
            {
                lblItemSafetyDistance.Location = new Point(8, 138);
                lblItemSafetyDistance.Size = new Size(120, 20);
                numItemSafetyDistance.Location = new Point(130, 136);
                numItemSafetyDistance.Size = new Size(68, 23);
            }

            if (lblPaddingPaperSafetyDistance != null && numPaddingPaperSafetyDistance != null)
            {
                lblPaddingPaperSafetyDistance.Location = new Point(8, 162);
                lblPaddingPaperSafetyDistance.Size = new Size(120, 20);
                numPaddingPaperSafetyDistance.Location = new Point(130, 160);
                numPaddingPaperSafetyDistance.Size = new Size(68, 23);

                if (btnOpenExcel != null && btnStartPacking != null)
                {
                    int actionBtnX = Math.Max(204, w - 16 - (btnW * 2 + gap));
                    btnOpenExcel.Location = new Point(actionBtnX, 160);
                    btnOpenExcel.Size = new Size(btnW, btnH);
                    btnStartPacking.Location = new Point(actionBtnX + btnW + gap, 160);
                    btnStartPacking.Size = new Size(btnW, btnH);
                }
            }
            
            // 牛皮纸填充策略下拉
            if (lblPaddingStrategy != null && cmbPaddingStrategy != null)
            {
                lblPaddingStrategy.Location = new Point(8, 188);
                lblPaddingStrategy.Size = new Size(w - 16, 18);
                cmbPaddingStrategy.Location = new Point(8, 206);
                cmbPaddingStrategy.Size = new Size(w - 16, 23);
            }

            grpRandomSelect.Size = new Size(w, 236);
            y += 242;

            lblResults.Location = new Point(6, y);
            lblResults.Width = w;
            y += 22;

            lstResults.Location = new Point(6, y);
            lstResults.Size = new Size(w, 70);
            y += 76;

            lblStep.Location = new Point(6, y);
            lblStep.Width = w;
            y += 22;

            int trackW = w - 70;
            trackStep.Location = new Point(6, y);
            trackStep.Width = trackW;
            trackStep.Height = 45;
            lblStepInfo.Location = new Point(6 + trackW + 6, y + 12);
            lblStepInfo.Width = 60;
            lblStepInfo.Height = 20;
            y += 50;

            grpSelected.Location = new Point(6, y);
            grpSelected.Size = new Size(w, 68);
            lblSelectedInfo.Location = new Point(8, 18);
            lblSelectedInfo.Size = new Size(w - 16, 45);
            y += 74;

            txtLog.Location = new Point(6, y);
            int logHeight = Math.Max(80, panelLeft.ClientSize.Height - y - 6);
            txtLog.Size = new Size(w, logHeight);
        }

        private string GetStateFilePath()
        {
            string dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ThreeDPacking");
            return Path.Combine(dir, "last_run_state.json");
        }

        private void TryRestoreLastRunState()
        {
            string path = GetStateFilePath();
            if (!File.Exists(path))
                return;

            try
            {
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var serializer = new DataContractJsonSerializer(typeof(LastRunState));
                    var state = (LastRunState)serializer.ReadObject(fs);
                    if (state == null)
                        return;
                    _lastRunState = state;
                }

                _isRestoringState = true;

                _loadedItems = (_lastRunState.LoadedItems ?? new List<LastRunItemCandidate>())
                    .Select(p => new ItemCandidate(p.Name, p.Dx, p.Dy, p.Dz, p.InstanceId, p.Probability))
                    .ToList();

                RefreshItemsGrid();

                dgvContainers.Rows.Clear();
                foreach (var c in (_lastRunState.ContainerCandidates ?? new List<LastRunContainerCandidate>()))
                {
                    dgvContainers.Rows.Add(
                        NormalizeContainerName(c.Name),
                        c.Dx.ToString(),
                        c.Dy.ToString(),
                        c.Dz.ToString(),
                        c.EmptyWeight.ToString(),
                        c.MaxLoadWeight.ToString());
                }

                _paddingPaperFillStrategy = (PaddingPaperFillStrategy)_lastRunState.PaddingStrategy;
                if (_paddingPaperFillStrategy == PaddingPaperFillStrategy.StableLayerFill)
                    cmbPaddingStrategy.SelectedIndex = 1;
                else if (_paddingPaperFillStrategy == PaddingPaperFillStrategy.CustomerDemandFill)
                    cmbPaddingStrategy.SelectedIndex = 2;
                else
                    cmbPaddingStrategy.SelectedIndex = 0;
                _paddingPaperMinWidth = _lastRunState.PaddingMinWidth > 0
                    ? _lastRunState.PaddingMinWidth
                    : ThreeDPacking.Core.Models.PaddingPaper.DefaultWidth;
                numPaddingMinWidth.Value = Math.Max((int)numPaddingMinWidth.Minimum,
                    Math.Min((int)numPaddingMinWidth.Maximum, _paddingPaperMinWidth));
                _containerSafetyDistance = Math.Max(0, _lastRunState.ContainerSafetyDistance);
                numContainerSafetyDistance.Value = Math.Max((int)numContainerSafetyDistance.Minimum,
                    Math.Min((int)numContainerSafetyDistance.Maximum, _containerSafetyDistance));
                _itemSafetyDistance = Math.Max(0, _lastRunState.ItemSafetyDistance);
                numItemSafetyDistance.Value = Math.Max((int)numItemSafetyDistance.Minimum,
                    Math.Min((int)numItemSafetyDistance.Maximum, _itemSafetyDistance));
                _paddingPaperSafetyDistance = Math.Max(0, _lastRunState.PaddingPaperSafetyDistance);
                numPaddingPaperSafetyDistance.Value = Math.Max((int)numPaddingPaperSafetyDistance.Minimum,
                    Math.Min((int)numPaddingPaperSafetyDistance.Maximum, _paddingPaperSafetyDistance));

                _selectionDirty = false;
                SetStartPackingEnabled(_loadedItems.Count > 0);

                statusLabel.Text = $"已恢复上次选择（{_loadedItems.Count} 件物品）";
                AppendLog($"[恢复] 使用上次的物体选择与装箱随机种子复现。策略={_paddingPaperFillStrategy}");
            }
            catch
            {
                // ignore: 恢复失败不影响正常使用
            }
            finally
            {
                _isRestoringState = false;
            }
        }

        private void SaveLastRunState(long packRandomSeed, List<ContainerCandidate> containerCandidates)
        {
            try
            {
                var state = new LastRunState
                {
                    RandomSeed = packRandomSeed,
                    PaddingStrategy = (int)_paddingPaperFillStrategy,
                    PaddingMinWidth = _paddingPaperMinWidth,
                    ContainerSafetyDistance = _containerSafetyDistance,
                    ItemSafetyDistance = _itemSafetyDistance,
                    PaddingPaperSafetyDistance = _paddingPaperSafetyDistance,
                    LoadedItems = _loadedItems.Select(i => new LastRunItemCandidate
                    {
                        Name = i.Name,
                        Dx = i.Dx,
                        Dy = i.Dy,
                        Dz = i.Dz,
                        InstanceId = i.InstanceId,
                        Probability = i.Probability
                    }).ToList(),
                    ContainerCandidates = containerCandidates.Select(c => new LastRunContainerCandidate
                    {
                        Name = c.Name,
                        Dx = c.Dx,
                        Dy = c.Dy,
                        Dz = c.Dz,
                        EmptyWeight = c.EmptyWeight,
                        MaxLoadWeight = c.MaxLoadWeight
                    }).ToList()
                };

                string path = GetStateFilePath();
                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var serializer = new DataContractJsonSerializer(typeof(LastRunState));
                    serializer.WriteObject(fs, state);
                }

                _lastRunState = state;
            }
            catch (Exception ex)
            {
                AppendLog($"[保存] last_run_state.json 失败: {ex.Message}");
            }
        }

        [DataContract]
        private class LastRunState
        {
            [DataMember] public long RandomSeed { get; set; }
            [DataMember] public int PaddingStrategy { get; set; }
            [DataMember] public int PaddingMinWidth { get; set; }
            [DataMember] public int ContainerSafetyDistance { get; set; }
            [DataMember] public int ItemSafetyDistance { get; set; }
            [DataMember] public int PaddingPaperSafetyDistance { get; set; }
            [DataMember] public List<LastRunItemCandidate> LoadedItems { get; set; }
            [DataMember] public List<LastRunContainerCandidate> ContainerCandidates { get; set; }
        }

        [DataContract]
        private class LastRunItemCandidate
        {
            [DataMember] public string Name { get; set; }
            [DataMember] public int Dx { get; set; }
            [DataMember] public int Dy { get; set; }
            [DataMember] public int Dz { get; set; }
            [DataMember] public int InstanceId { get; set; }
            [DataMember] public double Probability { get; set; }
        }

        [DataContract]
        private class LastRunContainerCandidate
        {
            [DataMember] public string Name { get; set; }
            [DataMember] public int Dx { get; set; }
            [DataMember] public int Dy { get; set; }
            [DataMember] public int Dz { get; set; }
            [DataMember] public int EmptyWeight { get; set; }
            [DataMember] public int MaxLoadWeight { get; set; }
        }

        private static string NormalizeContainerName(string name)
        {
            return name == "8" ? "最大容器" : name;
        }

        private static string FormatTopCenterPoint(Placement placement)
        {
            int topCenterX = placement.X + placement.StackValue.Dx / 2;
            int topCenterY = placement.Y + placement.StackValue.Dy / 2;
            int topCenterZ = placement.Z + placement.StackValue.Dz;
            return $"({topCenterX},{topCenterY},{topCenterZ})";
        }

        private const double ArmCoordinateMmToMeters = 0.001;
        private const int ArmDropOffsetMm = 460;

        private static string FormatArmCoordinateArray(int topCenterX, int topCenterY, int zMm)
        {
            string x = (topCenterX * ArmCoordinateMmToMeters).ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            string y = (topCenterY * ArmCoordinateMmToMeters).ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            string z = (zMm * ArmCoordinateMmToMeters).ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
            return $"[{x},{y},{z},0,0,0]";
        }

        private static string FormatArmCoordinate(Placement placement)
        {
            int topCenterX = placement.X + placement.StackValue.Dx / 2;
            int topCenterY = placement.Y + placement.StackValue.Dy / 2;
            int topCenterZ = placement.Z + placement.StackValue.Dz;
            return FormatArmCoordinateArray(topCenterX, topCenterY, topCenterZ);
        }

        private static string FormatArmDropCoordinate(Placement placement)
        {
            int topCenterX = placement.X + placement.StackValue.Dx / 2;
            int topCenterY = placement.Y + placement.StackValue.Dy / 2;
            int topCenterZ = placement.Z + placement.StackValue.Dz;
            return FormatArmCoordinateArray(topCenterX, topCenterY, topCenterZ - ArmDropOffsetMm);
        }

        private static string BuildPackingBoxId(ItemCandidate candidate)
        {
            if (candidate == null)
                return string.Empty;

            return candidate.Name + "#" + candidate.InstanceId;
        }

        private static string NormalizePackingMatchKey(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
        }

        private static void EnqueuePackingOrder(
            Dictionary<string, Queue<PackingOrderEntry>> orders,
            string key,
            PackingOrderEntry entry)
        {
            key = NormalizePackingMatchKey(key);
            if (orders == null || string.IsNullOrWhiteSpace(key) || entry == null)
                return;

            Queue<PackingOrderEntry> queue;
            if (!orders.TryGetValue(key, out queue))
            {
                queue = new Queue<PackingOrderEntry>();
                orders[key] = queue;
            }

            queue.Enqueue(entry);
        }

        private static bool IsLongShortSwapped(PackingSourceInfo source, Placement placement)
        {
            if (source == null || placement?.StackValue == null || source.Dx <= 0 || source.Dy <= 0)
                return false;

            return source.Dx != source.Dy &&
                placement.StackValue.Dx == source.Dy &&
                placement.StackValue.Dy == source.Dx;
        }

        private static bool TryDequeuePackingOrder(
            Dictionary<string, Queue<PackingOrderEntry>> orders,
            string key,
            HashSet<string> assignedBoxIds,
            out PackingOrderEntry entry)
        {
            entry = null;
            key = NormalizePackingMatchKey(key);
            if (orders == null || string.IsNullOrWhiteSpace(key))
                return false;

            Queue<PackingOrderEntry> queue;
            if (!orders.TryGetValue(key, out queue) || queue.Count == 0)
                return false;

            while (queue.Count > 0)
            {
                entry = queue.Dequeue();
                string boxId = entry == null ? string.Empty : entry.BoxId;
                if (assignedBoxIds == null || string.IsNullOrWhiteSpace(boxId) || !assignedBoxIds.Contains(boxId))
                    return entry != null;
            }

            entry = null;
            return false;
        }

        private void AssignPackingSequenceToScannedItem(ScannedItemInfo item)
        {
            if (item == null)
                return;

            item.PackingSequence = 0;
            item.PackingBoxId = string.Empty;

            PackingOrderEntry entry;
            if (!TryDequeuePackingOrder(_packingOrderBySku, item.Sku, _assignedPackingBoxIds, out entry) &&
                !TryDequeuePackingOrder(_packingOrderByBarcode, item.Barcode, _assignedPackingBoxIds, out entry))
            {
                return;
            }

            item.PackingSequence = entry.Sequence;
            item.PackingBoxId = entry.BoxId ?? string.Empty;
            item.IsPackingLongShortSwapped = entry.IsLongShortSwapped;
            if (!string.IsNullOrWhiteSpace(item.PackingBoxId))
                _assignedPackingBoxIds.Add(item.PackingBoxId);
        }

        private void ReassignPackingSequencesToScannedItems()
        {
            for (int i = 0; i < _scannedItems.Count; i++)
                AssignPackingSequenceToScannedItem(_scannedItems[i]);
        }

        private void ClearPackingOrderAssignments()
        {
            _packingOrderBySku.Clear();
            _packingOrderByBarcode.Clear();
            _assignedPackingBoxIds.Clear();

            for (int i = 0; i < _scannedItems.Count; i++)
            {
                _scannedItems[i].PackingSequence = 0;
                _scannedItems[i].PackingBoxId = string.Empty;
                _scannedItems[i].IsPackingLongShortSwapped = false;
            }

            RefreshScanInfoList();
        }

        private static List<Placement> GetItemPlacementsInPackingOrder(Container container)
        {
            if (container?.Stack?.Placements == null)
                return new List<Placement>();

            var orderedPlacements = new List<Placement>(container.Stack.Placements);
            orderedPlacements.Sort((a, b) =>
            {
                int c = a.Z.CompareTo(b.Z);
                if (c != 0) return c;

                bool aPad = a.IsPadding;
                bool bPad = b.IsPadding;
                if (aPad != bPad) return aPad ? 1 : -1;

                c = a.X.CompareTo(b.X);
                if (c != 0) return c;
                return a.Y.CompareTo(b.Y);
            });

            var items = new List<Placement>();
            foreach (var placement in orderedPlacements)
            {
                if (placement != null && !placement.IsPadding && placement.StackValue != null)
                    items.Add(placement);
            }
            return items;
        }

        private void RefreshPackingPositionsList()
        {
            if (lvPackingPositions == null)
                return;

            lvPackingPositions.BeginUpdate();
            lvPackingPositions.Items.Clear();
            if (lvPackingPoints != null)
            {
                lvPackingPoints.BeginUpdate();
                lvPackingPoints.Items.Clear();
            }
            _packingPointStrings.Clear();
            _packingOrderBySku.Clear();
            _packingOrderByBarcode.Clear();
            _assignedPackingBoxIds.Clear();
            UpdateSendStatus(null);
            var packingResultsForScanner = new List<ScannedItemInfo>();

            int sequence = 0;
            if (_packedContainers != null)
            {
                foreach (var container in _packedContainers)
                {
                    if (container == null)
                        continue;

                    string containerName = container.Description ?? "容器";
                    foreach (var placement in GetItemPlacementsInPackingOrder(container))
                    {
                        sequence++;
                        string itemName = placement.StackValue.Box?.Id ?? "?";
                        PackingSourceInfo source;
                        if (_packingSourceByBoxId.TryGetValue(itemName, out source))
                        {
                            var entry = new PackingOrderEntry
                            {
                                Sequence = sequence,
                                BoxId = itemName,
                                ContainerName = containerName,
                                IsLongShortSwapped = IsLongShortSwapped(source, placement)
                            };
                            EnqueuePackingOrder(_packingOrderBySku, source.Sku, entry);
                            EnqueuePackingOrder(_packingOrderByBarcode, source.Barcode, entry);
                            packingResultsForScanner.Add(new ScannedItemInfo
                            {
                                Sku = source.Sku ?? string.Empty,
                                Barcode = source.Barcode ?? string.Empty,
                                PackingSequence = sequence,
                                PackingBoxId = itemName,
                                IsPackingLongShortSwapped = entry.IsLongShortSwapped
                            });
                        }
                        string armCoordinate = FormatArmCoordinate(placement);
                        string armDropCoordinate = FormatArmDropCoordinate(placement);

                        var item = new ListViewItem(sequence.ToString());
                        item.SubItems.Add(containerName);
                        item.SubItems.Add(itemName);
                        item.SubItems.Add(FormatTopCenterPoint(placement));
                        lvPackingPositions.Items.Add(item);

                        _packingPointStrings.Add(armDropCoordinate);
                        if (lvPackingPoints != null)
                        {
                            var pointItem = new ListViewItem(sequence.ToString());
                            pointItem.SubItems.Add(itemName);
                            pointItem.SubItems.Add(armCoordinate);
                            pointItem.SubItems.Add(armDropCoordinate);
                            lvPackingPoints.Items.Add(pointItem);
                        }
                    }
                }
            }

            lvPackingPositions.EndUpdate();
            if (lvPackingPoints != null)
                lvPackingPoints.EndUpdate();

            if (lblPackingPositionsInfo != null)
            {
                lblPackingPositionsInfo.Text = sequence > 0
                    ? $"共 {sequence} 个物体（按装箱顺序，不含牛皮纸）"
                    : "暂无装箱结果";
            }

            if (lblPackingPointsInfo != null)
            {
                lblPackingPointsInfo.Text = sequence > 0
                    ? $"共 {sequence} 个点位（发送格式 [X,Y,Z,0,0,0],[X,Y,Z,0,0,0]，单位：米）"
                    : "暂无装箱点位";
            }

            ReassignPackingSequencesToScannedItems();
            RefreshScanInfoList();
            _scanTestControl?.UpdateOrderPackingResults(packingResultsForScanner);
            LayoutActualPackingPanel();
            UpdateArmSendButtonState();
            if (_armClient != null && _armClient.IsConnected && sequence > 0)
                AppendLog("[机械臂] 装箱点位已更新，点击「发送坐标」按顺序发送全部下放点位。");
        }

        private void AddDefaultContainer()
        {
            dgvContainers.Rows.Add("最大容器", "450", "450", "210", "0", "28000");
            dgvContainers.Rows.Add("常用容器", "260", "260", "260", "0", "28000");
            dgvContainers.Rows.Add("中间容器1", "310", "220", "160", "0", "28000");
            dgvContainers.Rows.Add("中间容器2", "310", "265", "220", "0", "28000");
            dgvContainers.Rows.Add("中间容器3", "360", "360", "360", "0", "28000");
            dgvContainers.Rows.Add("最小容器", "160", "120", "185", "0", "28000");
        }

        private void BtnAddContainer_Click(object sender, EventArgs e)
        {
            dgvContainers.Rows.Add("新容器", "300", "200", "150", "0", "28000");
            _selectionDirty = true;
            _lastRunState = null;
        }

        private void BtnRemoveContainer_Click(object sender, EventArgs e)
        {
            if (dgvContainers.SelectedRows.Count > 0)
            {
                foreach (DataGridViewRow row in dgvContainers.SelectedRows)
                {
                    if (!row.IsNewRow)
                        dgvContainers.Rows.Remove(row);
                }
            }
            else if (dgvContainers.CurrentRow != null && !dgvContainers.CurrentRow.IsNewRow)
            {
                dgvContainers.Rows.Remove(dgvContainers.CurrentRow);
            }
            _selectionDirty = true;
            _lastRunState = null;
        }

        #region Menu Handlers

        private void MenuOpenExcel_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "选择物品Excel文件";
                dlg.Filter = "Excel 文件|*.xlsx;*.xls|所有文件|*.*";
                if (dlg.ShowDialog() != DialogResult.OK) return;

                try
                {
                    _allLoadedItems = ExcelProbabilityReader.ReadItems(dlg.FileName);
                    // Assign unique instance IDs
                    for (int i = 0; i < _allLoadedItems.Count; i++)
                    {
                        var old = _allLoadedItems[i];
                        _allLoadedItems[i] = new ItemCandidate(
                            old.Name, old.Dx, old.Dy, old.Dz, i + 1, old.Probability, old.IsHighlighted);
                    }

                    // 默认加载所有物品
                    _loadedItems = new List<ItemCandidate>(_allLoadedItems);
                    _packingSourceByBoxId.Clear();
                    ClearPackingOrderAssignments();

                    RefreshItemsGrid();
                    SetStartPackingEnabled(_loadedItems.Count > 0);
                    btnRandomSelect.Enabled = _allLoadedItems.Count > 0;
                    if (btnInstanceSelect != null)
                        btnInstanceSelect.Enabled = _allLoadedItems.Count > 0;
                    if (btnProbabilitySelect != null)
                        btnProbabilitySelect.Enabled = _allLoadedItems.Count > 0;
                    lblRandomInfo.Text = $"已加载 {_allLoadedItems.Count} 个物品";
                    statusLabel.Text = $"已加载 {_allLoadedItems.Count} 个物品: {dlg.FileName}";
                    _selectionDirty = true;
                    _lastRunState = null;
                    AppendLog($"加载 {_allLoadedItems.Count} 个物品自 {dlg.FileName}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("读取Excel失败:\n" + ex.Message, "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void MenuExportJson_Click(object sender, EventArgs e)
        {
            if (_packedContainers.Count == 0)
            {
                MessageBox.Show("没有可导出的装箱结果。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var dlg = new SaveFileDialog())
            {
                dlg.Title = "导出JSON结果";
                dlg.Filter = "JSON 文件|*.json|所有文件|*.*";
                dlg.FileName = "packing_result.json";
                if (dlg.ShowDialog() != DialogResult.OK) return;

                try
                {
                    ResultSerializer.SerializeToFile(_packedContainers, dlg.FileName);
                    statusLabel.Text = "已导出: " + dlg.FileName;
                    AppendLog("导出JSON到 " + dlg.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("导出失败:\n" + ex.Message, "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnRandomSelect_Click(object sender, EventArgs e)
        {
            if (_allLoadedItems.Count == 0)
            {
                MessageBox.Show("请先加载物品Excel文件。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int minCount = (int)numRandomMin.Value;
            int maxCount = (int)numRandomMax.Value;

            if (minCount > maxCount)
            {
                MessageBox.Show("最小数量不能大于最大数量。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 生成随机数量
            var random = new Random();
            int targetCount = random.Next(minCount, maxCount + 1);
            // 有放回：允许目标数量超过物品种类数

            // 随机选择物品（有放回抽样：允许重复）
            _loadedItems = new List<ItemCandidate>(targetCount);
            for (int i = 0; i < targetCount; i++)
            {
                int idx = random.Next(_allLoadedItems.Count);
                _loadedItems.Add(_allLoadedItems[idx]);
            }

            // 重新分配InstanceId
            for (int i = 0; i < _loadedItems.Count; i++)
            {
                var old = _loadedItems[i];
                _loadedItems[i] = new ItemCandidate(
                    old.Name, old.Dx, old.Dy, old.Dz, i + 1, old.Probability, old.IsHighlighted);
            }

            _packingSourceByBoxId.Clear();
            ClearPackingOrderAssignments();
            RefreshItemsGrid();
            SetStartPackingEnabled(_loadedItems.Count > 0);
            statusLabel.Text = $"已随机选择 {_loadedItems.Count} 个物品 (范围: {minCount}-{maxCount})";
            _selectionDirty = true;
            _lastRunState = null;
            AppendLog($"随机选择了 {_loadedItems.Count} 个物品进行装箱");
        }

        private void BtnInstanceSelect_Click(object sender, EventArgs e)
        {
            if (_allLoadedItems.Count == 0)
            {
                MessageBox.Show("请先加载物品Excel文件。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int minCount = (int)numRandomMin.Value;
            int maxCount = (int)numRandomMax.Value;

            if (minCount > maxCount)
            {
                MessageBox.Show("最小数量不能大于最大数量。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var highlightedItems = _allLoadedItems.Where(i => i.IsHighlighted).ToList();
            if (highlightedItems.Count == 0)
            {
                MessageBox.Show("未检测到黄线物体，无法执行实例选择。请在Excel中将至少一行标黄。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var random = new Random();
            int targetCount = random.Next(minCount, maxCount + 1);
            _loadedItems = new List<ItemCandidate>(targetCount);

            // 第一个物体必须来自黄线物体集合
            if (targetCount > 0)
            {
                int highlightedIdx = random.Next(highlightedItems.Count);
                _loadedItems.Add(highlightedItems[highlightedIdx]);
            }

            // 其余物体从全量物体中有放回抽样（可重复）
            for (int i = _loadedItems.Count; i < targetCount; i++)
            {
                int idx = random.Next(_allLoadedItems.Count);
                _loadedItems.Add(_allLoadedItems[idx]);
            }

            for (int i = 0; i < _loadedItems.Count; i++)
            {
                var old = _loadedItems[i];
                _loadedItems[i] = new ItemCandidate(
                    old.Name, old.Dx, old.Dy, old.Dz, i + 1, old.Probability, old.IsHighlighted);
            }

            _packingSourceByBoxId.Clear();
            ClearPackingOrderAssignments();
            RefreshItemsGrid();
            SetStartPackingEnabled(_loadedItems.Count > 0);
            statusLabel.Text = $"已实例选择 {_loadedItems.Count} 个物品 (先黄线1个, 范围: {minCount}-{maxCount})";
            _selectionDirty = true;
            _lastRunState = null;
            AppendLog($"实例选择了 {_loadedItems.Count} 个物品（首个来自黄线物体，其余有放回随机）");
        }

        private void BtnProbabilitySelect_Click(object sender, EventArgs e)
        {
            if (_allLoadedItems.Count == 0)
            {
                MessageBox.Show("请先加载物品Excel文件。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int minCount = (int)numRandomMin.Value;
            int maxCount = (int)numRandomMax.Value;

            if (minCount > maxCount)
            {
                MessageBox.Show("最小数量不能大于最大数量。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 生成随机数量（与随机选择逻辑一致）
            var random = new Random();
            int targetCount = random.Next(minCount, maxCount + 1);
            // 有放回：允许目标数量超过物品种类数

            // 按权重（出现概率）有放回抽样：允许重复
            _loadedItems = SelectItemsByProbabilityWithReplacement(_allLoadedItems, targetCount, random);

            // 重新分配 InstanceId
            for (int i = 0; i < _loadedItems.Count; i++)
            {
                var old = _loadedItems[i];
                _loadedItems[i] = new ItemCandidate(
                    old.Name, old.Dx, old.Dy, old.Dz, i + 1, old.Probability, old.IsHighlighted);
            }

            _packingSourceByBoxId.Clear();
            ClearPackingOrderAssignments();
            RefreshItemsGrid();
            SetStartPackingEnabled(_loadedItems.Count > 0);
            statusLabel.Text = $"已按概率选择 {_loadedItems.Count} 个物品 (范围: {minCount}-{maxCount})";
            _selectionDirty = true;
            _lastRunState = null;
            AppendLog($"概率选择了 {_loadedItems.Count} 个物品进行装箱");
        }

        private List<ItemCandidate> SelectItemsByProbabilityWithReplacement(
            List<ItemCandidate> allItems,
            int targetCount,
            Random random)
        {
            var selected = new List<ItemCandidate>(targetCount);

            // 只把非负概率当作权重
            double sum = 0;
            foreach (var item in allItems)
                sum += Math.Max(0, item.Probability);

            for (int k = 0; k < targetCount; k++)
            {
                if (allItems.Count == 0)
                    break;

                if (sum <= 0)
                {
                    // 如果全部权重为 0，则退化为均匀抽样
                    int idx = random.Next(allItems.Count);
                    selected.Add(allItems[idx]);
                    continue;
                }

                double roll = random.NextDouble() * sum;
                double cumulative = 0;

                // 有放回抽样：每次都在全体内按权重选一次，不移除
                for (int i = 0; i < allItems.Count; i++)
                {
                    double w = Math.Max(0, allItems[i].Probability);
                    cumulative += w;
                    if (roll <= cumulative)
                    {
                        selected.Add(allItems[i]);
                        break;
                    }
                }

                // 防御：浮点误差导致没有命中时，兜底选择最后一个
                if (selected.Count < k + 1)
                {
                    selected.Add(allItems[allItems.Count - 1]);
                }
            }

            return selected;
        }

        private void ShuffleList<T>(List<T> list, Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                var temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        private void TryStartPackingAutomatically(string source)
        {
            if (_loadedItems.Count == 0)
            {
                AppendLog($"[{source}] 待装列表为空，跳过自动装箱。");
                return;
            }

            if (ReadContainerCandidates().Count == 0)
            {
                AppendLog($"[{source}] 未配置容器候选，跳过自动装箱。");
                return;
            }

            if (!menuStartPacking.Enabled && !btnStartPacking.Enabled)
            {
                AppendLog($"[{source}] 装箱正在进行中，跳过重复触发。");
                return;
            }

            AppendLog($"[{source}] 开始自动运行装箱算法。");
            MenuStartPacking_Click(this, EventArgs.Empty);
        }

        private void MenuStartPacking_Click(object sender, EventArgs e)
        {
            SyncPaddingSettingsFromUi();

            if (_loadedItems.Count == 0)
            {
                MessageBox.Show("请先加载物品 Excel，或完成批次扫码后同步到待装列表。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var containerCandidates = ReadContainerCandidates();
            if (containerCandidates.Count == 0)
            {
                MessageBox.Show("请至少添加一个容器候选。", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Disable UI during packing
            SetStartPackingEnabled(false);
            SetOpenExcelEnabled(false);
            statusLabel.Text = "正在装箱，请稍候...";
            AppendLog("========== 开始装箱 ==========");

            var itemsCopy = new List<ItemCandidate>(_loadedItems);
            var sw = Stopwatch.StartNew();
            long packSeed = (!_selectionDirty && _lastRunState != null)
                ? _lastRunState.RandomSeed
                : DateTime.Now.Ticks;

            var worker = new BackgroundWorker();
            worker.DoWork += (ws, we) =>
            {
                var orchestrator = new PackingOrchestrator();
                var packingOptions = new PackingOptions
                {
                    PaddingPaperStrategy = _paddingPaperFillStrategy,
                    PaddingPaperMinWidth = _paddingPaperMinWidth,
                    ContainerSafetyDistance = _containerSafetyDistance,
                    ItemSafetyDistance = _itemSafetyDistance,
                    PaddingPaperSafetyDistance = _paddingPaperSafetyDistance
                };
                we.Result = orchestrator.Run(itemsCopy, containerCandidates, packSeed,
                    msg => BeginInvoke((Action)(() => AppendLog(msg))),
                    _paddingPaperFillStrategy,
                    packingOptions);
            };
            worker.RunWorkerCompleted += (ws, we) =>
            {
                sw.Stop();
                SetStartPackingEnabled(true);
                SetOpenExcelEnabled(true);

                if (we.Error != null)
                {
                    statusLabel.Text = "装箱失败";
                    MessageBox.Show("装箱出错:\n" + we.Error.Message, "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _packedContainers = (List<Container>)we.Result;
                menuExportJson.Enabled = _packedContainers.Count > 0;

                // 只有成功生成结果后，才保存“可复现状态”
                if (_packedContainers != null && _packedContainers.Count > 0)
                {
                    SaveLastRunState(packSeed, containerCandidates);
                    _selectionDirty = false;
                    AppendLog($"[保存] 复现状态已保存（seed={packSeed}，策略={_paddingPaperFillStrategy}）。");
                }

                // 自动导出到unity文件夹（根目录下）
                if (_packedContainers.Count > 0)
                {
                    try
                    {
                        // 获取解决方案根目录（.sln文件所在目录）
                        string solutionDir = GetSolutionDirectory();
                        string unityFolder = Path.Combine(solutionDir, "unity");
                        
                        if (!Directory.Exists(unityFolder))
                        {
                            Directory.CreateDirectory(unityFolder);
                        }
                        
                        string jsonPath = Path.Combine(unityFolder, "packing_result.json");
                        ResultSerializer.SerializeToFile(_packedContainers, jsonPath);
                        AppendLog($"[Unity导出] 已自动生成: {jsonPath}");
                    }
                    catch (Exception ex)
                    {
                        AppendLog($"[Unity导出] 失败: {ex.Message}");
                    }
                }

                // Populate results list
                lstResults.Items.Clear();
                int totalPacked = 0;
                long totalVolPacked = 0;
                long totalContainerVol = 0;
                for (int i = 0; i < _packedContainers.Count; i++)
                {
                    var c = _packedContainers[i];
                    int count = c.Stack?.Size ?? 0;
                    totalPacked += count;
                    long cv = c.MaxLoadVolume;
                    long pv = c.Stack?.GetVolume() ?? 0;
                    totalVolPacked += pv;
                    totalContainerVol += cv;
                    double util = cv > 0 ? (double)pv / cv * 100 : 0;
                    lstResults.Items.Add($"[{i + 1}] {c.Description} - {count}件 - {util:F1}%");
                }

                double totalUtil = totalContainerVol > 0
                    ? (double)totalVolPacked / totalContainerVol * 100 : 0;

                statusLabel.Text = $"装箱完成: {_packedContainers.Count} 个容器, {totalPacked} 件物品";
                statusUtilization.Text = $"体积利用率: {totalUtil:F1}%";
                statusTime.Text = $"耗时: {sw.ElapsedMilliseconds}ms";
                AppendLog($"装箱完成! 容器数: {_packedContainers.Count}, 物品数: {totalPacked}, 耗时: {sw.ElapsedMilliseconds}ms");

                RefreshPackingPositionsList();

                if (lstResults.Items.Count > 0)
                {
                    lstResults.SelectedIndex = 0;
                }
                else
                {
                    ClearPackingResults();
                    MessageBox.Show(
                        "装箱未生成任何结果。常见原因：待装物品尺寸大于容器，或尺寸数据无效。请检查待装物品与容器尺寸（单位：mm）。",
                        "装箱结果为空", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };
            worker.RunWorkerAsync();
        }

        private void SyncPaddingSettingsFromUi()
        {
            if (numPaddingMinWidth == null || numContainerSafetyDistance == null ||
                numItemSafetyDistance == null || numPaddingPaperSafetyDistance == null)
                return;

            _paddingPaperMinWidth = SyncNumericValue(numPaddingMinWidth, _paddingPaperMinWidth);
            _containerSafetyDistance = SyncNumericValue(numContainerSafetyDistance, _containerSafetyDistance);
            _itemSafetyDistance = SyncNumericValue(numItemSafetyDistance, _itemSafetyDistance);
            _paddingPaperSafetyDistance = SyncNumericValue(numPaddingPaperSafetyDistance, _paddingPaperSafetyDistance);
        }

        private static int SyncNumericValue(NumericUpDown control, int fallback)
        {
            int min = (int)control.Minimum;
            int max = (int)control.Maximum;
            if (int.TryParse(control.Text?.Trim(), out int typedValue))
            {
                typedValue = Math.Max(min, Math.Min(max, typedValue));
                control.Value = typedValue;
                return typedValue;
            }
            int currentValue = (int)control.Value;
            if (currentValue < min || currentValue > max)
                return Math.Max(min, Math.Min(max, fallback));
            return currentValue;
        }

        #endregion

        #region Data Helpers

        private void RefreshItemsGrid()
        {
            dgvItems.Rows.Clear();
            foreach (var item in _loadedItems)
            {
                dgvItems.Rows.Add(item.Name, item.Dx, item.Dy, item.Dz);
            }
            lblItems.Text = $"待装物品 ({_loadedItems.Count})：";
        }

        private List<ContainerCandidate> ReadContainerCandidates()
        {
            var list = new List<ContainerCandidate>();
            foreach (DataGridViewRow row in dgvContainers.Rows)
            {
                if (row.IsNewRow) continue;

                string name = NormalizeContainerName((row.Cells[0].Value ?? "").ToString().Trim());
                if (string.IsNullOrEmpty(name)) continue;

                if (!TryParseInt(row.Cells[1].Value, out int dx) ||
                    !TryParseInt(row.Cells[2].Value, out int dy) ||
                    !TryParseInt(row.Cells[3].Value, out int dz))
                    continue;

                TryParseInt(row.Cells[4].Value, out int weight);
                TryParseInt(row.Cells[5].Value, out int maxLoad);
                if (maxLoad <= 0) maxLoad = int.MaxValue;

                list.Add(new ContainerCandidate(name, dx, dy, dz, weight, maxLoad));
            }
            return list;
        }

        private bool TryParseInt(object value, out int result)
        {
            result = 0;
            if (value == null) return false;
            string s = value.ToString().Trim();
            if (string.IsNullOrEmpty(s)) return false;
            return int.TryParse(s, out result) && result > 0;
        }

        private void AppendLog(string msg)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => AppendLog(msg)));
                return;
            }
            txtLog.AppendText(msg + Environment.NewLine);
        }

        #endregion

        #region Results & Step

        private void LstResults_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = lstResults.SelectedIndex;
            if (idx < 0 || idx >= _packedContainers.Count)
            {
                _renderer.Container = null;
                _renderer.Containers = new List<Container>();
                trackStep.Maximum = 0;
                trackStep.Value = 0;
                lblStepInfo.Text = "0 / 0";
                glControl.Invalidate();
                return;
            }

            // Set all containers for side-by-side rendering
            _renderer.Containers = _packedContainers;
            
            var container = _packedContainers[idx];
            _renderer.Container = container;
            _renderer.SelectedPlacement = null;
            lblSelectedInfo.Text = "无";

            // 步骤控制语义：每个 placement（物体或牛皮纸）都算一个步骤
            // 因此滑动条最大值取所有容器 placements 总数的最大值。
            int maxPlacementCount = 0;
            foreach (var c in _packedContainers)
            {
                int count = c.Stack?.Placements?.Count ?? 0;
                maxPlacementCount = Math.Max(maxPlacementCount, count);
            }

            int totalSteps = maxPlacementCount;
            trackStep.Maximum = totalSteps;
            trackStep.Value = totalSteps;           // 默认直接显示到最后一步（包含牛皮纸）
            _renderer.CurrentStep = totalSteps;
            lblStepInfo.Text = $"{totalSteps} / {totalSteps}";

            // Compute utilization for this container
            long cv = container.MaxLoadVolume;
            long pv = container.Stack?.GetVolume() ?? 0;
            double util = cv > 0 ? (double)pv / cv * 100 : 0;
            statusUtilization.Text = $"体积利用率: {util:F1}% ({pv}/{cv})";

            // Fit camera to scene - calculate total width of all containers
            float totalWidth = 0;
            float maxHeight = 0;
            float maxDepth = 0;
            foreach (var c in _packedContainers)
            {
                totalWidth += c.LoadDx + 150; // Include spacing
                maxHeight = Math.Max(maxHeight, c.LoadDz);
                maxDepth = Math.Max(maxDepth, c.LoadDy);
            }
            totalWidth -= 150; // Remove last spacing
            
            float sceneSize = Math.Max(totalWidth, Math.Max(maxHeight, maxDepth));
            _camera.FitToScene(sceneSize);

            glControl.Invalidate();
        }

        private void TrackStep_ValueChanged(object sender, EventArgs e)
        {
            int total = trackStep.Maximum;
            int val = trackStep.Value;
            _renderer.CurrentStep = val;
            // 0 表示显示全部（含牛皮纸）
            lblStepInfo.Text = val == 0 ? $"全部 / {total}" : $"{val} / {total}";
            
            // 无论单容器还是多容器模式，都需要重绘
            glControl.Invalidate();
        }

        #endregion

        #region GL Events

        private void GlControl_Load(object sender, EventArgs e)
        {
            _glInitialized = true;
            GL.ClearColor(0.1f, 0.1f, 0.14f, 1f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.LineSmooth);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);
            SetupViewport();
        }

        private void GlControl_Resize(object sender, EventArgs e)
        {
            if (!_glInitialized) return;
            SetupViewport();
            glControl.Invalidate();
        }

        private void SetupViewport()
        {
            int w = glControl.ClientSize.Width;
            int h = glControl.ClientSize.Height;
            if (w <= 0 || h <= 0) return;
            GL.Viewport(0, 0, w, h);
        }

        private void GlControl_Paint(object sender, PaintEventArgs e)
        {
            if (!_glInitialized) return;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            int w = glControl.ClientSize.Width;
            int h = glControl.ClientSize.Height;
            if (w <= 0 || h <= 0) return;

            float aspect = (float)w / h;
            var projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45f), aspect, 1f, 50000f);
            var view = _camera.GetViewMatrix();

            _renderer.Render(projection, view);

            glControl.SwapBuffers();
        }

        #endregion

        #region Mouse Controls

        private void GlControl_MouseDown(object sender, MouseEventArgs e)
        {
            _lastMouse = e.Location;
            if (e.Button == MouseButtons.Left)
                _mouseRotating = true;
            else if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle)
                _mousePanning = true;
        }

        private void GlControl_MouseUp(object sender, MouseEventArgs e)
        {
            _mouseRotating = false;
            _mousePanning = false;
        }

        private void GlControl_MouseMove(object sender, MouseEventArgs e)
        {
            float dx = e.X - _lastMouse.X;
            float dy = e.Y - _lastMouse.Y;
            _lastMouse = e.Location;

            if (_mouseRotating)
            {
                _camera.Rotate(-dx * 0.3f, -dy * 0.3f);
                glControl.Invalidate();
            }
            else if (_mousePanning)
            {
                _camera.Pan(-dx, dy);
                glControl.Invalidate();
            }
        }

        private void GlControl_MouseWheel(object sender, MouseEventArgs e)
        {
            float delta = e.Delta / 120f;
            _camera.Zoom(delta);
            glControl.Invalidate();
        }

        private void GlControl_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (_renderer.Container == null) return;

            // Unproject mouse to ray
            int w = glControl.ClientSize.Width;
            int h = glControl.ClientSize.Height;
            if (w <= 0 || h <= 0) return;

            float aspect = (float)w / h;
            var projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45f), aspect, 1f, 50000f);
            var view = _camera.GetViewMatrix();

            Vector3 rayOrigin, rayDir;
            Unproject(e.X, e.Y, w, h, projection, view, out rayOrigin, out rayDir);

            // 点击检测：maxStep 直接复用渲染当前的 step 语义
            // - CurrentStep=0 表示显示全部
            // - CurrentStep>0 表示按“Z升序（同Z padding优先）”显示前 maxStep 个 placement
            int maxStep = _renderer.CurrentStep;

            // Use multi-container hit testing if multiple containers are displayed
            Placement hit;
            if (_renderer.Containers.Count > 0)
            {
                Container hitContainer;
                hit = HitTester.FindHit(rayOrigin, rayDir, _renderer.Containers, maxStep, out hitContainer);
            }
            else
            {
                hit = HitTester.FindHit(rayOrigin, rayDir, _renderer.Container, maxStep);
            }

            _renderer.SelectedPlacement = hit;
            if (hit != null)
            {
                string itemName;
                if (hit.IsPadding)
                    itemName = "牛皮纸";
                else
                    itemName = hit.StackValue.Box?.Id ?? "?";
                lblSelectedInfo.Text =
                    $"{itemName}\n位置: ({hit.X},{hit.Y},{hit.Z}) 尺寸: {hit.StackValue.Dx}x{hit.StackValue.Dy}x{hit.StackValue.Dz} 顶部中心点: {FormatTopCenterPoint(hit)}";
            }
            else
            {
                lblSelectedInfo.Text = "无";
            }
            glControl.Invalidate();
        }

        private void Unproject(int mouseX, int mouseY, int viewW, int viewH,
            Matrix4 projection, Matrix4 view, out Vector3 rayOrigin, out Vector3 rayDir)
        {
            // Convert mouse coords to NDC
            float ndcX = (2f * mouseX / viewW) - 1f;
            float ndcY = 1f - (2f * mouseY / viewH);

            // Inverse matrices
            Matrix4 invProj = Matrix4.Invert(projection);
            Matrix4 invView = Matrix4.Invert(view);

            // Near and far points in clip space
            var nearClip = new Vector4(ndcX, ndcY, -1f, 1f);
            var farClip = new Vector4(ndcX, ndcY, 1f, 1f);

            // To eye space
            var nearEye = Vector4.Transform(nearClip, invProj);
            var farEye = Vector4.Transform(farClip, invProj);
            nearEye /= nearEye.W;
            farEye /= farEye.W;

            // To world space
            var nearWorld = Vector4.Transform(nearEye, invView);
            var farWorld = Vector4.Transform(farEye, invView);
            nearWorld /= nearWorld.W;
            farWorld /= farWorld.W;

            rayOrigin = nearWorld.Xyz;
            rayDir = Vector3.Normalize((farWorld - nearWorld).Xyz);
        }

        /// <summary>
        /// 获取解决方案根目录（.sln文件所在目录）
        /// </summary>
        private string GetSolutionDirectory()
        {
            // 从当前执行文件路径向上查找，直到找到.sln文件或到达根目录
            string currentDir = Path.GetDirectoryName(Application.ExecutablePath);
            
            // 尝试向上查找3层目录（通常在bin/Debug/net48下）
            for (int i = 0; i < 5; i++)
            {
                // 检查当前目录是否有.sln文件
                string[] slnFiles = Directory.GetFiles(currentDir, "*.sln");
                if (slnFiles.Length > 0)
                {
                    return currentDir;
                }
                
                // 检查是否有unity文件夹（作为备选判断）
                string unityFolder = Path.Combine(currentDir, "unity");
                if (Directory.Exists(unityFolder))
                {
                    // 检查unity文件夹中是否有PackingLoader.cs
                    string loaderFile = Path.Combine(unityFolder, "PackingLoader.cs");
                    if (File.Exists(loaderFile))
                    {
                        return currentDir;
                    }
                }
                
                // 向上移动一级
                string parentDir = Directory.GetParent(currentDir)?.FullName;
                if (parentDir == null)
                    break;
                currentDir = parentDir;
            }
            
            // 如果找不到，返回执行文件的上3级目录（默认情况）
            return Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(Application.ExecutablePath), "..", "..", ".."));
        }

        #endregion
    }
}
