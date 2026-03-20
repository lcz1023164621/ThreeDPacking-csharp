using System.Collections.Generic;
using ThreeDPacking.Core.Models;

namespace ThreeDPacking.Core.Packers
{
    /// <summary>
    /// 所有装箱算法的统一接口。
    /// </summary>
    public interface IPackager
    {
        /// <summary>
        /// 将物品装入单个容器。
        /// </summary>
        /// <param name="items">待装箱物品（内部会先克隆）。</param>
        /// <param name="container">目标容器（内部会先克隆）。</param>
        /// <returns>返回装箱后的容器；若无法放置任何物品则返回 null。</returns>
        Container Pack(List<BoxItem> items, Container container);
    }
}
