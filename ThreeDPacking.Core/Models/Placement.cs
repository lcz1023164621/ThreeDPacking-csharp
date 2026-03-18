using System;
using System.Collections.Generic;

namespace ThreeDPacking.Core.Models
{
    /// <summary>
    /// 物品放置信息，记录物品在容器中的具体位置和旋转方式（哪个物体在哪怎么放）
    /// </summary>
    public class Placement
    {
        //本次使用的旋转姿态
        public BoxStackValue StackValue { get; set; }
        //左下角坐标
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        /// <summary>
        /// 指向被消耗的BoxItem实例（用于数量扣减）
        /// </summary>
        public BoxItem BoxItem { get; set; }

        /// <summary>
        /// 标识是否为填充物（如牛皮纸）
        /// </summary>
        public bool IsPadding { get; set; }

        public Placement() { }

        public Placement(BoxStackValue stackValue, int x, int y, int z, BoxItem boxItem)
        {
            StackValue = stackValue;
            X = x;
            Y = y;
            Z = z;
            BoxItem = boxItem;
        }

        public int AbsoluteX => X;
        public int AbsoluteY => Y;
        public int AbsoluteZ => Z;

        public int AbsoluteEndX => X + StackValue.Dx - 1;
        public int AbsoluteEndY => Y + StackValue.Dy - 1;
        public int AbsoluteEndZ => Z + StackValue.Dz - 1;

        public bool Intersects(Placement other)
        {
            return IntersectsX(other) && IntersectsY(other) && IntersectsZ(other);
        }

        public bool IntersectsX(Placement other)
        {
            return AbsoluteX <= other.AbsoluteEndX && other.AbsoluteX <= AbsoluteEndX;
        }

        public bool IntersectsY(Placement other)
        {
            return AbsoluteY <= other.AbsoluteEndY && other.AbsoluteY <= AbsoluteEndY;
        }

        public bool IntersectsZ(Placement other)
        {
            return AbsoluteZ <= other.AbsoluteEndZ && other.AbsoluteZ <= AbsoluteEndZ;
        }

        public bool Intersects2D(Placement other)
        {
            return !(other.AbsoluteEndX < X || other.AbsoluteX > AbsoluteEndX ||
                     other.AbsoluteEndY < Y || other.AbsoluteY > AbsoluteEndY);
        }

        public bool Intersects3D(Placement other)
        {
            return !(other.AbsoluteEndX < X || other.AbsoluteX > AbsoluteEndX ||
                     other.AbsoluteEndY < Y || other.AbsoluteY > AbsoluteEndY ||
                     other.AbsoluteEndZ < Z || other.AbsoluteZ > AbsoluteEndZ);
        }

        public override string ToString()
        {
            string boxId = StackValue?.Box?.Id ?? "";
            return $"{boxId}[{X}x{Y}x{Z} {AbsoluteEndX}x{AbsoluteEndY}x{AbsoluteEndZ}]";
        }
    }
}
