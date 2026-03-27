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
    }
}
