using System;

namespace ThreeDPacking.Core.Models
{
    public class BoxItem
    {
        public Box Box { get; }
        public int Count { get; set; }
        public int Index { get; set; }
        private int _resetCount;

        public BoxItem(Box box, int count)
        {
            Box = box;
            Count = count;
            _resetCount = count;
            Index = -1;
            box.BoxItem = this;
        }

        public BoxItem(Box box, int count, int index)
        {
            Box = box;
            Count = count;
            Index = index;
            _resetCount = count;
            box.BoxItem = this;
        }

        public bool Decrement()
        {
            Count--;
            return Count > 0;
        }

        public bool Decrement(int value)
        {
            Count -= value;
            return Count > 0;
        }

        public bool IsEmpty => Count == 0;

        public long GetVolume()
        {
            return (long)Count * Box.Volume;
        }

        public long GetWeight()
        {
            return (long)Count * Box.Weight;
        }

        public void Reset()
        {
            Count = _resetCount;
        }

        public void Mark()
        {
            _resetCount = Count;
        }

        public BoxItem Clone()
        {
            return new BoxItem(Box, Count, Index);
        }

        public override string ToString()
        {
            return $"{Count}x{Box} #{Index}";
        }
    }
}
