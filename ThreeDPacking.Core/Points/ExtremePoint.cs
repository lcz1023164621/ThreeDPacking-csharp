using System;

namespace ThreeDPacking.Core.Points
{
    /// <summary>
    /// 极端点数据结构，表示可放置物品的候选位置
    /// </summary>
    public class ExtremePoint : IComparable<ExtremePoint>
    {
        //左下前坐标
        public int MinX { get; set; }
        public int MinY { get; set; }
        public int MinZ { get; set; }
        //右上后坐标
        public int MaxX { get; set; }
        public int MaxY { get; set; }
        public int MaxZ { get; set; }
        //可放尺寸
        public int Dx => MaxX - MinX + 1;
        public int Dy => MaxY - MinY + 1;
        public int Dz => MaxZ - MinZ + 1;

        public long Area => (long)Dx * Dy;
        public long Volume => (long)Dx * Dy * Dz;

        public bool Flagged { get; set; }

        public ExtremePoint(int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
        {
            MinX = minX;
            MinY = minY;
            MinZ = minZ;
            MaxX = maxX;
            MaxY = maxY;
            MaxZ = maxZ;
        }

        /// <summary>
        /// 判断这个剩余空间能否放下某个朝向的箱子
        /// </summary>
        /// <param name="sv"></param>
        /// <returns></returns>
        public bool FitsBox(BoxStackValueRef sv)
        {
            return Dx >= sv.Dx && Dy >= sv.Dy && Dz >= sv.Dz;
        }

        /// <summary>
        /// 判断当前点是否完全包含另一个点（用于去重/消除被覆盖的点）
        /// </summary>
        public bool Eclipses(ExtremePoint other)
        {
            return MinX <= other.MinX && MinY <= other.MinY && MinZ <= other.MinZ &&
                   MaxX >= other.MaxX && MaxY >= other.MaxY && MaxZ >= other.MaxZ;
        }

        public bool EclipsesMovedX(ExtremePoint other, int moveX)
        {
            return MinX <= moveX && MinY <= other.MinY && MinZ <= other.MinZ &&
                   MaxX >= other.MaxX && MaxY >= other.MaxY && MaxZ >= other.MaxZ;
        }

        public bool EclipsesMovedY(ExtremePoint other, int moveY)
        {
            return MinX <= other.MinX && MinY <= moveY && MinZ <= other.MinZ &&
                   MaxX >= other.MaxX && MaxY >= other.MaxY && MaxZ >= other.MaxZ;
        }

        public bool EclipsesMovedZ(ExtremePoint other, int moveZ)
        {
            return MinX <= other.MinX && MinY <= other.MinY && MinZ <= moveZ &&
                   MaxX >= other.MaxX && MaxY >= other.MaxY && MaxZ >= other.MaxZ;
        }

        public ExtremePoint Clone()
        {
            return new ExtremePoint(MinX, MinY, MinZ, MaxX, MaxY, MaxZ);
        }

        public ExtremePoint CloneWithMaxX(int maxX)
        {
            return new ExtremePoint(MinX, MinY, MinZ, maxX, MaxY, MaxZ);
        }

        public ExtremePoint CloneWithMaxY(int maxY)
        {
            return new ExtremePoint(MinX, MinY, MinZ, MaxX, maxY, MaxZ);
        }

        public ExtremePoint CloneWithMaxZ(int maxZ)
        {
            return new ExtremePoint(MinX, MinY, MinZ, MaxX, MaxY, maxZ);
        }

        /// <summary>
        /// 将该点移动到新的 X 位置（沿 X 轴投影）。
        /// </summary>
        public ExtremePoint MoveX(int newMinX)
        {
            return new ExtremePoint(newMinX, MinY, MinZ, MaxX, MaxY, MaxZ);
        }

        /// <summary>
        /// 将该点移动到新的 Y 位置（沿 Y 轴投影）。
        /// </summary>
        public ExtremePoint MoveY(int newMinY)
        {
            return new ExtremePoint(MinX, newMinY, MinZ, MaxX, MaxY, MaxZ);
        }

        /// <summary>
        /// 将该点移动到新的 Z 位置（沿 Z 轴投影）。
        /// </summary>
        public ExtremePoint MoveZ(int newMinZ)
        {
            return new ExtremePoint(MinX, MinY, newMinZ, MaxX, MaxY, MaxZ);
        }

        public int CompareTo(ExtremePoint other)
        {
            if (other == null) return 1;
            int c = MinX.CompareTo(other.MinX);
            if (c != 0) return c;
            c = MinY.CompareTo(other.MinY);
            if (c != 0) return c;
            c = MinZ.CompareTo(other.MinZ);
            if (c != 0) return c;
            c = MaxX.CompareTo(other.MaxX);
            if (c != 0) return c;
            c = MaxY.CompareTo(other.MaxY);
            if (c != 0) return c;
            return MaxZ.CompareTo(other.MaxZ);
        }

        public override string ToString()
        {
            return $"EP[({MinX},{MinY},{MinZ})-({MaxX},{MaxY},{MaxZ})]";
        }
    }

    /// <summary>
    /// 轻量级的 BoxStackValue 引用，用于极值点可放置性检查。
    /// </summary>
    public struct BoxStackValueRef
    {
        public int Dx;
        public int Dy;
        public int Dz;

        public BoxStackValueRef(int dx, int dy, int dz)
        {
            Dx = dx;
            Dy = dy;
            Dz = dz;
        }
    }
}
