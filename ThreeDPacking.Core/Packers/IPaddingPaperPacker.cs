using System;
using ThreeDPacking.Core.Models;

namespace ThreeDPacking.Core.Packers
{
    /// <summary>
    /// 牛皮纸填充统一接口。
    /// </summary>
    public interface IPaddingPaperPacker
    {
        void FillWithPaddingPaper(Container container, Action<string> log = null);
    }
}
