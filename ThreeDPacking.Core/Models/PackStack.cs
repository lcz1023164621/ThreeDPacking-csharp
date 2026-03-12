using System;
using System.Collections.Generic;

namespace ThreeDPacking.Core.Models
{
    public class PackStack
    {
        private readonly List<Placement> _entries = new List<Placement>();

        public List<Placement> Placements => _entries;

        public void Add(Placement e)
        {
            _entries.Add(e);
        }

        public void Clear()
        {
            _entries.Clear();
        }

        public int GetWeight()
        {
            int weight = 0;
            foreach (var p in _entries)
                weight += p.StackValue.Box.Weight;
            return weight;
        }

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
