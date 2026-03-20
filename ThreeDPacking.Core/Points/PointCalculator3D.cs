using System;
using System.Collections.Generic;
using ThreeDPacking.Core.Models;

namespace ThreeDPacking.Core.Points
{
    /// <summary>
    /// 三维极端点计算，允许在任意高度、任意位置放置箱子，只要满足支撑条件
    /// 每次放置后，会在 X、Y、Z 三个方向 都进行空间切割
    /// 产生更多的极端点且需要更复杂的支撑检查
    /// </summary>
    public class PointCalculator3D : IPointCalculator
    {
        private readonly List<ExtremePoint> _points = new List<ExtremePoint>();
        private readonly List<Placement> _placements = new List<Placement>();
        private int _containerMaxX;
        private int _containerMaxY;
        private int _containerMaxZ;
        private long _minAreaLimit;
        private long _minVolumeLimit;
        // 生成 Z 方向“顶部极值点”时，对下方投影支撑面积的最低比例要求。
        // 对于普通箱体放置保持默认 1.0（完全支撑），但牛皮纸填充可用更低比例来修正保守裁剪。
        private readonly float _minTopSupportRatio;

        public PointCalculator3D(float minTopSupportRatio = 1f)
        {
            if (minTopSupportRatio < 0f) minTopSupportRatio = 0f;
            if (minTopSupportRatio > 1f) minTopSupportRatio = 1f;
            _minTopSupportRatio = minTopSupportRatio;
        }

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
            // 初始点：整个容器空间
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
            for (int i = _points.Count - 1; i >= 0; i--)
            {
                var p = _points[i];
                if (p.Area < minArea || p.Volume < minVolume)
                    _points.RemoveAt(i);
            }
        }

        /// <summary>
        /// 添加一个放置结果并在 3D 中更新极值点。
        /// 会在已放置箱体的三个方向边界生成新候选点。
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

            // 在放置箱体的三个面方向生成新极值点

            // X 方向点（右侧面）
            if (endX + 1 <= _containerMaxX)
            {
                var point = new ExtremePoint(
                    endX + 1, py, pz,
                    _containerMaxX, _containerMaxY, _containerMaxZ);
                if (IsValidPoint(point))
                    AddPointIfNotEclipsed(point);
            }

            // Y 方向点（前侧面）
            if (endY + 1 <= _containerMaxY)
            {
                var point = new ExtremePoint(
                    px, endY + 1, pz,
                    _containerMaxX, _containerMaxY, _containerMaxZ);
                if (IsValidPoint(point))
                    AddPointIfNotEclipsed(point);
            }

            // Z 方向点（顶部面）- 仅当下方支撑足够时才添加
            if (endZ + 1 <= _containerMaxZ)
            {
                var topPoint = new ExtremePoint(
                    px, py, endZ + 1,
                    _containerMaxX, _containerMaxY, _containerMaxZ);
                if (IsValidPoint(topPoint) && HasSufficientSupportForTopPoint(topPoint, placement))
                    AddPointIfNotEclipsed(topPoint);
            }

            // 对与放置区域重叠的已有极值点执行约束裁剪
            ConstrainPoints(placement);

            // 删除被包含（被遮蔽）的极值点
            RemoveEclipsedPoints();

