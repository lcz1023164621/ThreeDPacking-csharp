using System.Collections.Generic;

namespace ThreeDPacking.Core.Models
{
    public class PackagerResult
    {
        public List<Container> Containers { get; }
        public long DurationMs { get; }
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
