using System;
using System.Collections.Generic;

namespace ThreeDPacking.Core.Models
{
    public class BoxStackValue
    {
        public int Dx { get; }
        public int Dy { get; }
        public int Dz { get; }
        public long Area { get; }
        public long Volume { get; }
        public int Index { get; }
        public Box Box { get; set; }

        public BoxStackValue(int dx, int dy, int dz, int index)
        {
            Dx = dx;
            Dy = dy;
            Dz = dz;
            Area = (long)dx * dy;
            Volume = Area * dz;
            Index = index;
        }

        public bool FitsInside3D(int dx, int dy, int dz)
        {
            return dx >= Dx && dy >= Dy && dz >= Dz;
        }

        public bool FitsInside2D(int dx, int dy)
        {
            return dx >= Dx && dy >= Dy;
        }

        public BoxStackValue Clone()
        {
            var clone = new BoxStackValue(Dx, Dy, Dz, Index);
            clone.Box = Box;
            return clone;
        }

        public override string ToString()
        {
            return $"BoxStackValue[{Dx}x{Dy}x{Dz}]";
        }
    }
}
