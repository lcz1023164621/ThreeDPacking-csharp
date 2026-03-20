using System;
using System.Collections.Generic;
using ThreeDPacking.Core.Models;

namespace ThreeDPacking.Core.Points
{
    /// <summary>
    /// 二维极端点计算，只在当前层地板上放置箱子，箱子之间不允许浮空，必须有完整支撑
    /// 实现方法：Z 方向不自由切割，只允许抬高一层（new level）
    /// 每个“层”都视为一个独立的 2D 装箱问题
    /// 放置时强制要求箱子底面四个角 + 中心点都被支撑
    /// </summary>
    public class PointCalculator2D : IPointCalculator
    {
        private readonly List<ExtremePoint> _points = new List<ExtremePoint>();
        private readonly List<Placement> _placements = new List<Placement>();
        private int _containerMaxX;
        private int _containerMaxY;
        private int _containerMaxZ;
        private long _minAreaLimit;
        private long _minVolumeLimit;

        public int PointCount => _points.Count;
        public bool IsEmpty => _points.Count == 0;

        public void ClearToSize(int dx, int dy, int dz)
        {
            _containerMaxX = dx - 1;
            _containerMaxY = dy - 1;
            _containerMaxZ = dz - 1;
            _points.Clear();
            _placements.Clear();
            _minAreaLimit = 0;
            _minVolumeLimit = 0;
            // 初始点：整个容器底面空间
            _points.Add(new ExtremePoint(0, 0, 0, _containerMaxX, _containerMaxY, _containerMaxZ));
        }

        public ExtremePoint GetPoint(int index)
        {
            return _points[index];
        }

        public IList<ExtremePoint> GetAllPoints()
        {
            return _points;
        }

        public long GetMaxArea()
        {
            long max = 0;
            foreach (var p in _points)
            {
                long a = p.Area;
                if (a > max) max = a;
            }
            return max;
        }

        public long GetMaxVolume()
        {
            long max = 0;
            foreach (var p in _points)
            {
                long v = p.Volume;
                if (v > max) max = v;
            }
            return max;
        }

        public void SetMinimumAreaAndVolumeLimit(long minArea, long minVolume)
        {
            _minAreaLimit = minArea;
            _minVolumeLimit = minVolume;
            // 移除尺寸过小的极值点
            for (int i = _points.Count - 1; i >= 0; i--)
            {
                var p = _points[i];
                if (p.Area < minArea || p.Volume < minVolume)
                    _points.RemoveAt(i);
            }
        }

        /// <summary>
        /// 添加一个放置结果并更新极值点。
        /// 使用 2D 极值点算法：当箱子放在某个点后，
        /// 在该箱体边缘生成新的候选极值点。
        /// </summary>
        public int Add(int pointIndex, Placement placement)
        {
            _placements.Add(placement);

            // 移除已使用的极值点
            _points.RemoveAt(pointIndex);

            int px = placement.AbsoluteX;
            int py = placement.AbsoluteY;
            int pz = placement.AbsoluteZ;
            int endX = placement.AbsoluteEndX;
            int endY = placement.AbsoluteEndY;
            int endZ = placement.AbsoluteEndZ;

            // 在箱体边缘生成新的极值点
            // 点1：放置箱体右侧（X 轴方向）
            if (endX + 1 <= _containerMaxX)
            {
                var rightPoint = new ExtremePoint(
                    endX + 1, py, pz,
                    _containerMaxX, _containerMaxY, _containerMaxZ);
                if (IsValidPoint(rightPoint))
                    AddPointIfNotEclipsed(rightPoint);
            }

            // 点2：放置箱体前侧（Y 轴方向）
            if (endY + 1 <= _containerMaxY)
            {
                var frontPoint = new ExtremePoint(
                    px, endY + 1, pz,
                    _containerMaxX, _containerMaxY, _containerMaxZ);
                if (IsValidPoint(frontPoint))
                    AddPointIfNotEclipsed(frontPoint);
            }

            // 对与放置区域重叠的已有极值点执行约束裁剪
            ConstrainPoints(placement);

            // 删除被包含（被遮蔽）的极值点
            RemoveEclipsedPoints();

            return pointIndex;
        }

