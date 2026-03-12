using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
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

        private List<ItemCandidate> _loadedItems = new List<ItemCandidate>();
        private List<Core.Models.Container> _packedContainers = new List<Core.Models.Container>();

        public MainForm()
        {
            InitializeComponent();
            WireEvents();
            AddDefaultContainer();
        }

        private void WireEvents()
        {
            this.Load += MainForm_Load;

            menuOpenExcel.Click += MenuOpenExcel_Click;
            menuExportJson.Click += MenuExportJson_Click;
            menuExit.Click += (s, e) => Close();
            menuStartPacking.Click += MenuStartPacking_Click;

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
            dgvContainers.Size = new Size(w, 120);
            y += 126;

            lblItems.Location = new Point(6, y);
            lblItems.Width = w;
            y += 22;

            dgvItems.Location = new Point(6, y);
            dgvItems.Size = new Size(w, 150);
            y += 156;

            lblResults.Location = new Point(6, y);
            lblResults.Width = w;
            y += 22;

            lstResults.Location = new Point(6, y);
            lstResults.Size = new Size(w, 80);
            y += 86;

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
            dgvContainers.Rows.Add("默认容器", "5900", "2330", "2390", "0", "28000");
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
                    _loadedItems = ExcelReader.ReadItems(dlg.FileName);
                    // Assign unique instance IDs
                    for (int i = 0; i < _loadedItems.Count; i++)
                    {
                        var old = _loadedItems[i];
                        _loadedItems[i] = new ItemCandidate(old.Name, old.Dx, old.Dy, old.Dz, i + 1);
                    }

                    RefreshItemsGrid();
                    menuStartPacking.Enabled = _loadedItems.Count > 0;
                    statusLabel.Text = $"已加载 {_loadedItems.Count} 个物品: {dlg.FileName}";
                    AppendLog($"加载 {_loadedItems.Count} 个物品自 {dlg.FileName}");
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
                trackStep.Maximum = 0;
                trackStep.Value = 0;
                lblStepInfo.Text = "0 / 0";
                glControl.Invalidate();
                return;
            }

            var container = _packedContainers[idx];
            _renderer.Container = container;
            _renderer.SelectedPlacement = null;
            lblSelectedInfo.Text = "无";

            int total = container.Stack?.Size ?? 0;
            trackStep.Maximum = total;
            trackStep.Value = total;
            _renderer.CurrentStep = total;
            lblStepInfo.Text = $"{total} / {total}";

            // Compute utilization for this container
            long cv = container.MaxLoadVolume;
            long pv = container.Stack?.GetVolume() ?? 0;
            double util = cv > 0 ? (double)pv / cv * 100 : 0;
            statusUtilization.Text = $"体积利用率: {util:F1}% ({pv}/{cv})";

            // Fit camera to scene
            float sceneSize = Math.Max(container.LoadDx, Math.Max(container.LoadDy, container.LoadDz));
            _camera.FitToScene(sceneSize);

            glControl.Invalidate();
        }

        private void TrackStep_ValueChanged(object sender, EventArgs e)
        {
            int total = trackStep.Maximum;
            int val = trackStep.Value;
            _renderer.CurrentStep = val;
            lblStepInfo.Text = val == 0 ? $"全部 / {total}" : $"{val} / {total}";
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

            int maxStep = _renderer.CurrentStep > 0 ? _renderer.CurrentStep : int.MaxValue;
            var hit = HitTester.FindHit(rayOrigin, rayDir, _renderer.Container, maxStep);

            _renderer.SelectedPlacement = hit;
            if (hit != null)
            {
                string boxId = hit.StackValue.Box?.Id ?? "?";
                lblSelectedInfo.Text = $"{boxId}\n位置: ({hit.X},{hit.Y},{hit.Z}) 尺寸: {hit.StackValue.Dx}x{hit.StackValue.Dy}x{hit.StackValue.Dz}";
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

        #endregion
    }
}
