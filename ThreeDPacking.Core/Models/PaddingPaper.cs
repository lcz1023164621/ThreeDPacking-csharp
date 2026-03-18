using System;

namespace ThreeDPacking.Core.Models
{
    /// <summary>
    /// 填充纸模型（牛皮纸），用于填充装箱后的空余空间
    /// 特点：高度固定70，宽度固定160，长度可调整
    /// 填充纸以长度与宽度所在的面作为底面积（高度始终朝上）
    /// </summary>
    public class PaddingPaper
    {
        /// <summary>
        /// 固定高度（70mm）- 始终朝上(Z方向)
        /// </summary>
        public const int DefaultHeight = 70;

        /// <summary>
        /// 固定宽度（160mm）
        /// </summary>
        public const int DefaultWidth = 160;

        /// <summary>
        /// 最小长度
        /// </summary>
        public const int MinLength = 50;

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
        /// 填充纸尺寸：长度可自适应，宽度固定160，高度固定70
        /// 以长度与宽度所在的面作为底面积（高度始终朝上，即Z方向）
        /// 如果空间放不下标准尺寸则不填充
        /// </summary>
        /// <param name="x">放置位置X</param>
        /// <param name="y">放置位置Y</param>
        /// <param name="z">放置位置Z</param>
        /// <param name="maxDx">可用空间长度</param>
        /// <param name="maxDy">可用空间宽度</param>
        /// <param name="maxDz">可用空间高度</param>
        /// <returns>如果能放下则返回填充纸对象，否则返回null</returns>
        public static PaddingPaper CreateForSpace(int x, int y, int z, int maxDx, int maxDy, int maxDz)
        {
            // 调试输出
            Console.WriteLine($"[PaddingPaper] 检查空间: 位置({x},{y},{z}) 可用尺寸({maxDx}x{maxDy}x{maxDz}) 需求高度={DefaultHeight} 需求宽度={DefaultWidth}");
            
            // 高度固定为70，始终朝上(Z方向)
            // 只允许两种底面放置方式：宽度朝Y或宽度朝X
            
            // 方向1: 高度朝Z(70)，宽度朝Y(160)，长度朝X(自适应)
            if (maxDz >= DefaultHeight && maxDy >= DefaultWidth && maxDx >= MinLength)
            {
                int length = maxDx; // 长度填满可用空间
                Console.WriteLine($"[PaddingPaper] 创建成功(方向1): 尺寸({length}x{DefaultWidth}x{DefaultHeight})");
                return new PaddingPaper(x, y, z, length, DefaultWidth, DefaultHeight);
            }

            // 方向2: 高度朝Z(70)，宽度朝X(160)，长度朝Y(自适应)
            if (maxDz >= DefaultHeight && maxDx >= DefaultWidth && maxDy >= MinLength)
            {
                int length = maxDy;
                Console.WriteLine($"[PaddingPaper] 创建成功(方向2): 尺寸({DefaultWidth}x{length}x{DefaultHeight})");
                return new PaddingPaper(x, y, z, DefaultWidth, length, DefaultHeight);
            }

            // 空间太小，无法放置标准填充纸，不填充
            Console.WriteLine($"[PaddingPaper] 创建失败: 空间不足 (需要Z>={DefaultHeight}, X或Y>={DefaultWidth}, 另一边>={MinLength})");
            return null;
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
