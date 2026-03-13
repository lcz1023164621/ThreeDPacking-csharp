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
            // Initial point: entire container space
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
        /// Add a placement and update extreme points in 3D.
        /// Generates new candidate points at all three edges of the placed box.
        /// </summary>
        public int Add(int pointIndex, Placement placement)
        {
            _placements.Add(placement);

            // Remove the used point
            _points.RemoveAt(pointIndex);

            int px = placement.AbsoluteX;
            int py = placement.AbsoluteY;
            int pz = placement.AbsoluteZ;
            int endX = placement.AbsoluteEndX;
            int endY = placement.AbsoluteEndY;
            int endZ = placement.AbsoluteEndZ;

            // Generate new extreme points at the 3 faces of the placed box

            // Point along X (right face)
            if (endX + 1 <= _containerMaxX)
            {
                var point = new ExtremePoint(
                    endX + 1, py, pz,
                    _containerMaxX, _containerMaxY, _containerMaxZ);
                if (IsValidPoint(point))
                    AddPointIfNotEclipsed(point);
            }

            // Point along Y (front face)
            if (endY + 1 <= _containerMaxY)
            {
                var point = new ExtremePoint(
                    px, endY + 1, pz,
                    _containerMaxX, _containerMaxY, _containerMaxZ);
                if (IsValidPoint(point))
                    AddPointIfNotEclipsed(point);
            }

            // Point along Z (top face) - only add if there's sufficient support below
            if (endZ + 1 <= _containerMaxZ)
            {
                var topPoint = new ExtremePoint(
                    px, py, endZ + 1,
                    _containerMaxX, _containerMaxY, _containerMaxZ);
                if (IsValidPoint(topPoint) && HasSufficientSupportForTopPoint(topPoint, placement))
                    AddPointIfNotEclipsed(topPoint);
            }

            // Constrain existing points that overlap with the placement
            ConstrainPoints(placement);

            // Remove eclipsed points
            RemoveEclipsedPoints();

            return pointIndex;
        }

        /// <summary>
        /// Set new points (used for level transitions).
        /// </summary>
        public void SetPoints(IList<ExtremePoint> newPoints)
        {
            _points.Clear();
            _points.AddRange(newPoints);
        }

        /// <summary>
        /// Clear placements but keep points.
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

                // Check 3D overlap
                bool overlapX = p.MinX <= pEndX && p.MaxX >= pMinX;
                bool overlapY = p.MinY <= pEndY && p.MaxY >= pMinY;
                bool overlapZ = p.MinZ <= pEndZ && p.MaxZ >= pMinZ;

                if (overlapX && overlapY && overlapZ)
                {
                    // Check if completely swallowed
                    if (p.MinX >= pMinX && p.MinY >= pMinY && p.MinZ >= pMinZ &&
                        p.MaxX <= pEndX && p.MaxY <= pEndY && p.MaxZ <= pEndZ)
                    {
                        _points.RemoveAt(i);
                        continue;
                    }

                    // The point origin is inside or touches the placement
                    if (p.MinX >= pMinX && p.MinY >= pMinY && p.MinZ >= pMinZ)
                    {
                        _points.RemoveAt(i);
                        continue;
                    }

                    // Try to constrain in each dimension
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

                    // If no constraints created valid points, the original was just swallowed
                    if (!constrainX && !constrainY && !constrainZ)
                    {
                        // Point fully inside placement, already removed
                    }
                }
            }

            // Add the newly constrained points
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
            return totalSupportedArea >= baseArea;
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
