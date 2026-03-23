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
    }
}
