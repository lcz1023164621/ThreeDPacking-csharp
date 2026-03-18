using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using ThreeDPacking.Core.Models;

namespace ThreeDPacking.App.Rendering
{
    /// <summary>
    /// 装箱结果 3D 渲染器核心，使用 OpenGL 绘制箱子和物品
    /// </summary>
    public class PackingRenderer
    {
        private Container _container;
        private List<Container> _containers = new List<Container>();
        private int _currentStep;
        private Placement _selectedPlacement;

        public Container Container
        {
            get => _container;
            set => _container = value;
        }

        public List<Container> Containers
        {
            get => _containers;
            set => _containers = value ?? new List<Container>();
        }

        public int CurrentStep
        {
            get => _currentStep;
            set => _currentStep = value;
        }

        public int TotalSteps => _container?.Stack?.Size ?? 0;

        public Placement SelectedPlacement
        {
            get => _selectedPlacement;
            set => _selectedPlacement = value;
        }

        public void Render(Matrix4 projection, Matrix4 view)
        {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref view);

            DrawGrid();
            DrawAxes();

            // Render multiple containers side by side
            if (_containers.Count > 0)
            {
                float offsetX = 0;
                foreach (var container in _containers)
                {
                    GL.PushMatrix();
                    GL.Translate(offsetX, 0, 0);
                    DrawContainerWireframe(container);
                    DrawPlacements(container);
                    GL.PopMatrix();
                    
                    // Move next container to the right with some spacing
                    offsetX += container.LoadDx + 150; // 150 units spacing between containers
                }
            }
            else if (_container != null)
            {
                // Single container mode (backward compatibility)
                DrawContainerWireframe(_container);
                DrawPlacements(_container);
            }
        }

        private void DrawGrid()
        {
            GL.Begin(PrimitiveType.Lines);
            GL.Color4(0.3f, 0.3f, 0.4f, 0.5f);
            float gridSize = 1000;
            float step = 50;
            for (float i = -gridSize; i <= gridSize; i += step)
            {
                GL.Vertex3(i, 0, -gridSize);
                GL.Vertex3(i, 0, gridSize);
                GL.Vertex3(-gridSize, 0, i);
                GL.Vertex3(gridSize, 0, i);
            }
            GL.End();
        }

