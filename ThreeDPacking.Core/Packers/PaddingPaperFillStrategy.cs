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

        /// <summary>
        /// 客户实际需求装填：
        /// 1) 先预铺底层牛皮纸
        /// 2) 按既有装箱策略装箱
        /// 3) 装箱完成后仅在顶部尽量叠放牛皮纸
        /// </summary>
        CustomerDemandFill = 2,

        // Backward compatible aliases
        MaxVolume = MaxUtilization,
        LayerFill = StableLayerFill
    }
}

