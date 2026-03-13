using System.Collections.Generic;

namespace ThreeDPacking.Core.Models
{
    /// <summary>
    /// 装箱结果封装
    /// </summary>
    public class PackagerResult
    {
        //最后成功装箱的所有容器
        public List<Container> Containers { get; }
        //装箱耗时（毫秒）
        public long DurationMs { get; }
        //是否超时未完成装箱
        public bool IsTimeout { get; }

        public PackagerResult(List<Container> containers, long durationMs, bool isTimeout)
        {
            Containers = containers ?? new List<Container>();
            DurationMs = durationMs;
            IsTimeout = isTimeout;
        }

        public bool IsSuccess => Containers.Count > 0;
    }
}