        private void DrawAxes()
        {
            GL.LineWidth(2f);
            GL.Begin(PrimitiveType.Lines);
            // X axis - Red
            GL.Color3(1f, 0f, 0f);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(100, 0, 0);
            // Y axis - Green
            GL.Color3(0f, 1f, 0f);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 100, 0);
            // Z axis - Blue
            GL.Color3(0f, 0f, 1f);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, 100);
            GL.End();
            GL.LineWidth(1f);
        }

        private void DrawContainerWireframe(Container c)
        {
            float x1 = 0, y1 = 0, z1 = 0;
            float x2 = c.LoadDx, y2 = c.LoadDz, z2 = c.LoadDy;

            // Draw white border lines for better visibility
            GL.LineWidth(3f);
            GL.Begin(PrimitiveType.Lines);
            GL.Color4(1.0f, 1.0f, 1.0f, 1.0f); // White border

            // Bottom face
            GL.Vertex3(x1, y1, z1); GL.Vertex3(x2, y1, z1);
            GL.Vertex3(x2, y1, z1); GL.Vertex3(x2, y1, z2);
            GL.Vertex3(x2, y1, z2); GL.Vertex3(x1, y1, z2);
            GL.Vertex3(x1, y1, z2); GL.Vertex3(x1, y1, z1);
            // Top face
            GL.Vertex3(x1, y2, z1); GL.Vertex3(x2, y2, z1);
            GL.Vertex3(x2, y2, z1); GL.Vertex3(x2, y2, z2);
            GL.Vertex3(x2, y2, z2); GL.Vertex3(x1, y2, z2);
            GL.Vertex3(x1, y2, z2); GL.Vertex3(x1, y2, z1);
            // Verticals
            GL.Vertex3(x1, y1, z1); GL.Vertex3(x1, y2, z1);
            GL.Vertex3(x2, y1, z1); GL.Vertex3(x2, y2, z1);
            GL.Vertex3(x2, y1, z2); GL.Vertex3(x2, y2, z2);
            GL.Vertex3(x1, y1, z2); GL.Vertex3(x1, y2, z2);

            GL.End();
            GL.LineWidth(1f);
        }

        private void DrawPlacements(Container container)
        {
            if (container.Stack == null) return;

            // 分离物品和填充纸
            var items = new List<Placement>();
            var paddings = new List<Placement>();
            
            foreach (var p in container.Stack.Placements)
            {
                if (p.IsPadding)
                    paddings.Add(p);
                else
                    items.Add(p);
            }
            
            // 调试输出
            System.Diagnostics.Debug.WriteLine($"[Renderer] Container: {container.Id}, Total Placements: {container.Stack.Placements.Count}, Items: {items.Count}, Paddings: {paddings.Count}");

            // 按步骤显示物品（_currentStep 控制物品显示数量，但不超过当前容器的物品数）
            int itemCount = items.Count;
            // _currentStep 是全局步骤值，每个容器显示 min(_currentStep, 该容器物品数)
            int displayCount = (_currentStep == 0) ? itemCount : Math.Min(_currentStep, itemCount);
            
            System.Diagnostics.Debug.WriteLine($"[Renderer] CurrentStep: {_currentStep}, ItemCount: {itemCount}, DisplayCount: {displayCount}");
            
            for (int i = 0; i < displayCount; i++)
            {
                var p = items[i];
                bool isSelected = p == _selectedPlacement;
                DrawBox(p, isSelected);
            }

            // 当全局步骤值达到或超过该容器的物品总数时，才显示该容器的填充纸
            // _currentStep == 0 表示显示全部，此时也应该显示填充纸
            if (_currentStep == 0 || _currentStep >= itemCount)
            {
                System.Diagnostics.Debug.WriteLine($"[Renderer] Drawing {paddings.Count} padding papers (CurrentStep: {_currentStep}, ItemCount: {itemCount})");
                foreach (var p in paddings)
                {
                    bool isSelected = p == _selectedPlacement;
                    System.Diagnostics.Debug.WriteLine($"[Renderer] Drawing padding at ({p.X},{p.Y},{p.Z}) size {p.StackValue.Dx}x{p.StackValue.Dy}x{p.StackValue.Dz}");
                    DrawBox(p, isSelected);
                }
            }
        }

        private void DrawBox(Placement p, bool isSelected)
        {
            // 检查是否为填充纸
            if (p.IsPadding)
            {
                DrawPaddingPaper(p, isSelected);
                return;
            }

            string id = p.StackValue.Box?.Id ?? "";
            Color color = ColorHelper.GetColor(id);

            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;
            float a = isSelected ? 0.9f : 0.6f;

            // Map: X->X, Z->Y(height), Y->Z(depth) for OpenGL
            // Note: AbsoluteEndX = X + Dx - 1, so we need to add 1 to get the actual end coordinate
            float x1 = p.X;
            float y1 = p.Z;          // Z maps to vertical
            float z1 = p.Y;
            float x2 = p.AbsoluteEndX + 1;
            float y2 = p.AbsoluteEndZ + 1;
            float z2 = p.AbsoluteEndY + 1;

            // Draw filled faces
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Begin(PrimitiveType.Quads);
            GL.Color4(r, g, b, a);

            // Front
            GL.Vertex3(x1, y1, z1); GL.Vertex3(x2, y1, z1);
            GL.Vertex3(x2, y2, z1); GL.Vertex3(x1, y2, z1);
            // Back
            GL.Vertex3(x1, y1, z2); GL.Vertex3(x2, y1, z2);
            GL.Vertex3(x2, y2, z2); GL.Vertex3(x1, y2, z2);
            // Left
            GL.Vertex3(x1, y1, z1); GL.Vertex3(x1, y1, z2);
            GL.Vertex3(x1, y2, z2); GL.Vertex3(x1, y2, z1);
            // Right
            GL.Vertex3(x2, y1, z1); GL.Vertex3(x2, y1, z2);
            GL.Vertex3(x2, y2, z2); GL.Vertex3(x2, y2, z1);
            // Bottom
            GL.Vertex3(x1, y1, z1); GL.Vertex3(x2, y1, z1);
            GL.Vertex3(x2, y1, z2); GL.Vertex3(x1, y1, z2);
            // Top
            GL.Vertex3(x1, y2, z1); GL.Vertex3(x2, y2, z1);
            GL.Vertex3(x2, y2, z2); GL.Vertex3(x1, y2, z2);

            GL.End();

            // Draw edges
            float edgeR = isSelected ? 1f : r * 0.5f;
            float edgeG = isSelected ? 1f : g * 0.5f;
            float edgeB = isSelected ? 0f : b * 0.5f;
            GL.LineWidth(isSelected ? 3f : 1f);
            GL.Begin(PrimitiveType.Lines);
            GL.Color4(edgeR, edgeG, edgeB, 1f);

            // Bottom
            GL.Vertex3(x1, y1, z1); GL.Vertex3(x2, y1, z1);
            GL.Vertex3(x2, y1, z1); GL.Vertex3(x2, y1, z2);
            GL.Vertex3(x2, y1, z2); GL.Vertex3(x1, y1, z2);
            GL.Vertex3(x1, y1, z2); GL.Vertex3(x1, y1, z1);
            // Top
            GL.Vertex3(x1, y2, z1); GL.Vertex3(x2, y2, z1);
            GL.Vertex3(x2, y2, z1); GL.Vertex3(x2, y2, z2);
            GL.Vertex3(x2, y2, z2); GL.Vertex3(x1, y2, z2);
            GL.Vertex3(x1, y2, z2); GL.Vertex3(x1, y2, z1);
            // Verticals
            GL.Vertex3(x1, y1, z1); GL.Vertex3(x1, y2, z1);
            GL.Vertex3(x2, y1, z1); GL.Vertex3(x2, y2, z1);
            GL.Vertex3(x2, y1, z2); GL.Vertex3(x2, y2, z2);
            GL.Vertex3(x1, y1, z2); GL.Vertex3(x1, y2, z2);

            GL.End();
            GL.LineWidth(1f);
        }

        /// <summary>
        /// 绘制填充纸（牛皮纸）- 所有面都有褶纹效果
        /// </summary>
        private void DrawPaddingPaper(Placement p, bool isSelected)
        {
            // 牛皮纸颜色 (RGB: 180, 140, 90) - 棕色/土黄色
            float baseR = 0.71f;  // 180/255
            float baseG = 0.55f;  // 140/255
            float baseB = 0.35f;  // 90/255
            float a = isSelected ? 0.95f : 0.85f;

            // Map: X->X, Z->Y(height), Y->Z(depth) for OpenGL
            float x1 = p.X;
            float y1 = p.Z;          // Z maps to vertical (bottom)
            float z1 = p.Y;
            float x2 = p.AbsoluteEndX + 1;
            float y2 = p.AbsoluteEndZ + 1;  // top
            float z2 = p.AbsoluteEndY + 1;

            float width = x2 - x1;   // X方向宽度
            float height = y2 - y1;  // Y方向高度 (OpenGL中的垂直方向)
            float depth = z2 - z1;   // Z方向深度

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // 褶纹参数 - 增大振幅和对比度
            int numFoldsX = Math.Max(4, (int)(width / 15));   // X方向褶纹数，更密集
            int numFoldsY = Math.Max(4, (int)(height / 15));  // Y方向褶纹数
            int numFoldsZ = Math.Max(4, (int)(depth / 15));   // Z方向褶纹数
            float foldAmp = 5.0f;  // 褶纹幅度，增大以更明显

            // ===== 1. 绘制底面 (Y = y1) - XZ平面褶纹 =====
            DrawWrinkledFace(
                x1, y1, z1, x2, y1, z2,
                numFoldsX, numFoldsZ, foldAmp,
                baseR * 0.75f, baseG * 0.75f, baseB * 0.75f, a,
                FaceOrientation.Bottom);

            // ===== 2. 绘制顶面 (Y = y2) - XZ平面褶纹 =====
            DrawWrinkledFace(
                x1, y2, z1, x2, y2, z2,
                numFoldsX, numFoldsZ, foldAmp,
                baseR, baseG, baseB, a,
                FaceOrientation.Top);

            // ===== 3. 绘制前面 (Z = z1) - XY平面褶纹 =====
            DrawWrinkledFace(
                x1, y1, z1, x2, y2, z1,
                numFoldsX, numFoldsY, foldAmp,
                baseR * 0.85f, baseG * 0.85f, baseB * 0.85f, a,
                FaceOrientation.Front);

            // ===== 4. 绘制后面 (Z = z2) - XY平面褶纹 =====
            DrawWrinkledFace(
                x1, y1, z2, x2, y2, z2,
                numFoldsX, numFoldsY, foldAmp,
                baseR * 0.80f, baseG * 0.80f, baseB * 0.80f, a,
                FaceOrientation.Back);

            // ===== 5. 绘制左面 (X = x1) - YZ平面褶纹 =====
            DrawWrinkledFace(
                x1, y1, z1, x1, y2, z2,
                numFoldsZ, numFoldsY, foldAmp,
                baseR * 0.90f, baseG * 0.90f, baseB * 0.90f, a,
                FaceOrientation.Left);

            // ===== 6. 绘制右面 (X = x2) - YZ平面褶纹 =====
            DrawWrinkledFace(
                x2, y1, z1, x2, y2, z2,
                numFoldsZ, numFoldsY, foldAmp,
                baseR * 0.85f, baseG * 0.85f, baseB * 0.85f, a,
                FaceOrientation.Right);

            // 绘制边框
            float edgeR = isSelected ? 1f : baseR * 0.5f;
            float edgeG = isSelected ? 1f : baseG * 0.5f;
            float edgeB = isSelected ? 0f : baseB * 0.5f;
            GL.LineWidth(isSelected ? 3f : 2f);
            GL.Begin(PrimitiveType.Lines);
            GL.Color4(edgeR, edgeG, edgeB, 1f);

            // 底部边框
            GL.Vertex3(x1, y1, z1); GL.Vertex3(x2, y1, z1);
            GL.Vertex3(x2, y1, z1); GL.Vertex3(x2, y1, z2);
            GL.Vertex3(x2, y1, z2); GL.Vertex3(x1, y1, z2);
            GL.Vertex3(x1, y1, z2); GL.Vertex3(x1, y1, z1);
            // 顶部边框
            GL.Vertex3(x1, y2, z1); GL.Vertex3(x2, y2, z1);
            GL.Vertex3(x2, y2, z1); GL.Vertex3(x2, y2, z2);
            GL.Vertex3(x2, y2, z2); GL.Vertex3(x1, y2, z2);
            GL.Vertex3(x1, y2, z2); GL.Vertex3(x1, y2, z1);
            // 垂直边缘
            GL.Vertex3(x1, y1, z1); GL.Vertex3(x1, y2, z1);
            GL.Vertex3(x2, y1, z1); GL.Vertex3(x2, y2, z1);
            GL.Vertex3(x2, y1, z2); GL.Vertex3(x2, y2, z2);
            GL.Vertex3(x1, y1, z2); GL.Vertex3(x1, y2, z2);

            GL.End();
            GL.LineWidth(1f);
        }

        /// <summary>
        /// 面的方向枚举
        /// </summary>
        private enum FaceOrientation { Top, Bottom, Front, Back, Left, Right }

        /// <summary>
        /// 绘制带褶纹的面
        /// </summary>
        private void DrawWrinkledFace(
            float x1, float y1, float z1,
            float x2, float y2, float z2,
            int foldsU, int foldsV, float amplitude,
            float r, float g, float b, float a,
            FaceOrientation orientation)
        {
            int segmentsU = foldsU * 2;
            int segmentsV = foldsV * 2;

            for (int i = 0; i < segmentsU; i++)
            {
                float u0 = (float)i / segmentsU;
                float u1 = (float)(i + 1) / segmentsU;

                GL.Begin(PrimitiveType.TriangleStrip);
                for (int j = 0; j <= segmentsV; j++)
                {
                    float v = (float)j / segmentsV;

                    // 计算褶纹偏移
                    float wrinkle0 = (float)(Math.Sin(u0 * foldsU * Math.PI) * Math.Sin(v * foldsV * Math.PI)) * amplitude;
                    float wrinkle1 = (float)(Math.Sin(u1 * foldsU * Math.PI) * Math.Sin(v * foldsV * Math.PI)) * amplitude;

                    // 颜色变化模拟光影 - 增大对比度
                    float shade0 = 0.6f + 0.4f * (float)Math.Abs(Math.Sin(u0 * foldsU * Math.PI) * Math.Sin(v * foldsV * Math.PI));
                    float shade1 = 0.6f + 0.4f * (float)Math.Abs(Math.Sin(u1 * foldsU * Math.PI) * Math.Sin(v * foldsV * Math.PI));

                    Vector3 p0, p1;

                    switch (orientation)
                    {
                        case FaceOrientation.Top:
                        case FaceOrientation.Bottom:
                            // XZ平面，Y方向褶纹
                            p0 = new Vector3(
                                x1 + u0 * (x2 - x1),
                                y1 + wrinkle0,
                                z1 + v * (z2 - z1));
                            p1 = new Vector3(
                                x1 + u1 * (x2 - x1),
                                y1 + wrinkle1,
                                z1 + v * (z2 - z1));
                            break;

                        case FaceOrientation.Front:
                        case FaceOrientation.Back:
                            // XY平面，Z方向褶纹
                            p0 = new Vector3(
                                x1 + u0 * (x2 - x1),
                                y1 + v * (y2 - y1),
                                z1 + wrinkle0);
                            p1 = new Vector3(
                                x1 + u1 * (x2 - x1),
                                y1 + v * (y2 - y1),
                                z1 + wrinkle1);
                            break;

                        case FaceOrientation.Left:
                        case FaceOrientation.Right:
                        default:
                            // YZ平面，X方向褶纹
                            p0 = new Vector3(
                                x1 + wrinkle0,
                                y1 + v * (y2 - y1),
                                z1 + u0 * (z2 - z1));
                            p1 = new Vector3(
                                x1 + wrinkle1,
                                y1 + v * (y2 - y1),
                                z1 + u1 * (z2 - z1));
                            break;
                    }

                    GL.Color4(r * shade0, g * shade0, b * shade0, a);
                    GL.Vertex3(p0.X, p0.Y, p0.Z);
                    GL.Color4(r * shade1, g * shade1, b * shade1, a);
                    GL.Vertex3(p1.X, p1.Y, p1.Z);
                }
                GL.End();
            }

            // 绘制褶纹线条
            GL.LineWidth(1f);
            GL.Begin(PrimitiveType.Lines);
            GL.Color4(r * 0.6f, g * 0.6f, b * 0.6f, a);

            // U方向线条
            for (int i = 0; i <= segmentsU; i += 2)
            {
                float u = (float)i / segmentsU;
                for (int j = 0; j < segmentsV; j++)
                {
                    float v0 = (float)j / segmentsV;
                    float v1 = (float)(j + 1) / segmentsV;

                    float w0 = (float)(Math.Sin(u * foldsU * Math.PI) * Math.Sin(v0 * foldsV * Math.PI)) * amplitude;
                    float w1 = (float)(Math.Sin(u * foldsU * Math.PI) * Math.Sin(v1 * foldsV * Math.PI)) * amplitude;

                    Vector3 p0, p1;
                    switch (orientation)
                    {
                        case FaceOrientation.Top:
                        case FaceOrientation.Bottom:
                            p0 = new Vector3(x1 + u * (x2 - x1), y1 + w0, z1 + v0 * (z2 - z1));
                            p1 = new Vector3(x1 + u * (x2 - x1), y1 + w1, z1 + v1 * (z2 - z1));
                            break;
                        case FaceOrientation.Front:
                        case FaceOrientation.Back:
                            p0 = new Vector3(x1 + u * (x2 - x1), y1 + v0 * (y2 - y1), z1 + w0);
                            p1 = new Vector3(x1 + u * (x2 - x1), y1 + v1 * (y2 - y1), z1 + w1);
                            break;
                        default:
                            p0 = new Vector3(x1 + w0, y1 + v0 * (y2 - y1), z1 + u * (z2 - z1));
                            p1 = new Vector3(x1 + w1, y1 + v1 * (y2 - y1), z1 + u * (z2 - z1));
                            break;
                    }
                    GL.Vertex3(p0.X, p0.Y, p0.Z);
                    GL.Vertex3(p1.X, p1.Y, p1.Z);
                }
            }
            GL.End();
        }
    }
}
