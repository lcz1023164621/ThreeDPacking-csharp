using System.Collections.Generic;
using ThreeDPacking.Core.Models;
using ThreeDPacking.Core.Points;

namespace ThreeDPacking.Core.Comparators
{
    /// <summary>
    /// 综合体积、重量、面积的排序
    /// </summary>
    public class VolumeWeightAreaPointComparer : IComparer<PlacementCandidate>
    {
        public int Compare(PlacementCandidate a, PlacementCandidate b)
        {
            // Largest volume first (maximize space utilization)
            int c = b.Placement.StackValue.Volume.CompareTo(a.Placement.StackValue.Volume);
            if (c != 0) return c;
            // Heaviest first
            c = b.Placement.StackValue.Box.Weight.CompareTo(a.Placement.StackValue.Box.Weight);
            if (c != 0) return c;
            // Prefer lower Z (bottom-up for stability, but not mandatory)
            c = a.Placement.Z.CompareTo(b.Placement.Z);
            if (c != 0) return c;
            // Smallest area first (prefer tight fits)
            c = a.Placement.StackValue.Area.CompareTo(b.Placement.StackValue.Area);
            if (c != 0) return c;
            // Smallest point volume first (prefer lower/closer points)
            return a.Point.Volume.CompareTo(b.Point.Volume);
        }
    }

    /// <summary>
    /// Represents a candidate placement with its associated point.
    /// </summary>
    public class PlacementCandidate
    {
        public Placement Placement { get; }
        public ExtremePoint Point { get; }

        public PlacementCandidate(Placement placement, ExtremePoint point)
        {
            Placement = placement;
            Point = point;
        }
    }
}
