using System;
using System.Collections.Generic;
using ThreeDPacking.Core.Models;

namespace ThreeDPacking.Core.Points
{
    /// <summary>
    /// 2D extreme point calculator for the LAFF (Largest Area Fit First) algorithm.
    /// Places boxes only on the floor of each level, tracking 2D extreme points.
    /// Simplified from the Java DefaultPointCalculator2D.
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
            // Initial point: entire container floor
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
            // Remove points that are too small
            for (int i = _points.Count - 1; i >= 0; i--)
            {
                var p = _points[i];
                if (p.Area < minArea || p.Volume < minVolume)
                    _points.RemoveAt(i);
            }
        }

        /// <summary>
        /// Add a placement and update extreme points.
        /// Uses the 2D extreme point algorithm: when a box is placed at a point,
        /// new candidate points are generated at the box's edges.
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

            // Generate new extreme points at the box edges
            // Point 1: Right of the placed box (along X axis)
            if (endX + 1 <= _containerMaxX)
            {
                var rightPoint = new ExtremePoint(
                    endX + 1, py, pz,
                    _containerMaxX, _containerMaxY, _containerMaxZ);
                if (IsValidPoint(rightPoint))
                    AddPointIfNotEclipsed(rightPoint);
            }

            // Point 2: In front of the placed box (along Y axis)
            if (endY + 1 <= _containerMaxY)
            {
                var frontPoint = new ExtremePoint(
                    px, endY + 1, pz,
                    _containerMaxX, _containerMaxY, _containerMaxZ);
                if (IsValidPoint(frontPoint))
                    AddPointIfNotEclipsed(frontPoint);
            }

            // Constrain existing points that overlap with the placement
            ConstrainPoints(placement);

            // Remove eclipsed points
            RemoveEclipsedPoints();

            return pointIndex;
        }

        /// <summary>
        /// Set new points (used when starting a new level in LAFF).
        /// </summary>
        public void SetPoints(IList<ExtremePoint> newPoints)
        {
            _points.Clear();
            _points.AddRange(newPoints);
        }

        /// <summary>
        /// Clear placements but keep points (used for level transitions).
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
            // Check if any existing point eclipses the new one
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

                // Check if point overlaps with placement in 2D
                bool overlapX = p.MinX <= endX && p.MaxX >= placement.AbsoluteX;
                bool overlapY = p.MinY <= endY && p.MaxY >= placement.AbsoluteY;

                if (overlapX && overlapY)
                {
                    // Point is inside or overlapping the placed box
                    if (p.MinX >= placement.AbsoluteX && p.MinY >= placement.AbsoluteY)
                    {
                        // Point is completely swallowed - remove it
                        _points.RemoveAt(i);
                    }
                    else
                    {
                        // Constrain the point
                        bool needsConstrainX = p.MinX < placement.AbsoluteX && p.MaxX >= placement.AbsoluteX;
                        bool needsConstrainY = p.MinY < placement.AbsoluteY && p.MaxY >= placement.AbsoluteY;

                        if (needsConstrainX && needsConstrainY)
                        {
                            // Split: create constrained X version and constrained Y version
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
