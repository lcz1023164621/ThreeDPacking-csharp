namespace ThreeDPacking.Core.IO
{
    /// <summary>
    /// 物品候选数据
    /// </summary>
    public class ItemCandidate
    {
        public string Name { get; }
        public int Dx { get; }
        public int Dy { get; }
        public int Dz { get; }
        public int InstanceId { get; }
        public double Probability { get; }

        public ItemCandidate(string name, int dx, int dy, int dz, int instanceId, double probability = 0)
        {
            Name = name;
            Dx = dx;
            Dy = dy;
            Dz = dz;
            InstanceId = instanceId;
            Probability = probability;
        }

        public long Volume => (long)Dx * Dy * Dz;

        public override string ToString()
        {
            return $"{Name}#{InstanceId} -> {Dx} x {Dy} x {Dz} (P={Probability})";
        }
    }
}
