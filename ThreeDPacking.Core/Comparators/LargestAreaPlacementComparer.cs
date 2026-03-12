using System.Collections.Generic;
using ThreeDPacking.Core.Models;

namespace ThreeDPacking.Core.Comparators
{
    /// <summary>
    /// Compares Placements by largest area (descending), then volume, then weight.
    /// Used to select the best placement point for new-level first placement.
    /// </summary>
    public class LargestAreaPlacementComparer : IComparer<Placement>
    {
        public int Compare(Placement a, Placement b)
        {
            // Largest area first (descending)
            int c = b.StackValue.Area.CompareTo(a.StackValue.Area);
            if (c != 0) return c;
            // Then largest volume
            c = b.StackValue.Volume.CompareTo(a.StackValue.Volume);
            if (c != 0) return c;
            // Then heaviest
            return b.StackValue.Box.Weight.CompareTo(a.StackValue.Box.Weight);
        }
    }
}
