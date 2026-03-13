using System;

namespace ThreeDPacking.Core.Models
{
    /// <summary>
    /// 待装箱物体模型,一个Box的“多份拷贝”管理器
    /// </summary>
    public class BoxItem
    {
        //关联的box模型
        public Box Box { get; }
        //剩余可装数量。每次成功放置后调用Decrement()减1
        public int Count { get; set; }
        public int Index { get; set; }
        //用于Clone/Reset时保存原始数量
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
        /// <summary>
        /// 消耗一件，返回是否還有剩餘
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// 把 Count 恢復到最初設定的數量
        /// </summary>
        public void Reset()
        {
            Count = _resetCount;
        }
        /// <summary>
        /// 把當前剩餘數量記錄為新的基準數量
        /// </summary>
        public void Mark()
        {
            _resetCount = Count;
        }
        /// <summary>
        /// 產生一個數量相同的獨立副本
        /// </summary>
        /// <returns></returns>
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
