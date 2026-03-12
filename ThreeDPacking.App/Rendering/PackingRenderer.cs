using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using ThreeDPacking.Core.Models;

namespace ThreeDPacking.App.Rendering
{
    /// <summary>
    /// Renders packed containers using OpenGL immediate mode.
    /// Draws container wireframes, colored semi-transparent boxes, grid, and axes.
    /// </summary>
    public class PackingRenderer
    {
        private Container _container;
        private int _currentStep;
        private Placement _selectedPlacement;

        public Container Container
        {
            get => _container;
            set => _container = value;
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

            if (_container != null)
            {
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

            GL.LineWidth(2f);
            GL.Begin(PrimitiveType.Lines);
            GL.Color4(1f, 1f, 1f, 0.8f);

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

            int step = 0;
            foreach (var p in container.Stack.Placements)
            {
                step++;
                if (step > _currentStep && _currentStep > 0)
                    break;

                bool isSelected = p == _selectedPlacement;
                DrawBox(p, isSelected);
            }
        }

        private void DrawBox(Placement p, bool isSelected)
        {
            string id = p.StackValue.Box?.Id ?? "";
            Color color = ColorHelper.GetColor(id);

            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;
            float a = isSelected ? 0.9f : 0.6f;

            // Map: X->X, Z->Y(height), Y->Z(depth) for OpenGL
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
    }
}
