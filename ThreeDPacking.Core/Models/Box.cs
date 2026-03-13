using System;
using System.Collections.Generic;

namespace ThreeDPacking.Core.Models
{
    public class Box
    {
        public string Id { get; }
        public string Description { get; }
        public int Weight { get; }
        public long Volume { get; }
        public BoxStackValue[] StackValues { get; }
        public BoxStackValue MinimumArea { get; }
        public BoxStackValue MaximumArea { get; }
        public BoxItem BoxItem { get; set; }

        public Box(string id, string description, int dx, int dy, int dz, int weight, bool rotate3D)
        {
            Id = id;
            Description = description;
            Weight = weight;
            Volume = (long)dx * dy * dz;
            StackValues = ComputeStackValues(dx, dy, dz, rotate3D);

            MinimumArea = GetMinimumArea(StackValues);
            MaximumArea = GetMaximumArea(StackValues);

            foreach (var sv in StackValues)
                sv.Box = this;
        }

        private static BoxStackValue[] ComputeStackValues(int dx, int dy, int dz, bool rotate3D)
        {
            var list = new List<BoxStackValue>();

            if (dx == dy && dx == dz)
            {
                // Cube: all sides equal, only 1 rotation needed
                list.Add(new BoxStackValue(dx, dy, dz, list.Count));
            }
            else if (!rotate3D)
            {
                // 2D rotation only: keep the original orientation, no rotation
                // The dx, dy, dz should already be set to have maximum bottom area
                list.Add(new BoxStackValue(dx, dy, dz, list.Count));
            }
            else if (dx == dy)
            {
                // Two square sides (bottom square)
                list.Add(new BoxStackValue(dx, dy, dz, list.Count)); // XY
                list.Add(new BoxStackValue(dx, dz, dx, list.Count)); // XZ 0
                list.Add(new BoxStackValue(dz, dx, dx, list.Count)); // XZ 90
            }
            else if (dz == dy)
            {
                // Two square sides (side square)
                list.Add(new BoxStackValue(dy, dy, dx, list.Count)); // YZ
                list.Add(new BoxStackValue(dx, dz, dz, list.Count)); // XY+XZ 0
                list.Add(new BoxStackValue(dz, dx, dz, list.Count)); // XY+XZ 90
            }
            else if (dx == dz)
            {
                // Two square sides (front square)
                list.Add(new BoxStackValue(dx, dx, dy, list.Count)); // XZ
                list.Add(new BoxStackValue(dx, dy, dx, list.Count)); // XY+YZ 0
                list.Add(new BoxStackValue(dy, dx, dx, list.Count)); // XY+YZ 90
            }
            else
            {
                // No equal edges: 6 rotations
                list.Add(new BoxStackValue(dx, dy, dz, list.Count)); // XY 0
                list.Add(new BoxStackValue(dy, dx, dz, list.Count)); // XY 90
                list.Add(new BoxStackValue(dx, dz, dy, list.Count)); // XZ 0
                list.Add(new BoxStackValue(dz, dx, dy, list.Count)); // XZ 90
                list.Add(new BoxStackValue(dz, dy, dx, list.Count)); // YZ 0
                list.Add(new BoxStackValue(dy, dz, dx, list.Count)); // YZ 90
            }

            if (list.Count == 0)
                throw new InvalidOperationException("Expected at least one stackable surface");

            return list.ToArray();
        }

        public bool FitsInside(int dx, int dy, int dz)
        {
            foreach (var sv in StackValues)
            {
                if (sv.FitsInside3D(dx, dy, dz))
                    return true;
            }
            return false;
        }

        public List<BoxStackValue> GetRotations(int dx, int dy, int dz)
        {
            var result = new List<BoxStackValue>();
            foreach (var sv in StackValues)
            {
                if (sv.FitsInside3D(dx, dy, dz))
                    result.Add(sv);
            }
            return result.Count > 0 ? result : null;
        }

        public long GetMinimumAreaValue()
        {
            return MinimumArea.Area;
        }

        public long GetMaximumAreaValue()
        {
            return MaximumArea.Area;
        }

        private static BoxStackValue GetMinimumArea(BoxStackValue[] values)
        {
            BoxStackValue min = null;
            foreach (var v in values)
            {
                if (min == null || v.Area < min.Area)
                    min = v;
            }
            return min;
        }

        private static BoxStackValue GetMaximumArea(BoxStackValue[] values)
        {
            BoxStackValue max = null;
            foreach (var v in values)
            {
                if (max == null || v.Area > max.Area)
                    max = v;
            }
            return max;
        }

        public override string ToString()
        {
            return $"Box {Id} ({Description}) [{Weight}w, {Volume}v]";
        }
    }
}
