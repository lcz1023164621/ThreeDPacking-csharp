using System;
using System.Collections.Generic;

namespace ThreeDPacking.Core.Models
{
    /// <summary>
    /// 装箱堆栈，记录装箱过程中的层级信息
    /// </summary>
    public class PackStack
    {
        //所有成功放置的物品位置信息
        private readonly List<Placement> _entries = new List<Placement>();

        public List<Placement> Placements => _entries;
        /// <summary>
        /// 加入放置记录
        /// </summary>
        /// <param name="e"></param>
        public void Add(Placement e)
        {
            _entries.Add(e);
        }

        public void Clear()
        {
            _entries.Clear();
        }
        /// <summary>
        /// 计算已放置物品总重量
        /// </summary>
        /// <returns></returns>
        public int GetWeight()
        {
            int weight = 0;
            foreach (var p in _entries)
                weight += p.StackValue.Box.Weight;
            return weight;
        }
        /// <summary>
        /// 计算已放入物体的总体积
        /// </summary>
        /// <returns></returns>
        public long GetVolume()
        {
            long volume = 0;
            foreach (var p in _entries)
                volume += p.StackValue.Box.Volume;
            return volume;
        }

        public int GetDz()
        {
            int dz = 0;
            foreach (var p in _entries)
                dz = Math.Max(dz, p.AbsoluteEndZ);
            return dz;
        }

        public bool IsEmpty => _entries.Count == 0;
        public int Size => _entries.Count;

        public void SetSize(int size)
        {
            while (size < _entries.Count)
                _entries.RemoveAt(_entries.Count - 1);
        }
    }
}
