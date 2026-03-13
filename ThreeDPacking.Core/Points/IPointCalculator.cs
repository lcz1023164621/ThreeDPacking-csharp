using System.Collections.Generic;
using ThreeDPacking.Core.Models;

namespace ThreeDPacking.Core.Points
{
    /// <summary>
    /// 极端点管理器的抽象接口
    /// </summary>
    public interface IPointCalculator
    {
        /// <summary>
        /// 初始化为空容器大小的空间（通常只有一个点：整个容器）
        /// </summary>
        void ClearToSize(int dx, int dy, int dz);

        /// <summary>
        /// 放置一个箱子后，更新所有剩余空间
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
