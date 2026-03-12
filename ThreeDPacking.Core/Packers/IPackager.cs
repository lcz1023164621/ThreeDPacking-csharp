using System.Collections.Generic;
using ThreeDPacking.Core.Models;

namespace ThreeDPacking.Core.Packers
{
    /// <summary>
    /// Interface for all packing algorithms.
    /// </summary>
    public interface IPackager
    {
        /// <summary>
        /// Pack items into a single container.
        /// </summary>
        /// <param name="items">The items to pack (will be cloned internally).</param>
        /// <param name="container">The container to pack into (will be cloned internally).</param>
        /// <returns>A packed container with placements, or null if nothing could be packed.</returns>
        Container Pack(List<BoxItem> items, Container container);
    }
}
