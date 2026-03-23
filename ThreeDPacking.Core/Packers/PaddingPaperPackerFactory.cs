namespace ThreeDPacking.Core.Packers
{
    /// <summary>
    /// 牛皮纸填充器工厂，按策略返回具体实现。
    /// </summary>
    public static class PaddingPaperPackerFactory
    {
        public static IPaddingPaperPacker Create(PaddingPaperFillStrategy strategy)
        {
            if (strategy == PaddingPaperFillStrategy.StableLayerFill ||
                strategy == PaddingPaperFillStrategy.LayerFill)
            {
                return new LayerFillPaddingPaperPacker();
            }

            return new PaddingPaperPacker();
        }
    }
}
