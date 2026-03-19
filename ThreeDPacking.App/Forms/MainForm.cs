using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using ThreeDPacking.App.Rendering;
using ThreeDPacking.Core.IO;
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

        public MainForm()
        {
            InitializeComponent();
            WireEvents();
            AddDefaultContainer();
            btnRandomSelect.Enabled = false;
        }

        private void WireEvents()
        {
            this.Load += MainForm_Load;

            menuOpenExcel.Click += MenuOpenExcel_Click;
            menuExportJson.Click += MenuExportJson_Click;
            menuExit.Click += (s, e) => Close();
            menuStartPacking.Click += MenuStartPacking_Click;
            btnRandomSelect.Click += BtnRandomSelect_Click;
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

            this.Resize += (s, e) => LayoutLeftPanel();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LayoutLeftPanel();
        }

        private void LayoutLeftPanel()
        {
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
            grpRandomSelect.Size = new Size(w, 75);
            lblRandomMin.Location = new Point(8, 20);
            numRandomMin.Location = new Point(48, 16);
            lblRandomMax.Location = new Point(115, 20);
            numRandomMax.Location = new Point(155, 16);
            btnRandomSelect.Location = new Point(w - 90, 15);
            btnRandomSelect.Size = new Size(85, 23);
            lblRandomInfo.Location = new Point(8, 48);
            y += 81;

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
            grpSelected.Size = new Size(w, 55);
            lblSelectedInfo.Location = new Point(8, 18);
            lblSelectedInfo.Size = new Size(w - 16, 32);
            y += 61;

            txtLog.Location = new Point(6, y);
            int logHeight = Math.Max(80, panelLeft.ClientSize.Height - y - 6);
            txtLog.Size = new Size(w, logHeight);
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
                    _allLoadedItems = ExcelReader.ReadItems(dlg.FileName);
                    // Assign unique instance IDs
                    for (int i = 0; i < _allLoadedItems.Count; i++)
                    {
                        var old = _allLoadedItems[i];
                        _allLoadedItems[i] = new ItemCandidate(old.Name, old.Dx, old.Dy, old.Dz, i + 1);
                    }

                    // 默认加载所有物品
                    _loadedItems = new List<ItemCandidate>(_allLoadedItems);

                    RefreshItemsGrid();
                    menuStartPacking.Enabled = _loadedItems.Count > 0;
                    btnRandomSelect.Enabled = _allLoadedItems.Count > 0;
                    lblRandomInfo.Text = $"已加载 {_allLoadedItems.Count} 个物品";
                    statusLabel.Text = $"已加载 {_allLoadedItems.Count} 个物品: {dlg.FileName}";
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
            targetCount = Math.Min(targetCount, _allLoadedItems.Count);

            // 随机选择物品
            var shuffled = new List<ItemCandidate>(_allLoadedItems);
            ShuffleList(shuffled, random);
            _loadedItems = shuffled.Take(targetCount).ToList();

            // 重新分配InstanceId
            for (int i = 0; i < _loadedItems.Count; i++)
            {
                var old = _loadedItems[i];
                _loadedItems[i] = new ItemCandidate(old.Name, old.Dx, old.Dy, old.Dz, i + 1);
            }

            RefreshItemsGrid();
            menuStartPacking.Enabled = _loadedItems.Count > 0;
            statusLabel.Text = $"已随机选择 {_loadedItems.Count} 个物品 (范围: {minCount}-{maxCount})";
            AppendLog($"随机选择了 {_loadedItems.Count} 个物品进行装箱");
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

        private void MenuStartPacking_Click(object sender, EventArgs e)
        {
            if (_loadedItems.Count == 0)
            {
                MessageBox.Show("请先加载物品Excel文件。", "提示",
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
            menuStartPacking.Enabled = false;
            menuOpenExcel.Enabled = false;
            statusLabel.Text = "正在装箱，请稍候...";
            AppendLog("========== 开始装箱 ==========");

            var itemsCopy = new List<ItemCandidate>(_loadedItems);
            var sw = Stopwatch.StartNew();

            var worker = new BackgroundWorker();
            worker.DoWork += (ws, we) =>
            {
                var orchestrator = new PackingOrchestrator();
                we.Result = orchestrator.Run(itemsCopy, containerCandidates, DateTime.Now.Ticks,
                    msg => BeginInvoke((Action)(() => AppendLog(msg))));
            };
            worker.RunWorkerCompleted += (ws, we) =>
            {
                sw.Stop();
                menuStartPacking.Enabled = true;
                menuOpenExcel.Enabled = true;

                if (we.Error != null)
                {
                    statusLabel.Text = "装箱失败";
                    MessageBox.Show("装箱出错:\n" + we.Error.Message, "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _packedContainers = (List<Container>)we.Result;
                menuExportJson.Enabled = _packedContainers.Count > 0;

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

                if (lstResults.Items.Count > 0)
                    lstResults.SelectedIndex = 0;
            };
            worker.RunWorkerAsync();
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

                string name = (row.Cells[0].Value ?? "").ToString().Trim();
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
                lblSelectedInfo.Text = $"{itemName}\n位置: ({hit.X},{hit.Y},{hit.Z}) 尺寸: {hit.StackValue.Dx}x{hit.StackValue.Dy}x{hit.StackValue.Dz}";
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
