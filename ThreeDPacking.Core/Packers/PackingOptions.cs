using System;
using ThreeDPacking.Core.Models;

namespace ThreeDPacking.Core.Packers
{
    /// <summary>
    /// 装箱可选行为（例如每放入一件物品后的回调）。
    /// </summary>
    public sealed class PackingOptions
    {
        public static PackingOptions Default { get; } = new PackingOptions();

        /// <summary>
        /// 每成功放置一件真实物品（非填充物）后调用；可用于与装箱同步填充牛皮纸。
        /// </summary>
        public Action<Container> AfterEachItemPlacement { get; set; }

        /// <summary>
        /// 牛皮纸填充策略。默认：<see cref="PaddingPaperFillStrategy.MaxUtilization"/>。
        /// </summary>
        public PaddingPaperFillStrategy PaddingPaperStrategy { get; set; } = PaddingPaperFillStrategy.MaxUtilization;

        /// <summary>
        /// 牛皮纸最小宽度（默认110mm）。
        /// </summary>
        public int PaddingPaperMinWidth { get; set; } = PaddingPaper.DefaultWidth;

        /// <summary>
        /// 容器四周（X/Y方向）保留的安全距离。默认0。
        /// </summary>
        public int ContainerSafetyDistance { get; set; } = 0;

        /// <summary>
        /// 物体与物体之间（X/Y方向）最小安全距离。默认0。
        /// 为避免双倍间距，算法内部按“单边膨胀”处理，只会形成一份安全距离。
        /// </summary>
        public int ItemSafetyDistance { get; set; } = 0;

        /// <summary>
        /// 牛皮纸与牛皮纸/物体/容器之间（X/Y方向）最小安全距离。默认0。
        /// </summary>
        public int PaddingPaperSafetyDistance { get; set; } = 0;
    }
}
