namespace ThreeDPacking.Core.Packers
{
    /// <summary>
    /// 牛皮纸填充器工厂，按策略返回具体实现。
    /// </summary>
    public static class PaddingPaperPackerFactory
    {
        public static IPaddingPaperPacker Create(PaddingPaperFillStrategy strategy, int minPaddingWidth = 110)
        {
            if (strategy == PaddingPaperFillStrategy.StableLayerFill ||
                strategy == PaddingPaperFillStrategy.LayerFill)
            {
                return new LayerFillPaddingPaperPacker(minPaddingWidth);
            }

            return new PaddingPaperPacker(minPaddingWidth);
        }
    }
}