            return pointIndex;
        }

        /// <summary>
        /// 设置新的极值点集合（用于层切换）。
        /// </summary>
        public void SetPoints(IList<ExtremePoint> newPoints)
        {
            _points.Clear();
            _points.AddRange(newPoints);
        }

        /// <summary>
        /// 清空已放置记录但保留极值点。
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
            foreach (var existing in _points)
            {
                if (existing.Eclipses(newPoint))
                    return;
            }
            _points.Add(newPoint);
        }

        private void ConstrainPoints(Placement placement)
        {
            int pMinX = placement.AbsoluteX;
            int pMinY = placement.AbsoluteY;
            int pMinZ = placement.AbsoluteZ;
            int pEndX = placement.AbsoluteEndX;
            int pEndY = placement.AbsoluteEndY;
            int pEndZ = placement.AbsoluteEndZ;

            var newPoints = new List<ExtremePoint>();

            for (int i = _points.Count - 1; i >= 0; i--)
            {
                var p = _points[i];

                // 检查 3D 重叠
                bool overlapX = p.MinX <= pEndX && p.MaxX >= pMinX;
                bool overlapY = p.MinY <= pEndY && p.MaxY >= pMinY;
                bool overlapZ = p.MinZ <= pEndZ && p.MaxZ >= pMinZ;

                if (overlapX && overlapY && overlapZ)
                {
                    // 检查是否被完全吞没
                    if (p.MinX >= pMinX && p.MinY >= pMinY && p.MinZ >= pMinZ &&
                        p.MaxX <= pEndX && p.MaxY <= pEndY && p.MaxZ <= pEndZ)
                    {
                        _points.RemoveAt(i);
                        continue;
                    }

                    // 点的原点位于放置区域内部或与其接触
                    if (p.MinX >= pMinX && p.MinY >= pMinY && p.MinZ >= pMinZ)
                    {
                        _points.RemoveAt(i);
                        continue;
                    }

                    // 在各维度尝试约束裁剪
                    bool constrainX = p.MinX < pMinX && p.MaxX >= pMinX;
                    bool constrainY = p.MinY < pMinY && p.MaxY >= pMinY;
                    bool constrainZ = p.MinZ < pMinZ && p.MaxZ >= pMinZ;

                    _points.RemoveAt(i);

                    if (constrainX)
                    {
                        var cp = new ExtremePoint(p.MinX, p.MinY, p.MinZ, pMinX - 1, p.MaxY, p.MaxZ);
                        if (IsValidPoint(cp))
                            newPoints.Add(cp);
                    }
                    if (constrainY)
                    {
                        var cp = new ExtremePoint(p.MinX, p.MinY, p.MinZ, p.MaxX, pMinY - 1, p.MaxZ);
                        if (IsValidPoint(cp))
                            newPoints.Add(cp);
                    }
                    if (constrainZ)
                    {
                        var cp = new ExtremePoint(p.MinX, p.MinY, p.MinZ, p.MaxX, p.MaxY, pMinZ - 1);
                        if (IsValidPoint(cp))
                            newPoints.Add(cp);
                    }

                    // 若没有产生有效约束点，则原点已被吞没
                    if (!constrainX && !constrainY && !constrainZ)
                    {
                        // 点完全位于放置区域内，已移除
                    }
                }
            }

            // 添加新生成的约束点
            foreach (var np in newPoints)
            {
                AddPointIfNotEclipsed(np);
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

        /// <summary>
        /// 检查顶部点位置是否有足够的支撑
        /// 要求：新放置点的投影区域必须被下方物品完全支撑
        /// ★★★ 关键修复：必须紧贴上一层
        /// </summary>
        private bool HasSufficientSupportForTopPoint(ExtremePoint topPoint, Placement basePlacement)
        {
            // 顶部点的Z坐标
            int topPointZ = topPoint.MinZ;

            // 计算basePlacement在X-Y平面的投影面积
            long baseArea = (long)basePlacement.StackValue.Dx * basePlacement.StackValue.Dy;

            // 计算所有下方物品（包括basePlacement）提供的总支撑面积
            long totalSupportedArea = 0;

            foreach (var existing in _placements)
            {
                if (existing == null) continue;

                // ★★★ 关键修复：必须紧贴上一层（existing的顶部必须正好在topPoint下方）
                if (existing.AbsoluteEndZ == topPointZ - 1)
                {
                    // 计算2D重叠面积（X-Y平面）
                    long overlapArea = CalculateOverlapArea2D(topPoint, existing);
                    totalSupportedArea += overlapArea;
                }
            }

            // 要求：支撑面积必须100%覆盖顶部点的底面积
            long requiredArea = (long)Math.Ceiling(baseArea * (double)_minTopSupportRatio);
            if (requiredArea < 0) requiredArea = 0;
            return totalSupportedArea >= requiredArea;
        }

        /// <summary>
        /// 计算ExtremePoint和Placement在X-Y平面上的重叠面积
        /// </summary>
        private long CalculateOverlapArea2D(ExtremePoint point, Placement placement)
        {
            // 计算X方向重叠
            int overlapXStart = Math.Max(point.MinX, placement.X);
            int overlapXEnd = Math.Min(point.MaxX, placement.AbsoluteEndX);
            int overlapX = overlapXEnd - overlapXStart + 1;

            // 计算Y方向重叠
            int overlapYStart = Math.Max(point.MinY, placement.Y);
            int overlapYEnd = Math.Min(point.MaxY, placement.AbsoluteEndY);
            int overlapY = overlapYEnd - overlapYStart + 1;

            if (overlapX <= 0 || overlapY <= 0)
                return 0;

            return (long)overlapX * overlapY;
        }
    }
}
