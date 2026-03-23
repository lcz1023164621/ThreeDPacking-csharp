namespace ThreeDPacking.Core.Packers
{
    /// <summary>
    /// 牛皮纸填充策略选择
    /// </summary>
    public enum PaddingPaperFillStrategy
    {
        /// <summary>
        /// 体积利用率优先（推荐默认）
        /// </summary>
        MaxUtilization = 0,

        /// <summary>
        /// 稳定分层填充优先（低层优先、层连续性优先）
        /// </summary>
        StableLayerFill = 1,

        // Backward compatible aliases
        MaxVolume = MaxUtilization,
        LayerFill = StableLayerFill
    }
}

