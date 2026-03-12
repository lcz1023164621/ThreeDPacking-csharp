namespace ThreeDPacking.Core.IO
{
    public class ContainerCandidate
    {
        public string Name { get; }
        public int Dx { get; }
        public int Dy { get; }
        public int Dz { get; }
        public int EmptyWeight { get; }
        public int MaxLoadWeight { get; }

        public ContainerCandidate(string name, int dx, int dy, int dz, int emptyWeight, int maxLoadWeight)
        {
            Name = name;
            Dx = dx;
            Dy = dy;
            Dz = dz;
            EmptyWeight = emptyWeight;
            MaxLoadWeight = maxLoadWeight;
        }

        public long Volume => (long)Dx * Dy * Dz;

        public override string ToString()
        {
            return $"{Name} -> {Dx} x {Dy} x {Dz}";
        }
    }
}
