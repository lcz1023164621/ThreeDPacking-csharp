using System;
using System.Collections.Generic;

namespace ThreeDPacking.Core.Points
{
    /// <summary>
    /// 极端点列表管理
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
        /// 从 startIndex 开始二分查找第一个 MinX > value 的点。
        /// 若不存在，返回 Count。
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
        /// 从 startIndex 开始二分查找第一个 MinY > value 的点。
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
        /// 按标准顺序排序（MinX, MinY, MinZ, MaxX, MaxY, MaxZ）。
        /// </summary>
        public void Sort()
        {
            _points.Sort();
        }

        /// <summary>
        /// 获取所有点中的最大面积。
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
        /// 获取所有点中的最大体积。
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
        /// 移除过小的点（面积或体积低于阈值）。
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
        /// 用给定初始列表替换当前全部点。
        /// </summary>
        public void SetFrom(IList<ExtremePoint> points)
        {
            _points.Clear();
            _points.AddRange(points);
        }

        /// <summary>
        /// 与另一个 PointList 交换内容。
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
