namespace ThreeDPacking.Core.IO
{
    public class ItemCandidate
    {
        public string Name { get; }
        public int Dx { get; }
        public int Dy { get; }
        public int Dz { get; }
        public int InstanceId { get; }

        public ItemCandidate(string name, int dx, int dy, int dz, int instanceId)
        {
            Name = name;
            Dx = dx;
            Dy = dy;
            Dz = dz;
            InstanceId = instanceId;
        }

        public long Volume => (long)Dx * Dy * Dz;

        public override string ToString()
        {
            return $"{Name}#{InstanceId} -> {Dx} x {Dy} x {Dz}";
        }
    }
}
