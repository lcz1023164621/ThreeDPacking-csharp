namespace ThreeDPacking.Core.Packers
{
    /// <summary>
    /// 牛皮纸填充策略选择
    /// </summary>
    public enum PaddingPaperFillStrategy
    {
        /// <summary>
        /// 旧策略：最大体积优先
        /// </summary>
        MaxVolume = 0,

        /// <summary>
        /// 新策略：分层填充优先（更均匀、更倾向持续铺满）
        /// </summary>
        LayerFill = 1
    }
}

