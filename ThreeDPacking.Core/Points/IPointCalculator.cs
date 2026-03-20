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
        /// 获取当前可用极值点数量。
        /// </summary>
        int PointCount { get; }

        /// <summary>
        /// 按索引获取一个极值点。
        /// </summary>
        ExtremePoint GetPoint(int index);

        /// <summary>
        /// 获取全部可用极值点。
        /// </summary>
        IList<ExtremePoint> GetAllPoints();

        /// <summary>
        /// 判断是否还有可用极值点。
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// 获取所有极值点中的最大可用底面积。
        /// </summary>
        long GetMaxArea();

        /// <summary>
        /// 获取所有极值点中的最大可用体积。
        /// </summary>
        long GetMaxVolume();

        /// <summary>
        /// 设置最小面积/体积阈值，并移除低于阈值的极值点。
        /// </summary>
        void SetMinimumAreaAndVolumeLimit(long minArea, long minVolume);
    }
}
