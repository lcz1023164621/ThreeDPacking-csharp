namespace ThreeDPacking.App.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.menuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.menuOpenExcel = new System.Windows.Forms.ToolStripMenuItem();
            this.menuExportJson = new System.Windows.Forms.ToolStripMenuItem();
            this.menuSep1 = new System.Windows.Forms.ToolStripSeparator();
            this.menuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.menuRun = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStartPacking = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusUtilization = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.splitMain = new System.Windows.Forms.SplitContainer();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.lblContainers = new System.Windows.Forms.Label();
            this.dgvContainers = new System.Windows.Forms.DataGridView();
            this.colContName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colContDx = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colContDy = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colContDz = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colContWeight = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colContMaxLoad = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnAddContainer = new System.Windows.Forms.Button();
            this.btnRemoveContainer = new System.Windows.Forms.Button();
            this.lblItems = new System.Windows.Forms.Label();
            this.dgvItems = new System.Windows.Forms.DataGridView();
            this.colItemName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colItemDx = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colItemDy = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colItemDz = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.grpRandomSelect = new System.Windows.Forms.GroupBox();
            this.lblRandomInfo = new System.Windows.Forms.Label();
            this.btnRandomSelect = new System.Windows.Forms.Button();
            this.numRandomMax = new System.Windows.Forms.NumericUpDown();
            this.lblRandomMax = new System.Windows.Forms.Label();
            this.numRandomMin = new System.Windows.Forms.NumericUpDown();
            this.lblRandomMin = new System.Windows.Forms.Label();
            this.lblResults = new System.Windows.Forms.Label();
            this.lstResults = new System.Windows.Forms.ListBox();
            this.lblStep = new System.Windows.Forms.Label();
            this.trackStep = new System.Windows.Forms.TrackBar();
            this.lblStepInfo = new System.Windows.Forms.Label();
            this.grpSelected = new System.Windows.Forms.GroupBox();
            this.lblSelectedInfo = new System.Windows.Forms.Label();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.glControl = new OpenTK.GLControl();
            this.menuStrip.SuspendLayout();
            this.statusStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.panelLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvContainers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvItems)).BeginInit();
            this.grpRandomSelect.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRandomMax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRandomMin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackStep)).BeginInit();
            this.grpSelected.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuFile,
            this.menuRun});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(1257, 25);
            this.menuStrip.TabIndex = 2;
            // 
            // menuFile
            // 
            this.menuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuOpenExcel,
            this.menuExportJson,
            this.menuSep1,
            this.menuExit});
            this.menuFile.Name = "menuFile";
            this.menuFile.Size = new System.Drawing.Size(58, 21);
            this.menuFile.Text = "文件(&F)";
            // 
            // menuOpenExcel
            // 
            this.menuOpenExcel.Name = "menuOpenExcel";
            this.menuOpenExcel.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.menuOpenExcel.Size = new System.Drawing.Size(203, 22);
            this.menuOpenExcel.Text = "打开Excel(&O)...";
            // 
            // menuExportJson
            // 
            this.menuExportJson.Enabled = false;
            this.menuExportJson.Name = "menuExportJson";
            this.menuExportJson.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.menuExportJson.Size = new System.Drawing.Size(203, 22);
            this.menuExportJson.Text = "导出JSON(&E)...";
            // 
            // menuSep1
            // 
            this.menuSep1.Name = "menuSep1";
            this.menuSep1.Size = new System.Drawing.Size(200, 6);
            // 
            // menuExit
            // 
            this.menuExit.Name = "menuExit";
            this.menuExit.Size = new System.Drawing.Size(203, 22);
            this.menuExit.Text = "退出(&X)";
            // 
            // menuRun
            // 
            this.menuRun.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuStartPacking});
            this.menuRun.Name = "menuRun";
            this.menuRun.Size = new System.Drawing.Size(60, 21);
            this.menuRun.Text = "运行(&R)";
            // 
            // menuStartPacking
            // 
            this.menuStartPacking.Enabled = false;
            this.menuStartPacking.Name = "menuStartPacking";
            this.menuStartPacking.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.menuStartPacking.Size = new System.Drawing.Size(160, 22);
            this.menuStartPacking.Text = "开始装箱(&S)";
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel,
            this.statusUtilization,
            this.statusTime});
            this.statusStrip.Location = new System.Drawing.Point(0, 690);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1257, 22);
            this.statusStrip.TabIndex = 1;
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(942, 17);
            this.statusLabel.Spring = true;
            this.statusLabel.Text = "就绪";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statusUtilization
            // 
            this.statusUtilization.AutoSize = false;
            this.statusUtilization.Name = "statusUtilization";
            this.statusUtilization.Size = new System.Drawing.Size(180, 17);
            // 
            // statusTime
            // 
            this.statusTime.AutoSize = false;
            this.statusTime.Name = "statusTime";
            this.statusTime.Size = new System.Drawing.Size(120, 17);
            // 
            // splitMain
            // 
            this.splitMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMain.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitMain.Location = new System.Drawing.Point(0, 25);
            this.splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            this.splitMain.Panel1.Controls.Add(this.panelLeft);
            // 
            // splitMain.Panel2
            // 
            this.splitMain.Panel2.Controls.Add(this.glControl);
            this.splitMain.Size = new System.Drawing.Size(1257, 665);
            this.splitMain.SplitterDistance = 371;
            this.splitMain.TabIndex = 0;
            // 
            // panelLeft
            // 
            this.panelLeft.AutoScroll = true;
            this.panelLeft.Controls.Add(this.lblContainers);
            this.panelLeft.Controls.Add(this.dgvContainers);
            this.panelLeft.Controls.Add(this.btnAddContainer);
            this.panelLeft.Controls.Add(this.btnRemoveContainer);
            this.panelLeft.Controls.Add(this.lblItems);
            this.panelLeft.Controls.Add(this.dgvItems);
            this.panelLeft.Controls.Add(this.grpRandomSelect);
            this.panelLeft.Controls.Add(this.lblResults);
            this.panelLeft.Controls.Add(this.lstResults);
            this.panelLeft.Controls.Add(this.lblStep);
            this.panelLeft.Controls.Add(this.trackStep);
            this.panelLeft.Controls.Add(this.lblStepInfo);
            this.panelLeft.Controls.Add(this.grpSelected);
            this.panelLeft.Controls.Add(this.txtLog);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Padding = new System.Windows.Forms.Padding(6);
            this.panelLeft.Size = new System.Drawing.Size(371, 665);
            this.panelLeft.TabIndex = 0;
            // 
            // lblContainers
            // 
            this.lblContainers.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblContainers.Location = new System.Drawing.Point(6, 6);
            this.lblContainers.Name = "lblContainers";
            this.lblContainers.Size = new System.Drawing.Size(320, 18);
            this.lblContainers.TabIndex = 0;
            this.lblContainers.Text = "容器候选：";
            // 
            // dgvContainers
            // 
            this.dgvContainers.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvContainers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colContName,
            this.colContDx,
            this.colContDy,
            this.colContDz,
            this.colContWeight,
            this.colContMaxLoad});
            this.dgvContainers.Location = new System.Drawing.Point(6, 26);
            this.dgvContainers.Name = "dgvContainers";
            this.dgvContainers.RowHeadersVisible = false;
            this.dgvContainers.Size = new System.Drawing.Size(356, 95);
            this.dgvContainers.TabIndex = 1;
            this.dgvContainers.AllowUserToAddRows = true;
            this.dgvContainers.AllowUserToDeleteRows = true;
            // 
            // btnAddContainer
            // 
            this.btnAddContainer.Location = new System.Drawing.Point(6, 125);
            this.btnAddContainer.Name = "btnAddContainer";
            this.btnAddContainer.Size = new System.Drawing.Size(75, 23);
            this.btnAddContainer.TabIndex = 12;
            this.btnAddContainer.Text = "添加";
            this.btnAddContainer.UseVisualStyleBackColor = true;
            // 
            // btnRemoveContainer
            // 
            this.btnRemoveContainer.Location = new System.Drawing.Point(87, 125);
            this.btnRemoveContainer.Name = "btnRemoveContainer";
            this.btnRemoveContainer.Size = new System.Drawing.Size(75, 23);
            this.btnRemoveContainer.TabIndex = 13;
            this.btnRemoveContainer.Text = "删除";
            this.btnRemoveContainer.UseVisualStyleBackColor = true;
            // 
            // colContName
            // 
            this.colContName.FillWeight = 30F;
            this.colContName.HeaderText = "名称";
            this.colContName.Name = "colContName";
            // 
            // colContDx
            // 
            this.colContDx.FillWeight = 14F;
            this.colContDx.HeaderText = "长(Dx)";
            this.colContDx.Name = "colContDx";
            // 
            // colContDy
            // 
            this.colContDy.FillWeight = 14F;
            this.colContDy.HeaderText = "宽(Dy)";
            this.colContDy.Name = "colContDy";
            // 
            // colContDz
            // 
            this.colContDz.FillWeight = 14F;
            this.colContDz.HeaderText = "高(Dz)";
            this.colContDz.Name = "colContDz";
            // 
            // colContWeight
            // 
            this.colContWeight.FillWeight = 14F;
            this.colContWeight.HeaderText = "空重";
            this.colContWeight.Name = "colContWeight";
            // 
            // colContMaxLoad
            // 
            this.colContMaxLoad.FillWeight = 14F;
            this.colContMaxLoad.HeaderText = "最大载重";
            this.colContMaxLoad.Name = "colContMaxLoad";
            // 
            // lblItems
            // 
            this.lblItems.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblItems.Location = new System.Drawing.Point(6, 142);
            this.lblItems.Name = "lblItems";
            this.lblItems.Size = new System.Drawing.Size(320, 18);
            this.lblItems.TabIndex = 2;
            this.lblItems.Text = "待装物品：";
            // 
            // dgvItems
            // 
            this.dgvItems.AllowUserToAddRows = false;
            this.dgvItems.AllowUserToDeleteRows = false;
            this.dgvItems.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvItems.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colItemName,
            this.colItemDx,
            this.colItemDy,
            this.colItemDz});
            this.dgvItems.Location = new System.Drawing.Point(6, 162);
            this.dgvItems.Name = "dgvItems";
            this.dgvItems.ReadOnly = true;
            this.dgvItems.RowHeadersVisible = false;
            this.dgvItems.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvItems.Size = new System.Drawing.Size(320, 138);
            this.dgvItems.TabIndex = 3;
            // 
            // colItemName
            // 
            this.colItemName.FillWeight = 40F;
            this.colItemName.HeaderText = "名称";
            this.colItemName.Name = "colItemName";
            this.colItemName.ReadOnly = true;
            // 
            // colItemDx
            // 
            this.colItemDx.FillWeight = 20F;
            this.colItemDx.HeaderText = "长(Dx)";
            this.colItemDx.Name = "colItemDx";
            this.colItemDx.ReadOnly = true;
            // 
            // colItemDy
            // 
            this.colItemDy.FillWeight = 20F;
            this.colItemDy.HeaderText = "宽(Dy)";
            this.colItemDy.Name = "colItemDy";
            this.colItemDy.ReadOnly = true;
            // 
            // colItemDz
            // 
            this.colItemDz.FillWeight = 20F;
            this.colItemDz.HeaderText = "高(Dz)";
            this.colItemDz.Name = "colItemDz";
            this.colItemDz.ReadOnly = true;
            // 
            // grpRandomSelect
            // 
            this.grpRandomSelect.Controls.Add(this.lblRandomInfo);
            this.grpRandomSelect.Controls.Add(this.btnRandomSelect);
            this.grpRandomSelect.Controls.Add(this.numRandomMax);
            this.grpRandomSelect.Controls.Add(this.lblRandomMax);
            this.grpRandomSelect.Controls.Add(this.numRandomMin);
            this.grpRandomSelect.Controls.Add(this.lblRandomMin);
            this.grpRandomSelect.Location = new System.Drawing.Point(6, 306);
            this.grpRandomSelect.Name = "grpRandomSelect";
            this.grpRandomSelect.Size = new System.Drawing.Size(320, 75);
            this.grpRandomSelect.TabIndex = 4;
            this.grpRandomSelect.TabStop = false;
            this.grpRandomSelect.Text = "随机选择物品";
            // 
            // lblRandomInfo
            // 
            this.lblRandomInfo.AutoSize = true;
            this.lblRandomInfo.Location = new System.Drawing.Point(8, 48);
            this.lblRandomInfo.Name = "lblRandomInfo";
            this.lblRandomInfo.Size = new System.Drawing.Size(95, 12);
            this.lblRandomInfo.TabIndex = 5;
            this.lblRandomInfo.Text = "已加载 0 个物品";
            // 
            // btnRandomSelect
            // 
            this.btnRandomSelect.Location = new System.Drawing.Point(225, 15);
            this.btnRandomSelect.Name = "btnRandomSelect";
            this.btnRandomSelect.Size = new System.Drawing.Size(85, 23);
            this.btnRandomSelect.TabIndex = 4;
            this.btnRandomSelect.Text = "随机选择";
            this.btnRandomSelect.UseVisualStyleBackColor = true;
            // 
            // numRandomMax
            // 
            this.numRandomMax.Location = new System.Drawing.Point(155, 16);
            this.numRandomMax.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numRandomMax.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numRandomMax.Name = "numRandomMax";
            this.numRandomMax.Size = new System.Drawing.Size(60, 21);
            this.numRandomMax.TabIndex = 3;
            this.numRandomMax.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // lblRandomMax
            // 
            this.lblRandomMax.AutoSize = true;
            this.lblRandomMax.Location = new System.Drawing.Point(115, 20);
            this.lblRandomMax.Name = "lblRandomMax";
            this.lblRandomMax.Size = new System.Drawing.Size(41, 12);
            this.lblRandomMax.TabIndex = 2;
            this.lblRandomMax.Text = "最大：";
            // 
            // numRandomMin
            // 
            this.numRandomMin.Location = new System.Drawing.Point(48, 16);
            this.numRandomMin.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numRandomMin.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numRandomMin.Name = "numRandomMin";
            this.numRandomMin.Size = new System.Drawing.Size(60, 21);
            this.numRandomMin.TabIndex = 1;
            this.numRandomMin.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // lblRandomMin
            // 
            this.lblRandomMin.AutoSize = true;
            this.lblRandomMin.Location = new System.Drawing.Point(8, 20);
            this.lblRandomMin.Name = "lblRandomMin";
            this.lblRandomMin.Size = new System.Drawing.Size(41, 12);
            this.lblRandomMin.TabIndex = 0;
            this.lblRandomMin.Text = "最小：";
            // 
            // lblResults
            // 
            this.lblResults.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblResults.Location = new System.Drawing.Point(6, 387);
            this.lblResults.Name = "lblResults";
            this.lblResults.Size = new System.Drawing.Size(320, 18);
            this.lblResults.TabIndex = 5;
            this.lblResults.Text = "装箱结果：";
            // 
            // lstResults
            // 
            this.lstResults.IntegralHeight = false;
            this.lstResults.ItemHeight = 12;
            this.lstResults.Location = new System.Drawing.Point(6, 408);
            this.lstResults.Name = "lstResults";
            this.lstResults.Size = new System.Drawing.Size(320, 74);
            this.lstResults.TabIndex = 6;
            // 
            // lblStep
            // 
            this.lblStep.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblStep.Location = new System.Drawing.Point(6, 487);
            this.lblStep.Name = "lblStep";
            this.lblStep.Size = new System.Drawing.Size(320, 18);
            this.lblStep.TabIndex = 7;
            this.lblStep.Text = "步骤控制：";
            // 
            // trackStep
            // 
            this.trackStep.Location = new System.Drawing.Point(6, 507);
            this.trackStep.Maximum = 0;
            this.trackStep.Name = "trackStep";
            this.trackStep.Size = new System.Drawing.Size(235, 45);
            this.trackStep.TabIndex = 8;
            // 
            // lblStepInfo
            // 
            this.lblStepInfo.Location = new System.Drawing.Point(272, 507);
            this.lblStepInfo.Name = "lblStepInfo";
            this.lblStepInfo.Size = new System.Drawing.Size(60, 28);
            this.lblStepInfo.TabIndex = 9;
            this.lblStepInfo.Text = "0 / 0";
            this.lblStepInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // grpSelected
            // 
            this.grpSelected.Controls.Add(this.lblSelectedInfo);
            this.grpSelected.Location = new System.Drawing.Point(6, 579);
            this.grpSelected.Name = "grpSelected";
            this.grpSelected.Size = new System.Drawing.Size(320, 52);
            this.grpSelected.TabIndex = 10;
            this.grpSelected.TabStop = false;
            this.grpSelected.Text = "选中物品";
            // 
            // lblSelectedInfo
            // 
            this.lblSelectedInfo.Location = new System.Drawing.Point(6, 16);
            this.lblSelectedInfo.Name = "lblSelectedInfo";
            this.lblSelectedInfo.Size = new System.Drawing.Size(300, 33);
            this.lblSelectedInfo.TabIndex = 0;
            this.lblSelectedInfo.Text = "无";
            // 
            // txtLog
            // 
            this.txtLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(30)))), ((int)(((byte)(30)))));
            this.txtLog.Font = new System.Drawing.Font("Consolas", 8F);
            this.txtLog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
            this.txtLog.Location = new System.Drawing.Point(6, 640);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(320, 90);
            this.txtLog.TabIndex = 11;
            // 
            // glControl
            // 
            this.glControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(25)))), ((int)(((byte)(35)))));
            this.glControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControl.Location = new System.Drawing.Point(0, 0);
            this.glControl.Name = "glControl";
            this.glControl.Size = new System.Drawing.Size(882, 665);
            this.glControl.TabIndex = 0;
            this.glControl.VSync = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1257, 712);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "3D 装箱可视化";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.panelLeft.ResumeLayout(false);
            this.panelLeft.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvContainers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvItems)).EndInit();
            this.grpRandomSelect.ResumeLayout(false);
            this.grpRandomSelect.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numRandomMax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numRandomMin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackStep)).EndInit();
            this.grpSelected.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem menuFile;
        private System.Windows.Forms.ToolStripMenuItem menuOpenExcel;
        private System.Windows.Forms.ToolStripMenuItem menuExportJson;
        private System.Windows.Forms.ToolStripSeparator menuSep1;
        private System.Windows.Forms.ToolStripMenuItem menuExit;
        private System.Windows.Forms.ToolStripMenuItem menuRun;
        private System.Windows.Forms.ToolStripMenuItem menuStartPacking;

        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.ToolStripStatusLabel statusUtilization;
        private System.Windows.Forms.ToolStripStatusLabel statusTime;

        private System.Windows.Forms.SplitContainer splitMain;
        private System.Windows.Forms.Panel panelLeft;

        private System.Windows.Forms.Label lblContainers;
        private System.Windows.Forms.DataGridView dgvContainers;
        private System.Windows.Forms.DataGridViewTextBoxColumn colContName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colContDx;
        private System.Windows.Forms.DataGridViewTextBoxColumn colContDy;
        private System.Windows.Forms.DataGridViewTextBoxColumn colContDz;
        private System.Windows.Forms.DataGridViewTextBoxColumn colContWeight;
        private System.Windows.Forms.DataGridViewTextBoxColumn colContMaxLoad;
        private System.Windows.Forms.Button btnAddContainer;
        private System.Windows.Forms.Button btnRemoveContainer;

        private System.Windows.Forms.Label lblItems;
        private System.Windows.Forms.DataGridView dgvItems;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItemName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItemDx;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItemDy;
        private System.Windows.Forms.DataGridViewTextBoxColumn colItemDz;
        private System.Windows.Forms.GroupBox grpRandomSelect;
        private System.Windows.Forms.Label lblRandomMin;
        private System.Windows.Forms.NumericUpDown numRandomMin;
        private System.Windows.Forms.Label lblRandomMax;
        private System.Windows.Forms.NumericUpDown numRandomMax;
        private System.Windows.Forms.Button btnRandomSelect;
        private System.Windows.Forms.Label lblRandomInfo;

        private System.Windows.Forms.Label lblResults;
        private System.Windows.Forms.ListBox lstResults;

        private System.Windows.Forms.Label lblStep;
        private System.Windows.Forms.TrackBar trackStep;
        private System.Windows.Forms.Label lblStepInfo;

        private System.Windows.Forms.GroupBox grpSelected;
        private System.Windows.Forms.Label lblSelectedInfo;

        private System.Windows.Forms.TextBox txtLog;

        private OpenTK.GLControl glControl;
    }
}
