using System.Collections.Generic;
using ThreeDPacking.Core.Models;

namespace ThreeDPacking.Core.Comparators
{
    /// <summary>
    /// Compares BoxItems by largest area (descending), then volume, then weight.
    /// Used to select which box to try placing first.
    /// </summary>
    public class LargestAreaBoxItemComparer : IComparer<BoxItem>
    {
        public int Compare(BoxItem a, BoxItem b)
        {
            // Largest area first (descending)
            int c = b.Box.GetMaximumAreaValue().CompareTo(a.Box.GetMaximumAreaValue());
            if (c != 0) return c;
            // Then largest volume
            c = b.Box.Volume.CompareTo(a.Box.Volume);
            if (c != 0) return c;
            // Then heaviest
            return b.Box.Weight.CompareTo(a.Box.Weight);
        }
    }
}