        /// <summary>
        /// 设置新的极值点集合（用于 LAFF 开新层）。
        /// </summary>
        public void SetPoints(IList<ExtremePoint> newPoints)
        {
            _points.Clear();
            _points.AddRange(newPoints);
        }

        /// <summary>
        /// 清空已放置记录但保留极值点（用于层切换）。
        /// </summary>
        public void Clear()
        {
            _placements.Clear();
        }

        private bool IsValidPoint(ExtremePoint point)
        {
            if (point.Dx <= 0 || point.Dy <= 0 || point.Dz <= 0) return false;
            if (_minAreaLimit > 0 && point.Area < _minAreaLimit) return false;
            if (_minVolumeLimit > 0 && point.Volume < _minVolumeLimit) return false;
            return true;
        }

        private void AddPointIfNotEclipsed(ExtremePoint newPoint)
        {
            // 若已有点完全包含新点，则无需添加
            foreach (var existing in _points)
            {
                if (existing.Eclipses(newPoint))
                    return;
            }
            _points.Add(newPoint);
        }

        private void ConstrainPoints(Placement placement)
        {
            int endX = placement.AbsoluteEndX;
            int endY = placement.AbsoluteEndY;

            for (int i = _points.Count - 1; i >= 0; i--)
            {
                var p = _points[i];

                // 检查该点与放置区域在 2D 平面是否重叠
                bool overlapX = p.MinX <= endX && p.MaxX >= placement.AbsoluteX;
                bool overlapY = p.MinY <= endY && p.MaxY >= placement.AbsoluteY;

                if (overlapX && overlapY)
                {
                    // 点位于箱体内部或与箱体发生重叠
                    if (p.MinX >= placement.AbsoluteX && p.MinY >= placement.AbsoluteY)
                    {
                        // 点被完全吞没，直接移除
                        _points.RemoveAt(i);
                    }
                    else
                    {
                        // 对该点做约束裁剪
                        bool needsConstrainX = p.MinX < placement.AbsoluteX && p.MaxX >= placement.AbsoluteX;
                        bool needsConstrainY = p.MinY < placement.AbsoluteY && p.MaxY >= placement.AbsoluteY;

                        if (needsConstrainX && needsConstrainY)
                        {
                            // 分裂：生成 X 约束版本与 Y 约束版本
                            var constrainedX = new ExtremePoint(
                                p.MinX, p.MinY, p.MinZ,
                                placement.AbsoluteX - 1, p.MaxY, p.MaxZ);
                            var constrainedY = new ExtremePoint(
                                p.MinX, p.MinY, p.MinZ,
                                p.MaxX, placement.AbsoluteY - 1, p.MaxZ);

                            _points.RemoveAt(i);
                            if (IsValidPoint(constrainedX))
                                _points.Add(constrainedX);
                            if (IsValidPoint(constrainedY))
                                _points.Add(constrainedY);
                        }
                        else if (needsConstrainX)
                        {
                            _points[i] = new ExtremePoint(
                                p.MinX, p.MinY, p.MinZ,
                                placement.AbsoluteX - 1, p.MaxY, p.MaxZ);
                            if (!IsValidPoint(_points[i]))
                                _points.RemoveAt(i);
                        }
                        else if (needsConstrainY)
                        {
                            _points[i] = new ExtremePoint(
                                p.MinX, p.MinY, p.MinZ,
                                p.MaxX, placement.AbsoluteY - 1, p.MaxZ);
                            if (!IsValidPoint(_points[i]))
                                _points.RemoveAt(i);
                        }
                    }
                }
            }
        }

        private void RemoveEclipsedPoints()
        {
            for (int i = _points.Count - 1; i >= 0; i--)
            {
                for (int j = 0; j < _points.Count; j++)
                {
                    if (i != j && i < _points.Count && j < _points.Count)
                    {
                        if (_points[j].Eclipses(_points[i]))
                        {
                            _points.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }
    }
}
