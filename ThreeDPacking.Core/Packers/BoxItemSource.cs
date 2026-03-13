using System;
using System.Collections.Generic;
using ThreeDPacking.Core.Models;

namespace ThreeDPacking.Core.Packers
{
    /// <summary>
    /// 物品源管理，控制待装箱物品的队列，优化数据层，优化开销
    /// </summary>
    public class BoxItemSource
    {
        private readonly List<BoxItem> _items;

        public BoxItemSource(List<BoxItem> items)
        {
            _items = new List<BoxItem>();
            for (int i = 0; i < items.Count; i++)
            {
                var clone = items[i].Clone();
                clone.Index = i;
                _items.Add(clone);
            }
        }

        public int Size => _items.Count;
        public bool IsEmpty => _items.Count == 0;

        public BoxItem Get(int index)
        {
            return _items[index];
        }

        public BoxItem Remove(int index)
        {
            var item = _items[index];
            _items.RemoveAt(index);
            // Re-index
            for (int i = index; i < _items.Count; i++)
                _items[i].Index = i;
            return item;
        }

        public void Decrement(int index, int count)
        {
            var item = _items[index];
            item.Decrement(count);
            if (item.IsEmpty)
            {
                _items.RemoveAt(index);
                for (int i = index; i < _items.Count; i++)
                    _items[i].Index = i;
            }
        }

        public long GetMinArea()
        {
            long min = long.MaxValue;
            foreach (var item in _items)
            {
                long area = item.Box.GetMinimumAreaValue();
                if (area < min) min = area;
            }
            return _items.Count == 0 ? 0 : min;
        }

        public long GetMaxArea()
        {
            long max = 0;
            foreach (var item in _items)
            {
                long area = item.Box.GetMaximumAreaValue();
                if (area > max) max = area;
            }
            return max;
        }

        public long GetMinVolume()
        {
            long min = long.MaxValue;
            foreach (var item in _items)
            {
                if (item.Box.Volume < min) min = item.Box.Volume;
            }
            return _items.Count == 0 ? 0 : min;
        }

        public long GetMaxVolume()
        {
            long max = 0;
            foreach (var item in _items)
            {
                if (item.Box.Volume > max) max = item.Box.Volume;
            }
            return max;
        }
    }
}
