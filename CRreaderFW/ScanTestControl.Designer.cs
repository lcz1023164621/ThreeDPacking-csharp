namespace WindowsFormsApp1
{
    partial class ScanTestControl
    {
        private System.ComponentModel.IContainer components = null;

        private void InitializeComponent()
        {
            this.panelTop = new System.Windows.Forms.Panel();
            this.grpWorkMode = new System.Windows.Forms.GroupBox();
            this.lblModeHint = new System.Windows.Forms.Label();
            this.rdoTestMode = new System.Windows.Forms.RadioButton();
            this.rdoProductionMode = new System.Windows.Forms.RadioButton();
            this.grpDeviceActions = new System.Windows.Forms.GroupBox();
            this.btnStopCurrentScan = new System.Windows.Forms.Button();
            this.btnStartTestScan = new System.Windows.Forms.Button();
            this.btnWaitRobotSignal = new System.Windows.Forms.Button();
            this.btnReaderSettings = new System.Windows.Forms.Button();
            this.btnConnectReader = new System.Windows.Forms.Button();
            this.grpCurrentOrder = new System.Windows.Forms.GroupBox();
            this.btnOpenOrderMatch = new System.Windows.Forms.Button();
            this.btnConfirmOrderBox = new System.Windows.Forms.Button();
            this.lblMatchedBoxCodeValue = new System.Windows.Forms.Label();
            this.lblMatchedBoxCodeTitle = new System.Windows.Forms.Label();
            this.lblMatchedOrderNoValue = new System.Windows.Forms.Label();
            this.lblMatchedOrderNoTitle = new System.Windows.Forms.Label();
            this.txtOrderBoxInput = new System.Windows.Forms.TextBox();
            this.lblOrderBoxInput = new System.Windows.Forms.Label();
            this.rdoInputByBoxCode = new System.Windows.Forms.RadioButton();
            this.rdoInputByOrderNo = new System.Windows.Forms.RadioButton();
            this.grpWorkState = new System.Windows.Forms.GroupBox();
            this.lblCamera3DStateValue = new System.Windows.Forms.Label();
            this.lblCamera3DStateTitle = new System.Windows.Forms.Label();
            this.lblRobotStateValue = new System.Windows.Forms.Label();
            this.lblRobotStateTitle = new System.Windows.Forms.Label();
            this.lblScanStateValue = new System.Windows.Forms.Label();
            this.lblScanStateTitle = new System.Windows.Forms.Label();
            this.lblReaderStateValue = new System.Windows.Forms.Label();
            this.lblReaderStateTitle = new System.Windows.Forms.Label();
            this.panelUpper = new System.Windows.Forms.Panel();
            this.grpAlarmSummary = new System.Windows.Forms.GroupBox();
            this.lblCurrentStateValue = new System.Windows.Forms.Label();
            this.lblCurrentStateTitle = new System.Windows.Forms.Label();
            this.lblTestTotalValue = new System.Windows.Forms.Label();
            this.lblTestTotalTitle = new System.Windows.Forms.Label();
            this.lblAbnormalTotalValue = new System.Windows.Forms.Label();
            this.lblAbnormalTotalTitle = new System.Windows.Forms.Label();
            this.lblActualTotalValue = new System.Windows.Forms.Label();
            this.lblActualTotalTitle = new System.Windows.Forms.Label();
            this.grpCurrentResult = new System.Windows.Forms.GroupBox();
            this.lblResultScanTimeValue = new System.Windows.Forms.Label();
            this.lblResultScanTimeTitle = new System.Windows.Forms.Label();
            this.lblResultScanCountValue = new System.Windows.Forms.Label();
            this.lblResultScanCountTitle = new System.Windows.Forms.Label();
            this.lblResultStatusValue = new System.Windows.Forms.Label();
            this.lblResultStatusTitle = new System.Windows.Forms.Label();
            this.lblResultSkuValue = new System.Windows.Forms.Label();
            this.lblResultSkuTitle = new System.Windows.Forms.Label();
            this.lblResultBarcodeValue = new System.Windows.Forms.Label();
            this.lblResultBarcodeTitle = new System.Windows.Forms.Label();
            this.lblResultSequenceValue = new System.Windows.Forms.Label();
            this.lblResultSequenceTitle = new System.Windows.Forms.Label();
            this.lblResultBatchBoxValue = new System.Windows.Forms.Label();
            this.lblResultBatchBoxTitle = new System.Windows.Forms.Label();
            this.lblResultOrderNoValue = new System.Windows.Forms.Label();
            this.lblResultOrderNoTitle = new System.Windows.Forms.Label();
            this.grpScanImage = new System.Windows.Forms.GroupBox();
            this.lblPhotoSourceValue = new System.Windows.Forms.Label();
            this.lblPhotoSourceTitle = new System.Windows.Forms.Label();
            this.lblPhotoSkuValue = new System.Windows.Forms.Label();
            this.lblPhotoSkuTitle = new System.Windows.Forms.Label();
            this.lblPhotoBarcodeValue = new System.Windows.Forms.Label();
            this.lblPhotoBarcodeTitle = new System.Windows.Forms.Label();
            this.picScanImage = new System.Windows.Forms.PictureBox();
            this.tabMain = new System.Windows.Forms.TabControl();
            this.tabCurrentOrder = new System.Windows.Forms.TabPage();
            this.splitCurrent = new System.Windows.Forms.SplitContainer();
            this.grpProductScanList = new System.Windows.Forms.GroupBox();
            this.dgvProductScanList = new System.Windows.Forms.DataGridView();
            this.colProductOrderNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colProductSequence = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colProductBarcode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colProductSku = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colProductStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colProductScanCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.grpScanMatchResult = new System.Windows.Forms.GroupBox();
            this.dgvScanMatchResult = new System.Windows.Forms.DataGridView();
            this.colMatchBarcode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMatchSku = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMatchOrderQuantity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMatchActualQuantity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMatchStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabHistory = new System.Windows.Forms.TabPage();
            this.splitHistory = new System.Windows.Forms.SplitContainer();
            this.grpHistoryScanList = new System.Windows.Forms.GroupBox();
            this.dgvHistoryScanList = new System.Windows.Forms.DataGridView();
            this.colHistoryOrderNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colHistorySequence = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colHistoryBarcode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colHistorySku = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colHistoryStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colHistoryScanCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.grpHistoryMatch = new System.Windows.Forms.GroupBox();
            this.dgvHistoryMatchResult = new System.Windows.Forms.DataGridView();
            this.colHistoryMatchBarcode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colHistoryMatchSku = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colHistoryMatchOrderQuantity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colHistoryMatchActualQuantity = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colHistoryMatchStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panelHistorySearch = new System.Windows.Forms.Panel();
            this.rdoSearchByOrderNo = new System.Windows.Forms.RadioButton();
            this.rdoSearchByBoxCode = new System.Windows.Forms.RadioButton();
            this.lblHistorySearchKey = new System.Windows.Forms.Label();
            this.txtHistorySearchKey = new System.Windows.Forms.TextBox();
            this.btnSearchHistory = new System.Windows.Forms.Button();
            this.lblHistoryResult = new System.Windows.Forms.Label();
            this.tabTestRecords = new System.Windows.Forms.TabPage();
            this.grpTestRecords = new System.Windows.Forms.GroupBox();
            this.dgvTestRecords = new System.Windows.Forms.DataGridView();
            this.colTestOrderNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTestSequence = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTestBarcode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTestSku = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTestStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTestScanCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabLog = new System.Windows.Forms.TabPage();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.statusStripMain = new System.Windows.Forms.StatusStrip();
            this.lblFooterStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.panelTop.SuspendLayout();
            this.grpWorkMode.SuspendLayout();
            this.grpDeviceActions.SuspendLayout();
            this.grpCurrentOrder.SuspendLayout();
            this.grpWorkState.SuspendLayout();
            this.panelUpper.SuspendLayout();
            this.grpAlarmSummary.SuspendLayout();
            this.grpCurrentResult.SuspendLayout();
            this.grpScanImage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picScanImage)).BeginInit();
            this.tabMain.SuspendLayout();
            this.tabCurrentOrder.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitCurrent)).BeginInit();
            this.splitCurrent.Panel1.SuspendLayout();
            this.splitCurrent.Panel2.SuspendLayout();
            this.splitCurrent.SuspendLayout();
            this.grpProductScanList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvProductScanList)).BeginInit();
            this.grpScanMatchResult.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvScanMatchResult)).BeginInit();
            this.tabHistory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitHistory)).BeginInit();
            this.splitHistory.Panel1.SuspendLayout();
            this.splitHistory.Panel2.SuspendLayout();
            this.splitHistory.SuspendLayout();
            this.grpHistoryScanList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistoryScanList)).BeginInit();
            this.grpHistoryMatch.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistoryMatchResult)).BeginInit();
            this.panelHistorySearch.SuspendLayout();
            this.tabTestRecords.SuspendLayout();
            this.grpTestRecords.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvTestRecords)).BeginInit();
            this.tabLog.SuspendLayout();
            this.statusStripMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.grpWorkMode);
            this.panelTop.Controls.Add(this.grpDeviceActions);
            this.panelTop.Controls.Add(this.grpCurrentOrder);
            this.panelTop.Controls.Add(this.grpWorkState);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panelTop.Name = "panelTop";
            this.panelTop.Padding = new System.Windows.Forms.Padding(8, 8, 8, 8);
            this.panelTop.Size = new System.Drawing.Size(960, 120);
            this.panelTop.TabIndex = 0;
            // 
            // grpWorkMode
            // 
            this.grpWorkMode.Controls.Add(this.lblModeHint);
            this.grpWorkMode.Controls.Add(this.rdoTestMode);
            this.grpWorkMode.Controls.Add(this.rdoProductionMode);
            this.grpWorkMode.Dock = System.Windows.Forms.DockStyle.Right;
            this.grpWorkMode.Location = new System.Drawing.Point(578, 8);
            this.grpWorkMode.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpWorkMode.Name = "grpWorkMode";
            this.grpWorkMode.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpWorkMode.Size = new System.Drawing.Size(142, 104);
            this.grpWorkMode.TabIndex = 3;
            this.grpWorkMode.TabStop = false;
            this.grpWorkMode.Text = "运行模式";
            // 
            // lblModeHint
            // 
            this.lblModeHint.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblModeHint.Location = new System.Drawing.Point(9, 64);
            this.lblModeHint.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblModeHint.Name = "lblModeHint";
            this.lblModeHint.Size = new System.Drawing.Size(128, 35);
            this.lblModeHint.TabIndex = 2;
            this.lblModeHint.Text = "工作模式：确认订单编号/箱码编号后等待机械臂到位。";
            // 
            // rdoTestMode
            // 
            this.rdoTestMode.AutoSize = true;
            this.rdoTestMode.Location = new System.Drawing.Point(11, 42);
            this.rdoTestMode.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rdoTestMode.Name = "rdoTestMode";
            this.rdoTestMode.Size = new System.Drawing.Size(71, 16);
            this.rdoTestMode.TabIndex = 1;
            this.rdoTestMode.Text = "测试模式";
            this.rdoTestMode.UseVisualStyleBackColor = true;
            this.rdoTestMode.CheckedChanged += new System.EventHandler(this.rdoTestMode_CheckedChanged);
            // 
            // rdoProductionMode
            // 
            this.rdoProductionMode.AutoSize = true;
            this.rdoProductionMode.Checked = true;
            this.rdoProductionMode.Location = new System.Drawing.Point(11, 20);
            this.rdoProductionMode.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rdoProductionMode.Name = "rdoProductionMode";
            this.rdoProductionMode.Size = new System.Drawing.Size(71, 16);
            this.rdoProductionMode.TabIndex = 0;
            this.rdoProductionMode.TabStop = true;
            this.rdoProductionMode.Text = "工作模式";
            this.rdoProductionMode.UseVisualStyleBackColor = true;
            this.rdoProductionMode.CheckedChanged += new System.EventHandler(this.rdoProductionMode_CheckedChanged);
            // 
            // grpDeviceActions
            // 
            this.grpDeviceActions.Controls.Add(this.btnStopCurrentScan);
            this.grpDeviceActions.Controls.Add(this.btnStartTestScan);
            this.grpDeviceActions.Controls.Add(this.btnWaitRobotSignal);
            this.grpDeviceActions.Controls.Add(this.btnReaderSettings);
            this.grpDeviceActions.Controls.Add(this.btnConnectReader);
            this.grpDeviceActions.Dock = System.Windows.Forms.DockStyle.Right;
            this.grpDeviceActions.Location = new System.Drawing.Point(720, 8);
            this.grpDeviceActions.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpDeviceActions.Name = "grpDeviceActions";
            this.grpDeviceActions.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpDeviceActions.Size = new System.Drawing.Size(232, 118);
            this.grpDeviceActions.TabIndex = 2;
            this.grpDeviceActions.TabStop = false;
            this.grpDeviceActions.Text = "扫码器与动作";
            // 
            // btnStopCurrentScan
            // 
            this.btnStopCurrentScan.Enabled = false;
            this.btnStopCurrentScan.Location = new System.Drawing.Point(11, 86);
            this.btnStopCurrentScan.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnStopCurrentScan.Name = "btnStopCurrentScan";
            this.btnStopCurrentScan.Size = new System.Drawing.Size(207, 29);
            this.btnStopCurrentScan.TabIndex = 4;
            this.btnStopCurrentScan.Text = "停止当前扫码";
            this.btnStopCurrentScan.UseVisualStyleBackColor = true;
            this.btnStopCurrentScan.Click += new System.EventHandler(this.btnStopCurrentScan_Click);
            // 
            // btnStartTestScan
            // 
            this.btnStartTestScan.Location = new System.Drawing.Point(119, 53);
            this.btnStartTestScan.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnStartTestScan.Name = "btnStartTestScan";
            this.btnStartTestScan.Size = new System.Drawing.Size(99, 29);
            this.btnStartTestScan.TabIndex = 3;
            this.btnStartTestScan.Text = "开始测试扫码";
            this.btnStartTestScan.UseVisualStyleBackColor = true;
            this.btnStartTestScan.Click += new System.EventHandler(this.btnStartTestScan_Click);
            // 
            // btnWaitRobotSignal
            // 
            this.btnWaitRobotSignal.Location = new System.Drawing.Point(11, 53);
            this.btnWaitRobotSignal.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnWaitRobotSignal.Name = "btnWaitRobotSignal";
            this.btnWaitRobotSignal.Size = new System.Drawing.Size(99, 29);
            this.btnWaitRobotSignal.TabIndex = 2;
            this.btnWaitRobotSignal.Text = "机械臂到位信号";
            this.btnWaitRobotSignal.UseVisualStyleBackColor = true;
            this.btnWaitRobotSignal.Click += new System.EventHandler(this.btnWaitRobotSignal_Click);
            // 
            // btnReaderSettings
            // 
            this.btnReaderSettings.Location = new System.Drawing.Point(119, 20);
            this.btnReaderSettings.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnReaderSettings.Name = "btnReaderSettings";
            this.btnReaderSettings.Size = new System.Drawing.Size(99, 26);
            this.btnReaderSettings.TabIndex = 1;
            this.btnReaderSettings.Text = "扫码器设置";
            this.btnReaderSettings.UseVisualStyleBackColor = true;
            this.btnReaderSettings.Click += new System.EventHandler(this.btnReaderSettings_Click);
            // 
            // btnConnectReader
            // 
            this.btnConnectReader.Location = new System.Drawing.Point(11, 20);
            this.btnConnectReader.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnConnectReader.Name = "btnConnectReader";
            this.btnConnectReader.Size = new System.Drawing.Size(99, 29);
            this.btnConnectReader.TabIndex = 0;
            this.btnConnectReader.Text = "连接扫码器";
            this.btnConnectReader.UseVisualStyleBackColor = true;
            this.btnConnectReader.Click += new System.EventHandler(this.btnConnectReader_Click);
            // 
            // grpCurrentOrder
            // 
            this.grpCurrentOrder.Controls.Add(this.btnOpenOrderMatch);
            this.grpCurrentOrder.Controls.Add(this.btnConfirmOrderBox);
            this.grpCurrentOrder.Controls.Add(this.lblMatchedBoxCodeValue);
            this.grpCurrentOrder.Controls.Add(this.lblMatchedBoxCodeTitle);
            this.grpCurrentOrder.Controls.Add(this.lblMatchedOrderNoValue);
            this.grpCurrentOrder.Controls.Add(this.lblMatchedOrderNoTitle);
            this.grpCurrentOrder.Controls.Add(this.txtOrderBoxInput);
            this.grpCurrentOrder.Controls.Add(this.lblOrderBoxInput);
            this.grpCurrentOrder.Controls.Add(this.rdoInputByBoxCode);
            this.grpCurrentOrder.Controls.Add(this.rdoInputByOrderNo);
            this.grpCurrentOrder.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpCurrentOrder.Location = new System.Drawing.Point(8, 8);
            this.grpCurrentOrder.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpCurrentOrder.Name = "grpCurrentOrder";
            this.grpCurrentOrder.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpCurrentOrder.Size = new System.Drawing.Size(345, 104);
            this.grpCurrentOrder.TabIndex = 0;
            this.grpCurrentOrder.TabStop = false;
            this.grpCurrentOrder.Text = "当前订单与箱码匹配";
            // 
            // btnOpenOrderMatch
            // 
            this.btnOpenOrderMatch.Location = new System.Drawing.Point(261, 20);
            this.btnOpenOrderMatch.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnOpenOrderMatch.Name = "btnOpenOrderMatch";
            this.btnOpenOrderMatch.Size = new System.Drawing.Size(82, 26);
            this.btnOpenOrderMatch.TabIndex = 4;
            this.btnOpenOrderMatch.Text = "匹配信息";
            this.btnOpenOrderMatch.UseVisualStyleBackColor = true;
            this.btnOpenOrderMatch.Click += new System.EventHandler(this.btnOpenOrderMatch_Click);
            // 
            // btnConfirmOrderBox
            // 
            this.btnConfirmOrderBox.Location = new System.Drawing.Point(261, 50);
            this.btnConfirmOrderBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnConfirmOrderBox.Name = "btnConfirmOrderBox";
            this.btnConfirmOrderBox.Size = new System.Drawing.Size(82, 26);
            this.btnConfirmOrderBox.TabIndex = 5;
            this.btnConfirmOrderBox.Text = "确认订单/箱码";
            this.btnConfirmOrderBox.UseVisualStyleBackColor = true;
            this.btnConfirmOrderBox.Click += new System.EventHandler(this.btnConfirmOrderBox_Click);
            // 
            // lblMatchedBoxCodeValue
            // 
            this.lblMatchedBoxCodeValue.BackColor = System.Drawing.Color.WhiteSmoke;
            this.lblMatchedBoxCodeValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblMatchedBoxCodeValue.Location = new System.Drawing.Point(71, 83);
            this.lblMatchedBoxCodeValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblMatchedBoxCodeValue.Name = "lblMatchedBoxCodeValue";
            this.lblMatchedBoxCodeValue.Size = new System.Drawing.Size(169, 18);
            this.lblMatchedBoxCodeValue.TabIndex = 9;
            this.lblMatchedBoxCodeValue.Text = "-";
            this.lblMatchedBoxCodeValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMatchedBoxCodeTitle
            // 
            this.lblMatchedBoxCodeTitle.Location = new System.Drawing.Point(9, 85);
            this.lblMatchedBoxCodeTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblMatchedBoxCodeTitle.Name = "lblMatchedBoxCodeTitle";
            this.lblMatchedBoxCodeTitle.Size = new System.Drawing.Size(62, 14);
            this.lblMatchedBoxCodeTitle.TabIndex = 8;
            this.lblMatchedBoxCodeTitle.Text = "匹配箱码";
            // 
            // lblMatchedOrderNoValue
            // 
            this.lblMatchedOrderNoValue.BackColor = System.Drawing.Color.WhiteSmoke;
            this.lblMatchedOrderNoValue.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblMatchedOrderNoValue.Location = new System.Drawing.Point(71, 64);
            this.lblMatchedOrderNoValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblMatchedOrderNoValue.Name = "lblMatchedOrderNoValue";
            this.lblMatchedOrderNoValue.Size = new System.Drawing.Size(169, 18);
            this.lblMatchedOrderNoValue.TabIndex = 7;
            this.lblMatchedOrderNoValue.Text = "-";
            this.lblMatchedOrderNoValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMatchedOrderNoTitle
            // 
            this.lblMatchedOrderNoTitle.Location = new System.Drawing.Point(9, 66);
            this.lblMatchedOrderNoTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblMatchedOrderNoTitle.Name = "lblMatchedOrderNoTitle";
            this.lblMatchedOrderNoTitle.Size = new System.Drawing.Size(62, 14);
            this.lblMatchedOrderNoTitle.TabIndex = 6;
            this.lblMatchedOrderNoTitle.Text = "匹配订单";
            // 
            // txtOrderBoxInput
            // 
            this.txtOrderBoxInput.Location = new System.Drawing.Point(71, 42);
            this.txtOrderBoxInput.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtOrderBoxInput.Name = "txtOrderBoxInput";
            this.txtOrderBoxInput.Size = new System.Drawing.Size(170, 21);
            this.txtOrderBoxInput.TabIndex = 3;
            // 
            // lblOrderBoxInput
            // 
            this.lblOrderBoxInput.Location = new System.Drawing.Point(9, 43);
            this.lblOrderBoxInput.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblOrderBoxInput.Name = "lblOrderBoxInput";
            this.lblOrderBoxInput.Size = new System.Drawing.Size(62, 19);
            this.lblOrderBoxInput.TabIndex = 2;
            this.lblOrderBoxInput.Text = "输入编号";
            this.lblOrderBoxInput.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // rdoInputByBoxCode
            // 
            this.rdoInputByBoxCode.AutoSize = true;
            this.rdoInputByBoxCode.Location = new System.Drawing.Point(109, 20);
            this.rdoInputByBoxCode.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rdoInputByBoxCode.Name = "rdoInputByBoxCode";
            this.rdoInputByBoxCode.Size = new System.Drawing.Size(107, 16);
            this.rdoInputByBoxCode.TabIndex = 1;
            this.rdoInputByBoxCode.Text = "按箱码编号输入";
            this.rdoInputByBoxCode.UseVisualStyleBackColor = true;
            // 
            // rdoInputByOrderNo
            // 
            this.rdoInputByOrderNo.AutoSize = true;
            this.rdoInputByOrderNo.Checked = true;
            this.rdoInputByOrderNo.Location = new System.Drawing.Point(10, 20);
            this.rdoInputByOrderNo.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rdoInputByOrderNo.Name = "rdoInputByOrderNo";
            this.rdoInputByOrderNo.Size = new System.Drawing.Size(107, 16);
            this.rdoInputByOrderNo.TabIndex = 0;
            this.rdoInputByOrderNo.TabStop = true;
            this.rdoInputByOrderNo.Text = "按订单编号输入";
            this.rdoInputByOrderNo.UseVisualStyleBackColor = true;
            // 
            // grpWorkState
            // 
            this.grpWorkState.Controls.Add(this.lblCamera3DStateValue);
            this.grpWorkState.Controls.Add(this.lblCamera3DStateTitle);
            this.grpWorkState.Controls.Add(this.lblRobotStateValue);
            this.grpWorkState.Controls.Add(this.lblRobotStateTitle);
            this.grpWorkState.Controls.Add(this.lblScanStateValue);
            this.grpWorkState.Controls.Add(this.lblScanStateTitle);
            this.grpWorkState.Controls.Add(this.lblReaderStateValue);
            this.grpWorkState.Controls.Add(this.lblReaderStateTitle);
            this.grpWorkState.Location = new System.Drawing.Point(360, 8);
            this.grpWorkState.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpWorkState.Name = "grpWorkState";
            this.grpWorkState.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpWorkState.Size = new System.Drawing.Size(188, 104);
            this.grpWorkState.TabIndex = 1;
            this.grpWorkState.TabStop = false;
            this.grpWorkState.Text = "工作状态";
            // 
            // lblCamera3DStateValue
            // 
            this.lblCamera3DStateValue.Location = new System.Drawing.Point(64, 82);
            this.lblCamera3DStateValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCamera3DStateValue.Name = "lblCamera3DStateValue";
            this.lblCamera3DStateValue.Size = new System.Drawing.Size(116, 16);
            this.lblCamera3DStateValue.TabIndex = 0;
            this.lblCamera3DStateValue.Text = "-";
            // 
            // lblCamera3DStateTitle
            // 
            this.lblCamera3DStateTitle.Location = new System.Drawing.Point(9, 82);
            this.lblCamera3DStateTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCamera3DStateTitle.Name = "lblCamera3DStateTitle";
            this.lblCamera3DStateTitle.Size = new System.Drawing.Size(52, 16);
            this.lblCamera3DStateTitle.TabIndex = 1;
            this.lblCamera3DStateTitle.Text = "3D相机";
            // 
            // lblRobotStateValue
            // 
            this.lblRobotStateValue.Location = new System.Drawing.Point(64, 61);
            this.lblRobotStateValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblRobotStateValue.Name = "lblRobotStateValue";
            this.lblRobotStateValue.Size = new System.Drawing.Size(116, 16);
            this.lblRobotStateValue.TabIndex = 2;
            this.lblRobotStateValue.Text = "-";
            // 
            // lblRobotStateTitle
            // 
            this.lblRobotStateTitle.Location = new System.Drawing.Point(9, 61);
            this.lblRobotStateTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblRobotStateTitle.Name = "lblRobotStateTitle";
            this.lblRobotStateTitle.Size = new System.Drawing.Size(52, 16);
            this.lblRobotStateTitle.TabIndex = 3;
            this.lblRobotStateTitle.Text = "机械臂";
            // 
            // lblScanStateValue
            // 
            this.lblScanStateValue.Location = new System.Drawing.Point(64, 40);
            this.lblScanStateValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblScanStateValue.Name = "lblScanStateValue";
            this.lblScanStateValue.Size = new System.Drawing.Size(116, 16);
            this.lblScanStateValue.TabIndex = 4;
            this.lblScanStateValue.Text = "-";
            // 
            // lblScanStateTitle
            // 
            this.lblScanStateTitle.Location = new System.Drawing.Point(9, 40);
            this.lblScanStateTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblScanStateTitle.Name = "lblScanStateTitle";
            this.lblScanStateTitle.Size = new System.Drawing.Size(52, 16);
            this.lblScanStateTitle.TabIndex = 5;
            this.lblScanStateTitle.Text = "采码";
            // 
            // lblReaderStateValue
            // 
            this.lblReaderStateValue.Location = new System.Drawing.Point(64, 19);
            this.lblReaderStateValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblReaderStateValue.Name = "lblReaderStateValue";
            this.lblReaderStateValue.Size = new System.Drawing.Size(116, 16);
            this.lblReaderStateValue.TabIndex = 6;
            this.lblReaderStateValue.Text = "-";
            // 
            // lblReaderStateTitle
            // 
            this.lblReaderStateTitle.Location = new System.Drawing.Point(9, 19);
            this.lblReaderStateTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblReaderStateTitle.Name = "lblReaderStateTitle";
            this.lblReaderStateTitle.Size = new System.Drawing.Size(52, 16);
            this.lblReaderStateTitle.TabIndex = 7;
            this.lblReaderStateTitle.Text = "扫码器";
            // 
            // panelUpper
            // 
            this.panelUpper.Controls.Add(this.grpAlarmSummary);
            this.panelUpper.Controls.Add(this.grpCurrentResult);
            this.panelUpper.Controls.Add(this.grpScanImage);
            this.panelUpper.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelUpper.Location = new System.Drawing.Point(0, 120);
            this.panelUpper.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panelUpper.Name = "panelUpper";
            this.panelUpper.Padding = new System.Windows.Forms.Padding(8, 0, 8, 8);
            this.panelUpper.Size = new System.Drawing.Size(960, 200);
            this.panelUpper.TabIndex = 1;
            // 
            // grpAlarmSummary
            // 
            this.grpAlarmSummary.Controls.Add(this.lblCurrentStateValue);
            this.grpAlarmSummary.Controls.Add(this.lblCurrentStateTitle);
            this.grpAlarmSummary.Controls.Add(this.lblTestTotalValue);
            this.grpAlarmSummary.Controls.Add(this.lblTestTotalTitle);
            this.grpAlarmSummary.Controls.Add(this.lblAbnormalTotalValue);
            this.grpAlarmSummary.Controls.Add(this.lblAbnormalTotalTitle);
            this.grpAlarmSummary.Controls.Add(this.lblActualTotalValue);
            this.grpAlarmSummary.Controls.Add(this.lblActualTotalTitle);
            this.grpAlarmSummary.Dock = System.Windows.Forms.DockStyle.Right;
            this.grpAlarmSummary.Location = new System.Drawing.Point(701, 0);
            this.grpAlarmSummary.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpAlarmSummary.Name = "grpAlarmSummary";
            this.grpAlarmSummary.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpAlarmSummary.Size = new System.Drawing.Size(251, 192);
            this.grpAlarmSummary.TabIndex = 2;
            this.grpAlarmSummary.TabStop = false;
            this.grpAlarmSummary.Text = "数量与异常";
            // 
            // lblCurrentStateValue
            // 
            this.lblCurrentStateValue.Font = new System.Drawing.Font("Microsoft YaHei UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblCurrentStateValue.ForeColor = System.Drawing.Color.DarkOrange;
            this.lblCurrentStateValue.Location = new System.Drawing.Point(109, 138);
            this.lblCurrentStateValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCurrentStateValue.Name = "lblCurrentStateValue";
            this.lblCurrentStateValue.Size = new System.Drawing.Size(120, 27);
            this.lblCurrentStateValue.TabIndex = 0;
            this.lblCurrentStateValue.Text = "-";
            // 
            // lblCurrentStateTitle
            // 
            this.lblCurrentStateTitle.Location = new System.Drawing.Point(15, 141);
            this.lblCurrentStateTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCurrentStateTitle.Name = "lblCurrentStateTitle";
            this.lblCurrentStateTitle.Size = new System.Drawing.Size(79, 19);
            this.lblCurrentStateTitle.TabIndex = 1;
            this.lblCurrentStateTitle.Text = "当前状态";
            // 
            // lblTestTotalValue
            // 
            this.lblTestTotalValue.Font = new System.Drawing.Font("Microsoft YaHei UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTestTotalValue.Location = new System.Drawing.Point(109, 95);
            this.lblTestTotalValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTestTotalValue.Name = "lblTestTotalValue";
            this.lblTestTotalValue.Size = new System.Drawing.Size(75, 34);
            this.lblTestTotalValue.TabIndex = 2;
            this.lblTestTotalValue.Text = "0";
            // 
            // lblTestTotalTitle
            // 
            this.lblTestTotalTitle.Location = new System.Drawing.Point(15, 103);
            this.lblTestTotalTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblTestTotalTitle.Name = "lblTestTotalTitle";
            this.lblTestTotalTitle.Size = new System.Drawing.Size(79, 19);
            this.lblTestTotalTitle.TabIndex = 3;
            this.lblTestTotalTitle.Text = "测试记录数";
            // 
            // lblAbnormalTotalValue
            // 
            this.lblAbnormalTotalValue.Font = new System.Drawing.Font("Microsoft YaHei UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblAbnormalTotalValue.ForeColor = System.Drawing.Color.Firebrick;
            this.lblAbnormalTotalValue.Location = new System.Drawing.Point(109, 58);
            this.lblAbnormalTotalValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblAbnormalTotalValue.Name = "lblAbnormalTotalValue";
            this.lblAbnormalTotalValue.Size = new System.Drawing.Size(75, 34);
            this.lblAbnormalTotalValue.TabIndex = 4;
            this.lblAbnormalTotalValue.Text = "0";
            // 
            // lblAbnormalTotalTitle
            // 
            this.lblAbnormalTotalTitle.Location = new System.Drawing.Point(15, 66);
            this.lblAbnormalTotalTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblAbnormalTotalTitle.Name = "lblAbnormalTotalTitle";
            this.lblAbnormalTotalTitle.Size = new System.Drawing.Size(79, 19);
            this.lblAbnormalTotalTitle.TabIndex = 5;
            this.lblAbnormalTotalTitle.Text = "异常数量";
            // 
            // lblActualTotalValue
            // 
            this.lblActualTotalValue.Font = new System.Drawing.Font("Microsoft YaHei UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblActualTotalValue.ForeColor = System.Drawing.Color.SeaGreen;
            this.lblActualTotalValue.Location = new System.Drawing.Point(109, 20);
            this.lblActualTotalValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblActualTotalValue.Name = "lblActualTotalValue";
            this.lblActualTotalValue.Size = new System.Drawing.Size(75, 34);
            this.lblActualTotalValue.TabIndex = 6;
            this.lblActualTotalValue.Text = "0";
            // 
            // lblActualTotalTitle
            // 
            this.lblActualTotalTitle.Location = new System.Drawing.Point(15, 28);
            this.lblActualTotalTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblActualTotalTitle.Name = "lblActualTotalTitle";
            this.lblActualTotalTitle.Size = new System.Drawing.Size(79, 19);
            this.lblActualTotalTitle.TabIndex = 7;
            this.lblActualTotalTitle.Text = "实扫总数";
            // 
            // grpCurrentResult
            // 
            this.grpCurrentResult.Controls.Add(this.lblResultScanTimeValue);
            this.grpCurrentResult.Controls.Add(this.lblResultScanTimeTitle);
            this.grpCurrentResult.Controls.Add(this.lblResultScanCountValue);
            this.grpCurrentResult.Controls.Add(this.lblResultScanCountTitle);
            this.grpCurrentResult.Controls.Add(this.lblResultStatusValue);
            this.grpCurrentResult.Controls.Add(this.lblResultStatusTitle);
            this.grpCurrentResult.Controls.Add(this.lblResultSkuValue);
            this.grpCurrentResult.Controls.Add(this.lblResultSkuTitle);
            this.grpCurrentResult.Controls.Add(this.lblResultBarcodeValue);
            this.grpCurrentResult.Controls.Add(this.lblResultBarcodeTitle);
            this.grpCurrentResult.Controls.Add(this.lblResultSequenceValue);
            this.grpCurrentResult.Controls.Add(this.lblResultSequenceTitle);
            this.grpCurrentResult.Controls.Add(this.lblResultBatchBoxValue);
            this.grpCurrentResult.Controls.Add(this.lblResultBatchBoxTitle);
            this.grpCurrentResult.Controls.Add(this.lblResultOrderNoValue);
            this.grpCurrentResult.Controls.Add(this.lblResultOrderNoTitle);
            this.grpCurrentResult.Location = new System.Drawing.Point(308, 0);
            this.grpCurrentResult.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpCurrentResult.Name = "grpCurrentResult";
            this.grpCurrentResult.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpCurrentResult.Size = new System.Drawing.Size(386, 192);
            this.grpCurrentResult.TabIndex = 1;
            this.grpCurrentResult.TabStop = false;
            this.grpCurrentResult.Text = "当前采码结果";
            // 
            // lblResultScanTimeValue
            // 
            this.lblResultScanTimeValue.Location = new System.Drawing.Point(79, 138);
            this.lblResultScanTimeValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblResultScanTimeValue.Name = "lblResultScanTimeValue";
            this.lblResultScanTimeValue.Size = new System.Drawing.Size(262, 18);
            this.lblResultScanTimeValue.TabIndex = 0;
            this.lblResultScanTimeValue.Text = "-";
            // 
            // lblResultScanTimeTitle
            // 
            this.lblResultScanTimeTitle.Location = new System.Drawing.Point(12, 138);
            this.lblResultScanTimeTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblResultScanTimeTitle.Name = "lblResultScanTimeTitle";
            this.lblResultScanTimeTitle.Size = new System.Drawing.Size(64, 18);
            this.lblResultScanTimeTitle.TabIndex = 1;
            this.lblResultScanTimeTitle.Text = "采码时间";
            // 
            // lblResultScanCountValue
            // 
            this.lblResultScanCountValue.Location = new System.Drawing.Point(270, 109);
            this.lblResultScanCountValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblResultScanCountValue.Name = "lblResultScanCountValue";
            this.lblResultScanCountValue.Size = new System.Drawing.Size(105, 18);
            this.lblResultScanCountValue.TabIndex = 2;
            this.lblResultScanCountValue.Text = "-";
            // 
            // lblResultScanCountTitle
            // 
            this.lblResultScanCountTitle.Location = new System.Drawing.Point(202, 109);
            this.lblResultScanCountTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblResultScanCountTitle.Name = "lblResultScanCountTitle";
            this.lblResultScanCountTitle.Size = new System.Drawing.Size(64, 18);
            this.lblResultScanCountTitle.TabIndex = 3;
            this.lblResultScanCountTitle.Text = "采码次数";
            // 
            // lblResultStatusValue
            // 
            this.lblResultStatusValue.Location = new System.Drawing.Point(79, 109);
            this.lblResultStatusValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblResultStatusValue.Name = "lblResultStatusValue";
            this.lblResultStatusValue.Size = new System.Drawing.Size(120, 18);
            this.lblResultStatusValue.TabIndex = 4;
            this.lblResultStatusValue.Text = "-";
            // 
            // lblResultStatusTitle
            // 
            this.lblResultStatusTitle.Location = new System.Drawing.Point(12, 109);
            this.lblResultStatusTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblResultStatusTitle.Name = "lblResultStatusTitle";
            this.lblResultStatusTitle.Size = new System.Drawing.Size(64, 18);
            this.lblResultStatusTitle.TabIndex = 5;
            this.lblResultStatusTitle.Text = "状态";
            // 
            // lblResultSkuValue
            // 
            this.lblResultSkuValue.Location = new System.Drawing.Point(270, 80);
            this.lblResultSkuValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblResultSkuValue.Name = "lblResultSkuValue";
            this.lblResultSkuValue.Size = new System.Drawing.Size(105, 18);
            this.lblResultSkuValue.TabIndex = 6;
            this.lblResultSkuValue.Text = "-";
            // 
            // lblResultSkuTitle
            // 
            this.lblResultSkuTitle.Location = new System.Drawing.Point(202, 80);
            this.lblResultSkuTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblResultSkuTitle.Name = "lblResultSkuTitle";
            this.lblResultSkuTitle.Size = new System.Drawing.Size(64, 18);
            this.lblResultSkuTitle.TabIndex = 7;
            this.lblResultSkuTitle.Text = "产品货号";
            // 
            // lblResultBarcodeValue
            // 
            this.lblResultBarcodeValue.Location = new System.Drawing.Point(79, 80);
            this.lblResultBarcodeValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblResultBarcodeValue.Name = "lblResultBarcodeValue";
            this.lblResultBarcodeValue.Size = new System.Drawing.Size(120, 18);
            this.lblResultBarcodeValue.TabIndex = 8;
            this.lblResultBarcodeValue.Text = "-";
            // 
            // lblResultBarcodeTitle
            // 
            this.lblResultBarcodeTitle.Location = new System.Drawing.Point(12, 80);
            this.lblResultBarcodeTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblResultBarcodeTitle.Name = "lblResultBarcodeTitle";
            this.lblResultBarcodeTitle.Size = new System.Drawing.Size(64, 18);
            this.lblResultBarcodeTitle.TabIndex = 9;
            this.lblResultBarcodeTitle.Text = "条形码号";
            // 
            // lblResultSequenceValue
            // 
            this.lblResultSequenceValue.Location = new System.Drawing.Point(79, 51);
            this.lblResultSequenceValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblResultSequenceValue.Name = "lblResultSequenceValue";
            this.lblResultSequenceValue.Size = new System.Drawing.Size(120, 18);
            this.lblResultSequenceValue.TabIndex = 10;
            this.lblResultSequenceValue.Text = "-";
            // 
            // lblResultSequenceTitle
            // 
            this.lblResultSequenceTitle.Location = new System.Drawing.Point(12, 51);
            this.lblResultSequenceTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblResultSequenceTitle.Name = "lblResultSequenceTitle";
            this.lblResultSequenceTitle.Size = new System.Drawing.Size(64, 18);
            this.lblResultSequenceTitle.TabIndex = 11;
            this.lblResultSequenceTitle.Text = "次序";
            // 
            // lblResultBatchBoxValue
            // 
            this.lblResultBatchBoxValue.Location = new System.Drawing.Point(270, 22);
            this.lblResultBatchBoxValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblResultBatchBoxValue.Name = "lblResultBatchBoxValue";
            this.lblResultBatchBoxValue.Size = new System.Drawing.Size(105, 18);
            this.lblResultBatchBoxValue.TabIndex = 12;
            this.lblResultBatchBoxValue.Text = "-";
            // 
            // lblResultBatchBoxTitle
            // 
            this.lblResultBatchBoxTitle.Location = new System.Drawing.Point(202, 22);
            this.lblResultBatchBoxTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblResultBatchBoxTitle.Name = "lblResultBatchBoxTitle";
            this.lblResultBatchBoxTitle.Size = new System.Drawing.Size(64, 18);
            this.lblResultBatchBoxTitle.TabIndex = 13;
            this.lblResultBatchBoxTitle.Text = "箱码编号";
            // 
            // lblResultOrderNoValue
            // 
            this.lblResultOrderNoValue.Location = new System.Drawing.Point(79, 22);
            this.lblResultOrderNoValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblResultOrderNoValue.Name = "lblResultOrderNoValue";
            this.lblResultOrderNoValue.Size = new System.Drawing.Size(120, 18);
            this.lblResultOrderNoValue.TabIndex = 14;
            this.lblResultOrderNoValue.Text = "-";
            // 
            // lblResultOrderNoTitle
            // 
            this.lblResultOrderNoTitle.Location = new System.Drawing.Point(12, 22);
            this.lblResultOrderNoTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblResultOrderNoTitle.Name = "lblResultOrderNoTitle";
            this.lblResultOrderNoTitle.Size = new System.Drawing.Size(64, 18);
            this.lblResultOrderNoTitle.TabIndex = 15;
            this.lblResultOrderNoTitle.Text = "订单编号";
            // 
            // grpScanImage
            // 
            this.grpScanImage.Controls.Add(this.lblPhotoSourceValue);
            this.grpScanImage.Controls.Add(this.lblPhotoSourceTitle);
            this.grpScanImage.Controls.Add(this.lblPhotoSkuValue);
            this.grpScanImage.Controls.Add(this.lblPhotoSkuTitle);
            this.grpScanImage.Controls.Add(this.lblPhotoBarcodeValue);
            this.grpScanImage.Controls.Add(this.lblPhotoBarcodeTitle);
            this.grpScanImage.Controls.Add(this.picScanImage);
            this.grpScanImage.Dock = System.Windows.Forms.DockStyle.Left;
            this.grpScanImage.Location = new System.Drawing.Point(8, 0);
            this.grpScanImage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpScanImage.Name = "grpScanImage";
            this.grpScanImage.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpScanImage.Size = new System.Drawing.Size(292, 192);
            this.grpScanImage.TabIndex = 0;
            this.grpScanImage.TabStop = false;
            this.grpScanImage.Text = "当前/历史采码照片";
            // 
            // lblPhotoSourceValue
            // 
            this.lblPhotoSourceValue.Location = new System.Drawing.Point(66, 168);
            this.lblPhotoSourceValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblPhotoSourceValue.Name = "lblPhotoSourceValue";
            this.lblPhotoSourceValue.Size = new System.Drawing.Size(188, 16);
            this.lblPhotoSourceValue.TabIndex = 0;
            this.lblPhotoSourceValue.Text = "-";
            // 
            // lblPhotoSourceTitle
            // 
            this.lblPhotoSourceTitle.Location = new System.Drawing.Point(9, 168);
            this.lblPhotoSourceTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblPhotoSourceTitle.Name = "lblPhotoSourceTitle";
            this.lblPhotoSourceTitle.Size = new System.Drawing.Size(56, 16);
            this.lblPhotoSourceTitle.TabIndex = 1;
            this.lblPhotoSourceTitle.Text = "照片来源";
            // 
            // lblPhotoSkuValue
            // 
            this.lblPhotoSkuValue.Location = new System.Drawing.Point(202, 148);
            this.lblPhotoSkuValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblPhotoSkuValue.Name = "lblPhotoSkuValue";
            this.lblPhotoSkuValue.Size = new System.Drawing.Size(79, 16);
            this.lblPhotoSkuValue.TabIndex = 2;
            this.lblPhotoSkuValue.Text = "-";
            // 
            // lblPhotoSkuTitle
            // 
            this.lblPhotoSkuTitle.Location = new System.Drawing.Point(154, 148);
            this.lblPhotoSkuTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblPhotoSkuTitle.Name = "lblPhotoSkuTitle";
            this.lblPhotoSkuTitle.Size = new System.Drawing.Size(49, 16);
            this.lblPhotoSkuTitle.TabIndex = 3;
            this.lblPhotoSkuTitle.Text = "货号";
            // 
            // lblPhotoBarcodeValue
            // 
            this.lblPhotoBarcodeValue.Location = new System.Drawing.Point(66, 148);
            this.lblPhotoBarcodeValue.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblPhotoBarcodeValue.Name = "lblPhotoBarcodeValue";
            this.lblPhotoBarcodeValue.Size = new System.Drawing.Size(86, 16);
            this.lblPhotoBarcodeValue.TabIndex = 4;
            this.lblPhotoBarcodeValue.Text = "-";
            // 
            // lblPhotoBarcodeTitle
            // 
            this.lblPhotoBarcodeTitle.Location = new System.Drawing.Point(9, 148);
            this.lblPhotoBarcodeTitle.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblPhotoBarcodeTitle.Name = "lblPhotoBarcodeTitle";
            this.lblPhotoBarcodeTitle.Size = new System.Drawing.Size(56, 16);
            this.lblPhotoBarcodeTitle.TabIndex = 5;
            this.lblPhotoBarcodeTitle.Text = "条形码号";
            // 
            // picScanImage
            // 
            this.picScanImage.BackColor = System.Drawing.Color.WhiteSmoke;
            this.picScanImage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picScanImage.Location = new System.Drawing.Point(9, 19);
            this.picScanImage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.picScanImage.Name = "picScanImage";
            this.picScanImage.Size = new System.Drawing.Size(274, 124);
            this.picScanImage.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.picScanImage.TabIndex = 6;
            this.picScanImage.TabStop = false;
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.tabCurrentOrder);
            this.tabMain.Controls.Add(this.tabHistory);
            this.tabMain.Controls.Add(this.tabTestRecords);
            this.tabMain.Controls.Add(this.tabLog);
            this.tabMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabMain.Location = new System.Drawing.Point(0, 320);
            this.tabMain.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Size = new System.Drawing.Size(960, 268);
            this.tabMain.TabIndex = 2;
            // 
            // tabCurrentOrder
            // 
            this.tabCurrentOrder.Controls.Add(this.splitCurrent);
            this.tabCurrentOrder.Location = new System.Drawing.Point(4, 22);
            this.tabCurrentOrder.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabCurrentOrder.Name = "tabCurrentOrder";
            this.tabCurrentOrder.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.tabCurrentOrder.Size = new System.Drawing.Size(952, 242);
            this.tabCurrentOrder.TabIndex = 0;
            this.tabCurrentOrder.Text = "当前订单采码";
            this.tabCurrentOrder.UseVisualStyleBackColor = true;
            // 
            // splitCurrent
            // 
            this.splitCurrent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitCurrent.Location = new System.Drawing.Point(6, 6);
            this.splitCurrent.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.splitCurrent.Name = "splitCurrent";
            // 
            // splitCurrent.Panel1
            // 
            this.splitCurrent.Panel1.Controls.Add(this.grpProductScanList);
            // 
            // splitCurrent.Panel2
            // 
            this.splitCurrent.Panel2.Controls.Add(this.grpScanMatchResult);
            this.splitCurrent.Size = new System.Drawing.Size(940, 230);
            this.splitCurrent.SplitterDistance = 523;
            this.splitCurrent.SplitterWidth = 3;
            this.splitCurrent.TabIndex = 0;
            // 
            // grpProductScanList
            // 
            this.grpProductScanList.Controls.Add(this.dgvProductScanList);
            this.grpProductScanList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpProductScanList.Location = new System.Drawing.Point(0, 0);
            this.grpProductScanList.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpProductScanList.Name = "grpProductScanList";
            this.grpProductScanList.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpProductScanList.Size = new System.Drawing.Size(523, 230);
            this.grpProductScanList.TabIndex = 0;
            this.grpProductScanList.TabStop = false;
            this.grpProductScanList.Text = "产品采码列表";
            // 
            // dgvProductScanList
            // 
            this.dgvProductScanList.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvProductScanList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colProductOrderNo,
            this.colProductSequence,
            this.colProductBarcode,
            this.colProductSku,
            this.colProductStatus,
            this.colProductScanCount});
            this.dgvProductScanList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvProductScanList.Location = new System.Drawing.Point(2, 16);
            this.dgvProductScanList.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dgvProductScanList.Name = "dgvProductScanList";
            this.dgvProductScanList.RowHeadersVisible = false;
            this.dgvProductScanList.Size = new System.Drawing.Size(519, 212);
            this.dgvProductScanList.TabIndex = 0;
            this.dgvProductScanList.SelectionChanged += new System.EventHandler(this.dgvProductScanList_SelectionChanged);
            // 
            // colProductOrderNo
            // 
            this.colProductOrderNo.HeaderText = "订单编号";
            this.colProductOrderNo.Name = "colProductOrderNo";
            // 
            // colProductSequence
            // 
            this.colProductSequence.HeaderText = "次序";
            this.colProductSequence.Name = "colProductSequence";
            // 
            // colProductBarcode
            // 
            this.colProductBarcode.HeaderText = "条形码号";
            this.colProductBarcode.Name = "colProductBarcode";
            // 
            // colProductSku
            // 
            this.colProductSku.HeaderText = "产品货号";
            this.colProductSku.Name = "colProductSku";
            // 
            // colProductStatus
            // 
            this.colProductStatus.HeaderText = "状态";
            this.colProductStatus.Name = "colProductStatus";
            // 
            // colProductScanCount
            // 
            this.colProductScanCount.HeaderText = "采码次数";
            this.colProductScanCount.Name = "colProductScanCount";
            // 
            // grpScanMatchResult
            // 
            this.grpScanMatchResult.Controls.Add(this.dgvScanMatchResult);
            this.grpScanMatchResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpScanMatchResult.Location = new System.Drawing.Point(0, 0);
            this.grpScanMatchResult.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpScanMatchResult.Name = "grpScanMatchResult";
            this.grpScanMatchResult.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpScanMatchResult.Size = new System.Drawing.Size(414, 230);
            this.grpScanMatchResult.TabIndex = 0;
            this.grpScanMatchResult.TabStop = false;
            this.grpScanMatchResult.Text = "扫码匹配结果";
            // 
            // dgvScanMatchResult
            // 
            this.dgvScanMatchResult.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvScanMatchResult.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colMatchBarcode,
            this.colMatchSku,
            this.colMatchOrderQuantity,
            this.colMatchActualQuantity,
            this.colMatchStatus});
            this.dgvScanMatchResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvScanMatchResult.Location = new System.Drawing.Point(2, 16);
            this.dgvScanMatchResult.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dgvScanMatchResult.Name = "dgvScanMatchResult";
            this.dgvScanMatchResult.RowHeadersVisible = false;
            this.dgvScanMatchResult.Size = new System.Drawing.Size(410, 212);
            this.dgvScanMatchResult.TabIndex = 0;
            // 
            // colMatchBarcode
            // 
            this.colMatchBarcode.HeaderText = "条码号";
            this.colMatchBarcode.Name = "colMatchBarcode";
            // 
            // colMatchSku
            // 
            this.colMatchSku.HeaderText = "货号";
            this.colMatchSku.Name = "colMatchSku";
            // 
            // colMatchOrderQuantity
            // 
            this.colMatchOrderQuantity.HeaderText = "订单数量";
            this.colMatchOrderQuantity.Name = "colMatchOrderQuantity";
            // 
            // colMatchActualQuantity
            // 
            this.colMatchActualQuantity.HeaderText = "实扫数量";
            this.colMatchActualQuantity.Name = "colMatchActualQuantity";
            // 
            // colMatchStatus
            // 
            this.colMatchStatus.HeaderText = "状态";
            this.colMatchStatus.Name = "colMatchStatus";
            // 
            // tabHistory
            // 
            this.tabHistory.Controls.Add(this.splitHistory);
            this.tabHistory.Controls.Add(this.panelHistorySearch);
            this.tabHistory.Location = new System.Drawing.Point(4, 22);
            this.tabHistory.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabHistory.Name = "tabHistory";
            this.tabHistory.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.tabHistory.Size = new System.Drawing.Size(952, 242);
            this.tabHistory.TabIndex = 1;
            this.tabHistory.Text = "历史订单搜索";
            this.tabHistory.UseVisualStyleBackColor = true;
            // 
            // splitHistory
            // 
            this.splitHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitHistory.Location = new System.Drawing.Point(6, 49);
            this.splitHistory.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.splitHistory.Name = "splitHistory";
            // 
            // splitHistory.Panel1
            // 
            this.splitHistory.Panel1.Controls.Add(this.grpHistoryScanList);
            // 
            // splitHistory.Panel2
            // 
            this.splitHistory.Panel2.Controls.Add(this.grpHistoryMatch);
            this.splitHistory.Size = new System.Drawing.Size(940, 187);
            this.splitHistory.SplitterDistance = 567;
            this.splitHistory.SplitterWidth = 3;
            this.splitHistory.TabIndex = 0;
            // 
            // grpHistoryScanList
            // 
            this.grpHistoryScanList.Controls.Add(this.dgvHistoryScanList);
            this.grpHistoryScanList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpHistoryScanList.Location = new System.Drawing.Point(0, 0);
            this.grpHistoryScanList.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpHistoryScanList.Name = "grpHistoryScanList";
            this.grpHistoryScanList.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpHistoryScanList.Size = new System.Drawing.Size(567, 187);
            this.grpHistoryScanList.TabIndex = 0;
            this.grpHistoryScanList.TabStop = false;
            this.grpHistoryScanList.Text = "历史产品采码列表";
            // 
            // dgvHistoryScanList
            // 
            this.dgvHistoryScanList.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvHistoryScanList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colHistoryOrderNo,
            this.colHistorySequence,
            this.colHistoryBarcode,
            this.colHistorySku,
            this.colHistoryStatus,
            this.colHistoryScanCount});
            this.dgvHistoryScanList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvHistoryScanList.Location = new System.Drawing.Point(2, 16);
            this.dgvHistoryScanList.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dgvHistoryScanList.Name = "dgvHistoryScanList";
            this.dgvHistoryScanList.RowHeadersVisible = false;
            this.dgvHistoryScanList.Size = new System.Drawing.Size(563, 169);
            this.dgvHistoryScanList.TabIndex = 0;
            this.dgvHistoryScanList.SelectionChanged += new System.EventHandler(this.dgvHistoryScanList_SelectionChanged);
            // 
            // colHistoryOrderNo
            // 
            this.colHistoryOrderNo.HeaderText = "订单编号";
            this.colHistoryOrderNo.Name = "colHistoryOrderNo";
            // 
            // colHistorySequence
            // 
            this.colHistorySequence.HeaderText = "次序";
            this.colHistorySequence.Name = "colHistorySequence";
            // 
            // colHistoryBarcode
            // 
            this.colHistoryBarcode.HeaderText = "条形码号";
            this.colHistoryBarcode.Name = "colHistoryBarcode";
            // 
            // colHistorySku
            // 
            this.colHistorySku.HeaderText = "产品货号";
            this.colHistorySku.Name = "colHistorySku";
            // 
            // colHistoryStatus
            // 
            this.colHistoryStatus.HeaderText = "状态";
            this.colHistoryStatus.Name = "colHistoryStatus";
            // 
            // colHistoryScanCount
            // 
            this.colHistoryScanCount.HeaderText = "采码次数";
            this.colHistoryScanCount.Name = "colHistoryScanCount";
            // 
            // grpHistoryMatch
            // 
            this.grpHistoryMatch.Controls.Add(this.dgvHistoryMatchResult);
            this.grpHistoryMatch.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpHistoryMatch.Location = new System.Drawing.Point(0, 0);
            this.grpHistoryMatch.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpHistoryMatch.Name = "grpHistoryMatch";
            this.grpHistoryMatch.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpHistoryMatch.Size = new System.Drawing.Size(370, 187);
            this.grpHistoryMatch.TabIndex = 0;
            this.grpHistoryMatch.TabStop = false;
            this.grpHistoryMatch.Text = "历史扫码匹配结果";
            // 
            // dgvHistoryMatchResult
            // 
            this.dgvHistoryMatchResult.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvHistoryMatchResult.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colHistoryMatchBarcode,
            this.colHistoryMatchSku,
            this.colHistoryMatchOrderQuantity,
            this.colHistoryMatchActualQuantity,
            this.colHistoryMatchStatus});
            this.dgvHistoryMatchResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvHistoryMatchResult.Location = new System.Drawing.Point(2, 16);
            this.dgvHistoryMatchResult.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dgvHistoryMatchResult.Name = "dgvHistoryMatchResult";
            this.dgvHistoryMatchResult.RowHeadersVisible = false;
            this.dgvHistoryMatchResult.Size = new System.Drawing.Size(366, 169);
            this.dgvHistoryMatchResult.TabIndex = 0;
            // 
            // colHistoryMatchBarcode
            // 
            this.colHistoryMatchBarcode.HeaderText = "条码号";
            this.colHistoryMatchBarcode.Name = "colHistoryMatchBarcode";
            // 
            // colHistoryMatchSku
            // 
            this.colHistoryMatchSku.HeaderText = "货号";
            this.colHistoryMatchSku.Name = "colHistoryMatchSku";
            // 
            // colHistoryMatchOrderQuantity
            // 
            this.colHistoryMatchOrderQuantity.HeaderText = "订单数量";
            this.colHistoryMatchOrderQuantity.Name = "colHistoryMatchOrderQuantity";
            // 
            // colHistoryMatchActualQuantity
            // 
            this.colHistoryMatchActualQuantity.HeaderText = "实扫数量";
            this.colHistoryMatchActualQuantity.Name = "colHistoryMatchActualQuantity";
            // 
            // colHistoryMatchStatus
            // 
            this.colHistoryMatchStatus.HeaderText = "状态";
            this.colHistoryMatchStatus.Name = "colHistoryMatchStatus";
            // 
            // panelHistorySearch
            // 
            this.panelHistorySearch.Controls.Add(this.rdoSearchByOrderNo);
            this.panelHistorySearch.Controls.Add(this.rdoSearchByBoxCode);
            this.panelHistorySearch.Controls.Add(this.lblHistorySearchKey);
            this.panelHistorySearch.Controls.Add(this.txtHistorySearchKey);
            this.panelHistorySearch.Controls.Add(this.btnSearchHistory);
            this.panelHistorySearch.Controls.Add(this.lblHistoryResult);
            this.panelHistorySearch.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHistorySearch.Location = new System.Drawing.Point(6, 6);
            this.panelHistorySearch.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panelHistorySearch.Name = "panelHistorySearch";
            this.panelHistorySearch.Size = new System.Drawing.Size(940, 43);
            this.panelHistorySearch.TabIndex = 1;
            // 
            // rdoSearchByOrderNo
            // 
            this.rdoSearchByOrderNo.AutoSize = true;
            this.rdoSearchByOrderNo.Checked = true;
            this.rdoSearchByOrderNo.Location = new System.Drawing.Point(6, 13);
            this.rdoSearchByOrderNo.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rdoSearchByOrderNo.Name = "rdoSearchByOrderNo";
            this.rdoSearchByOrderNo.Size = new System.Drawing.Size(107, 16);
            this.rdoSearchByOrderNo.TabIndex = 0;
            this.rdoSearchByOrderNo.TabStop = true;
            this.rdoSearchByOrderNo.Text = "按订单编号检索";
            this.rdoSearchByOrderNo.UseVisualStyleBackColor = true;
            // 
            // rdoSearchByBoxCode
            // 
            this.rdoSearchByBoxCode.AutoSize = true;
            this.rdoSearchByBoxCode.Location = new System.Drawing.Point(122, 13);
            this.rdoSearchByBoxCode.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rdoSearchByBoxCode.Name = "rdoSearchByBoxCode";
            this.rdoSearchByBoxCode.Size = new System.Drawing.Size(107, 16);
            this.rdoSearchByBoxCode.TabIndex = 1;
            this.rdoSearchByBoxCode.Text = "按箱码编号检索";
            this.rdoSearchByBoxCode.UseVisualStyleBackColor = true;
            // 
            // lblHistorySearchKey
            // 
            this.lblHistorySearchKey.Location = new System.Drawing.Point(298, 13);
            this.lblHistorySearchKey.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblHistorySearchKey.Name = "lblHistorySearchKey";
            this.lblHistorySearchKey.Size = new System.Drawing.Size(60, 18);
            this.lblHistorySearchKey.TabIndex = 2;
            this.lblHistorySearchKey.Text = "输入编号";
            // 
            // txtHistorySearchKey
            // 
            this.txtHistorySearchKey.Location = new System.Drawing.Point(353, 10);
            this.txtHistorySearchKey.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtHistorySearchKey.Name = "txtHistorySearchKey";
            this.txtHistorySearchKey.Size = new System.Drawing.Size(144, 21);
            this.txtHistorySearchKey.TabIndex = 3;
            // 
            // btnSearchHistory
            // 
            this.btnSearchHistory.Location = new System.Drawing.Point(507, 7);
            this.btnSearchHistory.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnSearchHistory.Name = "btnSearchHistory";
            this.btnSearchHistory.Size = new System.Drawing.Size(82, 26);
            this.btnSearchHistory.TabIndex = 4;
            this.btnSearchHistory.Text = "查询历史";
            this.btnSearchHistory.Click += new System.EventHandler(this.btnSearchHistory_Click);
            // 
            // lblHistoryResult
            // 
            this.lblHistoryResult.Location = new System.Drawing.Point(595, 11);
            this.lblHistoryResult.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblHistoryResult.Name = "lblHistoryResult";
            this.lblHistoryResult.Size = new System.Drawing.Size(375, 19);
            this.lblHistoryResult.TabIndex = 5;
            this.lblHistoryResult.Text = "可按订单编号或箱码编号查询已扫描过的生产历史。";
            // 
            // tabTestRecords
            // 
            this.tabTestRecords.Controls.Add(this.grpTestRecords);
            this.tabTestRecords.Location = new System.Drawing.Point(4, 22);
            this.tabTestRecords.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabTestRecords.Name = "tabTestRecords";
            this.tabTestRecords.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.tabTestRecords.Size = new System.Drawing.Size(952, 244);
            this.tabTestRecords.TabIndex = 2;
            this.tabTestRecords.Text = "测试记录";
            this.tabTestRecords.UseVisualStyleBackColor = true;
            // 
            // grpTestRecords
            // 
            this.grpTestRecords.Controls.Add(this.dgvTestRecords);
            this.grpTestRecords.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpTestRecords.Location = new System.Drawing.Point(6, 6);
            this.grpTestRecords.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpTestRecords.Name = "grpTestRecords";
            this.grpTestRecords.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.grpTestRecords.Size = new System.Drawing.Size(940, 232);
            this.grpTestRecords.TabIndex = 0;
            this.grpTestRecords.TabStop = false;
            this.grpTestRecords.Text = "测试模式采码记录";
            // 
            // dgvTestRecords
            // 
            this.dgvTestRecords.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colTestOrderNo,
            this.colTestSequence,
            this.colTestBarcode,
            this.colTestSku,
            this.colTestStatus,
            this.colTestScanCount});
            this.dgvTestRecords.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvTestRecords.Location = new System.Drawing.Point(2, 16);
            this.dgvTestRecords.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dgvTestRecords.Name = "dgvTestRecords";
            this.dgvTestRecords.Size = new System.Drawing.Size(936, 214);
            this.dgvTestRecords.TabIndex = 0;
            this.dgvTestRecords.SelectionChanged += new System.EventHandler(this.dgvTestRecords_SelectionChanged);
            // 
            // colTestOrderNo
            // 
            this.colTestOrderNo.HeaderText = "订单编号";
            this.colTestOrderNo.Name = "colTestOrderNo";
            // 
            // colTestSequence
            // 
            this.colTestSequence.HeaderText = "次序";
            this.colTestSequence.Name = "colTestSequence";
            // 
            // colTestBarcode
            // 
            this.colTestBarcode.HeaderText = "条形码号";
            this.colTestBarcode.Name = "colTestBarcode";
            // 
            // colTestSku
            // 
            this.colTestSku.HeaderText = "产品货号";
            this.colTestSku.Name = "colTestSku";
            // 
            // colTestStatus
            // 
            this.colTestStatus.HeaderText = "状态";
            this.colTestStatus.Name = "colTestStatus";
            // 
            // colTestScanCount
            // 
            this.colTestScanCount.HeaderText = "采码次数";
            this.colTestScanCount.Name = "colTestScanCount";
            // 
            // tabLog
            // 
            this.tabLog.Controls.Add(this.txtLog);
            this.tabLog.Location = new System.Drawing.Point(4, 22);
            this.tabLog.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabLog.Name = "tabLog";
            this.tabLog.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.tabLog.Size = new System.Drawing.Size(952, 244);
            this.tabLog.TabIndex = 3;
            this.tabLog.Text = "运行日志";
            this.tabLog.UseVisualStyleBackColor = true;
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Location = new System.Drawing.Point(6, 6);
            this.txtLog.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(940, 232);
            this.txtLog.TabIndex = 0;
            this.txtLog.WordWrap = false;
            // 
            // statusStripMain
            // 
            this.statusStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblFooterStatus});
            this.statusStripMain.Location = new System.Drawing.Point(0, 588);
            this.statusStripMain.Name = "statusStripMain";
            this.statusStripMain.Padding = new System.Windows.Forms.Padding(1, 0, 10, 0);
            this.statusStripMain.Size = new System.Drawing.Size(960, 22);
            this.statusStripMain.TabIndex = 3;
            // 
            // lblFooterStatus
            // 
            this.lblFooterStatus.Name = "lblFooterStatus";
            this.lblFooterStatus.Size = new System.Drawing.Size(327, 17);
            this.lblFooterStatus.Text = "订单编号: - | 箱码编号: - | 实扫: 0 | 异常: 0 | 当前: 工作模式";
            // 
            // ScanTestControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabMain);
            this.Controls.Add(this.panelUpper);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.statusStripMain);
            this.Name = "ScanTestControl";
            this.Size = new System.Drawing.Size(360, 640);
            this.panelTop.ResumeLayout(false);
            this.grpWorkMode.ResumeLayout(false);
            this.grpWorkMode.PerformLayout();
            this.grpDeviceActions.ResumeLayout(false);
            this.grpCurrentOrder.ResumeLayout(false);
            this.grpCurrentOrder.PerformLayout();
            this.grpWorkState.ResumeLayout(false);
            this.panelUpper.ResumeLayout(false);
            this.grpAlarmSummary.ResumeLayout(false);
            this.grpCurrentResult.ResumeLayout(false);
            this.grpScanImage.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picScanImage)).EndInit();
            this.tabMain.ResumeLayout(false);
            this.tabCurrentOrder.ResumeLayout(false);
            this.splitCurrent.Panel1.ResumeLayout(false);
            this.splitCurrent.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitCurrent)).EndInit();
            this.splitCurrent.ResumeLayout(false);
            this.grpProductScanList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvProductScanList)).EndInit();
            this.grpScanMatchResult.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvScanMatchResult)).EndInit();
            this.tabHistory.ResumeLayout(false);
            this.splitHistory.Panel1.ResumeLayout(false);
            this.splitHistory.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitHistory)).EndInit();
            this.splitHistory.ResumeLayout(false);
            this.grpHistoryScanList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistoryScanList)).EndInit();
            this.grpHistoryMatch.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvHistoryMatchResult)).EndInit();
            this.panelHistorySearch.ResumeLayout(false);
            this.panelHistorySearch.PerformLayout();
            this.tabTestRecords.ResumeLayout(false);
            this.grpTestRecords.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvTestRecords)).EndInit();
            this.tabLog.ResumeLayout(false);
            this.tabLog.PerformLayout();
            this.statusStripMain.ResumeLayout(false);
            this.statusStripMain.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.GroupBox grpCurrentOrder;
        private System.Windows.Forms.RadioButton rdoInputByOrderNo;
        private System.Windows.Forms.RadioButton rdoInputByBoxCode;
        private System.Windows.Forms.Label lblOrderBoxInput;
        private System.Windows.Forms.TextBox txtOrderBoxInput;
        private System.Windows.Forms.Label lblMatchedOrderNoTitle;
        private System.Windows.Forms.Label lblMatchedOrderNoValue;
        private System.Windows.Forms.Label lblMatchedBoxCodeTitle;
        private System.Windows.Forms.Label lblMatchedBoxCodeValue;
        private System.Windows.Forms.Button btnConfirmOrderBox;
        private System.Windows.Forms.Button btnOpenOrderMatch;
        private System.Windows.Forms.GroupBox grpWorkState;
        private System.Windows.Forms.Label lblReaderStateTitle;
        private System.Windows.Forms.Label lblReaderStateValue;
        private System.Windows.Forms.Label lblScanStateTitle;
        private System.Windows.Forms.Label lblScanStateValue;
        private System.Windows.Forms.Label lblRobotStateTitle;
        private System.Windows.Forms.Label lblRobotStateValue;
        private System.Windows.Forms.Label lblCamera3DStateTitle;
        private System.Windows.Forms.Label lblCamera3DStateValue;
        private System.Windows.Forms.GroupBox grpDeviceActions;
        private System.Windows.Forms.Button btnConnectReader;
        private System.Windows.Forms.Button btnReaderSettings;
        private System.Windows.Forms.Button btnWaitRobotSignal;
        private System.Windows.Forms.Button btnStartTestScan;
        private System.Windows.Forms.Button btnStopCurrentScan;
        private System.Windows.Forms.GroupBox grpWorkMode;
        private System.Windows.Forms.RadioButton rdoProductionMode;
        private System.Windows.Forms.RadioButton rdoTestMode;
        private System.Windows.Forms.Label lblModeHint;
        private System.Windows.Forms.Panel panelUpper;
        private System.Windows.Forms.GroupBox grpScanImage;
        private System.Windows.Forms.PictureBox picScanImage;
        private System.Windows.Forms.Label lblPhotoBarcodeTitle;
        private System.Windows.Forms.Label lblPhotoBarcodeValue;
        private System.Windows.Forms.Label lblPhotoSkuTitle;
        private System.Windows.Forms.Label lblPhotoSkuValue;
        private System.Windows.Forms.Label lblPhotoSourceTitle;
        private System.Windows.Forms.Label lblPhotoSourceValue;
        private System.Windows.Forms.GroupBox grpCurrentResult;
        private System.Windows.Forms.Label lblResultOrderNoTitle;
        private System.Windows.Forms.Label lblResultOrderNoValue;
        private System.Windows.Forms.Label lblResultBatchBoxTitle;
        private System.Windows.Forms.Label lblResultBatchBoxValue;
        private System.Windows.Forms.Label lblResultSequenceTitle;
        private System.Windows.Forms.Label lblResultSequenceValue;
        private System.Windows.Forms.Label lblResultBarcodeTitle;
        private System.Windows.Forms.Label lblResultBarcodeValue;
        private System.Windows.Forms.Label lblResultSkuTitle;
        private System.Windows.Forms.Label lblResultSkuValue;
        private System.Windows.Forms.Label lblResultStatusTitle;
        private System.Windows.Forms.Label lblResultStatusValue;
        private System.Windows.Forms.Label lblResultScanCountTitle;
        private System.Windows.Forms.Label lblResultScanCountValue;
        private System.Windows.Forms.Label lblResultScanTimeTitle;
        private System.Windows.Forms.Label lblResultScanTimeValue;
        private System.Windows.Forms.GroupBox grpAlarmSummary;
        private System.Windows.Forms.Label lblActualTotalTitle;
        private System.Windows.Forms.Label lblActualTotalValue;
        private System.Windows.Forms.Label lblAbnormalTotalTitle;
        private System.Windows.Forms.Label lblAbnormalTotalValue;
        private System.Windows.Forms.Label lblTestTotalTitle;
        private System.Windows.Forms.Label lblTestTotalValue;
        private System.Windows.Forms.Label lblCurrentStateTitle;
        private System.Windows.Forms.Label lblCurrentStateValue;
        private System.Windows.Forms.TabControl tabMain;
        private System.Windows.Forms.TabPage tabCurrentOrder;
        private System.Windows.Forms.SplitContainer splitCurrent;
        private System.Windows.Forms.GroupBox grpProductScanList;
        private System.Windows.Forms.DataGridView dgvProductScanList;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProductOrderNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProductSequence;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProductBarcode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProductSku;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProductStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colProductScanCount;
        private System.Windows.Forms.GroupBox grpScanMatchResult;
        private System.Windows.Forms.DataGridView dgvScanMatchResult;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMatchBarcode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMatchSku;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMatchOrderQuantity;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMatchActualQuantity;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMatchStatus;
        private System.Windows.Forms.TabPage tabHistory;
        private System.Windows.Forms.Panel panelHistorySearch;
        private System.Windows.Forms.RadioButton rdoSearchByOrderNo;
        private System.Windows.Forms.RadioButton rdoSearchByBoxCode;
        private System.Windows.Forms.Label lblHistorySearchKey;
        private System.Windows.Forms.TextBox txtHistorySearchKey;
        private System.Windows.Forms.Button btnSearchHistory;
        private System.Windows.Forms.Label lblHistoryResult;
        private System.Windows.Forms.SplitContainer splitHistory;
        private System.Windows.Forms.GroupBox grpHistoryScanList;
        private System.Windows.Forms.DataGridView dgvHistoryScanList;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHistoryOrderNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHistorySequence;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHistoryBarcode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHistorySku;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHistoryStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHistoryScanCount;
        private System.Windows.Forms.GroupBox grpHistoryMatch;
        private System.Windows.Forms.DataGridView dgvHistoryMatchResult;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHistoryMatchBarcode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHistoryMatchSku;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHistoryMatchOrderQuantity;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHistoryMatchActualQuantity;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHistoryMatchStatus;
        private System.Windows.Forms.TabPage tabTestRecords;
        private System.Windows.Forms.GroupBox grpTestRecords;
        private System.Windows.Forms.DataGridView dgvTestRecords;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTestOrderNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTestSequence;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTestBarcode;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTestSku;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTestStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTestScanCount;
        private System.Windows.Forms.TabPage tabLog;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.StatusStrip statusStripMain;
        private System.Windows.Forms.ToolStripStatusLabel lblFooterStatus;
    }
}
