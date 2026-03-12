using System;
using System.Collections.Generic;

namespace ThreeDPacking.Core.Points
{
    /// <summary>
    /// A sorted list of ExtremePoints with flag-based removal.
    /// Corresponds to Java Point2DFlagList / Point3DFlagList.
    /// </summary>
    public class PointList
    {
        private readonly List<ExtremePoint> _points = new List<ExtremePoint>();

        public int Count => _points.Count;
        public bool IsEmpty => _points.Count == 0;

        public ExtremePoint this[int index]
        {
            get => _points[index];
            set => _points[index] = value;
        }

        public void Clear()
        {
            _points.Clear();
        }

        public void Add(ExtremePoint point)
        {
            _points.Add(point);
        }

        public void Insert(int index, ExtremePoint point)
        {
            _points.Insert(index, point);
        }

        public void Flag(int index)
        {
            _points[index].Flagged = true;
        }

        public void RemoveFlagged()
        {
            _points.RemoveAll(p => p.Flagged);
        }

        /// <summary>
        /// Binary search for the first point with MinX > value, starting from startIndex.
        /// Returns the index of the first element with MinX > value, or Count if none.
        /// </summary>
        public int BinarySearchPlusMinX(int startIndex, int value)
        {
            int lo = startIndex;
            int hi = _points.Count;
            while (lo < hi)
            {
                int mid = lo + (hi - lo) / 2;
                if (_points[mid].MinX <= value)
                    lo = mid + 1;
                else
                    hi = mid;
            }
            return lo;
        }

        /// <summary>
        /// Binary search for the first point with MinY > value, starting from startIndex.
        /// </summary>
        public int BinarySearchPlusMinY(int startIndex, int value)
        {
            int lo = startIndex;
            int hi = _points.Count;
            while (lo < hi)
            {
                int mid = lo + (hi - lo) / 2;
                if (_points[mid].MinY <= value)
                    lo = mid + 1;
                else
                    hi = mid;
            }
            return lo;
        }

        /// <summary>
        /// Sort the list by the standard ordering (MinX, MinY, MinZ, MaxX, MaxY, MaxZ).
        /// </summary>
        public void Sort()
        {
            _points.Sort();
        }

        /// <summary>
        /// Get the maximum area across all points.
        /// </summary>
        public long GetMaxArea()
        {
            long max = 0;
            foreach (var p in _points)
            {
                long area = p.Area;
                if (area > max) max = area;
            }
            return max;
        }

        /// <summary>
        /// Get the maximum volume across all points.
        /// </summary>
        public long GetMaxVolume()
        {
            long max = 0;
            foreach (var p in _points)
            {
                long vol = p.Volume;
                if (vol > max) max = vol;
            }
            return max;
        }

        /// <summary>
        /// Remove points that are too small (area or volume below limits).
        /// </summary>
        public void RemoveSmallPoints(long minArea, long minVolume)
        {
            for (int i = _points.Count - 1; i >= 0; i--)
            {
                var p = _points[i];
                if (p.Area < minArea || p.Volume < minVolume)
                    _points.RemoveAt(i);
            }
        }

        /// <summary>
        /// Set points from an initial list, replacing all current points.
        /// </summary>
        public void SetFrom(IList<ExtremePoint> points)
        {
            _points.Clear();
            _points.AddRange(points);
        }

        /// <summary>
        /// Swap the contents of this list with another.
        /// </summary>
        public void SwapWith(PointList other)
        {
            var temp = new List<ExtremePoint>(_points);
            _points.Clear();
            _points.AddRange(other._points);
            other._points.Clear();
            other._points.AddRange(temp);
        }

        public List<ExtremePoint> ToList()
        {
            return new List<ExtremePoint>(_points);
        }
    }
}
