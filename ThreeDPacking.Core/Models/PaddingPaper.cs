using System;

namespace ThreeDPacking.Core.Models
{
    /// <summary>
    /// 填充纸模型（牛皮纸），用于填充装箱后的空余空间
    /// 特点：尺寸可灵活调整以适应不同空间，支持多种旋转方向
    /// 目标：尽可能填满所有空余空间
    /// </summary>
    public class PaddingPaper
    {
        /// <summary>
        /// 默认高度（60mm）
        /// </summary>
        public const int DefaultHeight = 60;

        /// <summary>
        /// 默认宽度（110mm）
        /// </summary>
        public const int DefaultWidth = 110;

        /// <summary>
        /// 最小尺寸（任何方向）
        /// </summary>
        public const int MinSize = 30;

        /// <summary>
        /// 牛皮纸长度的最小值（可变方向，>=200mm）
        /// </summary>
        public const int MinLength = 200;

        /// <summary>
        /// 填充纸的实际尺寸
        /// </summary>
        public int Dx { get; }
        public int Dy { get; }
        public int Dz { get; }

        /// <summary>
        /// 放置位置
        /// </summary>
        public int X { get; }
        public int Y { get; }
        public int Z { get; }

        /// <summary>
        /// 体积
        /// </summary>
        public long Volume => (long)Dx * Dy * Dz;

        public PaddingPaper(int x, int y, int z, int dx, int dy, int dz)
        {
            X = x;
            Y = y;
            Z = z;
            Dx = dx;
            Dy = dy;
            Dz = dz;
        }

        /// <summary>
        /// 创建一个适合指定空间的填充纸
        /// 牛皮纸规格：高度固定60，宽度固定110，长度可变
        /// 旋转策略：只有底面（长x宽）可以旋转，高度始终固定为60
        /// </summary>
        public static PaddingPaper CreateForSpace(int x, int y, int z, int maxDx, int maxDy, int maxDz)
        {
            // 最小空间要求：至少能容纳高度60和一个足够小的候选底面（后续还会按长度最小值筛选）
            if (maxDx < MinSize || maxDy < MinSize || maxDz < DefaultHeight)
                return null;
            
            PaddingPaper best = null;
            long bestVolume = 0;
            
            // 牛皮纸固定高度为60，放在Z方向
            int dz = DefaultHeight;
            
            // 情况1: 宽度160放在Y方向，长度(X方向)自适应
            // 底面尺寸: 长=maxDx, 宽=110
            if (maxDy >= DefaultWidth)
            {
                int dy = DefaultWidth;
                int dx = maxDx;
                if (dx >= MinLength)
                {
                    var paper = new PaddingPaper(x, y, z, dx, dy, dz);
                    if (paper.Volume > bestVolume) { best = paper; bestVolume = paper.Volume; }
                }
            }
            
            // 情况2: 宽度110放在X方向，长度(Y方向)自适应
            // 底面尺寸: 长=110, 宽=maxDy
            if (maxDx >= DefaultWidth)
            {
                int dx = DefaultWidth;
                int dy = maxDy;
                if (dy >= MinLength)
                {
                    var paper = new PaddingPaper(x, y, z, dx, dy, dz);
                    if (paper.Volume > bestVolume) { best = paper; bestVolume = paper.Volume; }
                }
            }
            
            return best;
        }

        /// <summary>
        /// 转换为Placement对象用于存储和渲染
        /// </summary>
        public Placement ToPlacement()
        {
            // 创建一个虚拟的BoxStackValue来表示填充纸
            var stackValue = new BoxStackValue(Dx, Dy, Dz, -1); // Index=-1表示是填充物
            var placement = new Placement(stackValue, X, Y, Z, null);
            placement.IsPadding = true;
            return placement;
        }

        public int AbsoluteEndX => X + Dx - 1;
        public int AbsoluteEndY => Y + Dy - 1;
        public int AbsoluteEndZ => Z + Dz - 1;

        /// <summary>
        /// 检查是否与指定放置位置3D相交
        /// </summary>
        public bool Intersects3D(Placement other)
        {
            return !(other.AbsoluteEndX < X || other.AbsoluteX > AbsoluteEndX ||
                     other.AbsoluteEndY < Y || other.AbsoluteY > AbsoluteEndY ||
                     other.AbsoluteEndZ < Z || other.AbsoluteZ > AbsoluteEndZ);
        }

        /// <summary>
        /// 检查是否与另一个填充纸3D相交
        /// </summary>
        public bool Intersects3D(PaddingPaper other)
        {
            return !(other.AbsoluteEndX < X || other.X > AbsoluteEndX ||
                     other.AbsoluteEndY < Y || other.Y > AbsoluteEndY ||
                     other.AbsoluteEndZ < Z || other.Z > AbsoluteEndZ);
        }

        public override string ToString()
        {
            return $"PaddingPaper[({X},{Y},{Z}) {Dx}x{Dy}x{Dz}]";
        }
    }
}
