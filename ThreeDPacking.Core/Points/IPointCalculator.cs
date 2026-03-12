using System.Collections.Generic;
using ThreeDPacking.Core.Models;

namespace ThreeDPacking.Core.Points
{
    /// <summary>
    /// Interface for point calculators that manage extreme points for box placement.
    /// </summary>
    public interface IPointCalculator
    {
        /// <summary>
        /// Initialize the point calculator to the given container dimensions.
        /// Creates a single initial point spanning the entire container.
        /// </summary>
        void ClearToSize(int dx, int dy, int dz);

        /// <summary>
        /// Add a placement to the calculator, updating extreme points.
        /// Returns the index of the point that was used for this placement.
        /// </summary>
        int Add(int pointIndex, Placement placement);

        /// <summary>
        /// Get the number of available points.
        /// </summary>
        int PointCount { get; }

        /// <summary>
        /// Get a point by index.
        /// </summary>
        ExtremePoint GetPoint(int index);

        /// <summary>
        /// Get all available points.
        /// </summary>
        IList<ExtremePoint> GetAllPoints();

        /// <summary>
        /// Check if there are any available points.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Get the maximum area available across all points.
        /// </summary>
        long GetMaxArea();

        /// <summary>
        /// Get the maximum volume available across all points.
        /// </summary>
        long GetMaxVolume();

        /// <summary>
        /// Set minimum area and volume limits, removing points below these thresholds.
        /// </summary>
        void SetMinimumAreaAndVolumeLimit(long minArea, long minVolume);
    }
}
