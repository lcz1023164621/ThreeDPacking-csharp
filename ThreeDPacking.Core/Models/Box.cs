using System;
using System.Collections.Generic;

namespace ThreeDPacking.Core.Models
{
    /// <summary>
    /// 箱子（容器）模型
    /// </summary>
    public class Box
    {
        public string Id { get; }
        public string Description { get; }
        public int Weight { get; }
        public long Volume { get; }
        //核心旋转集合。根据rotate3D参数自动生成所有合法旋转姿态（最多6种)
        public BoxStackValue[] StackValues { get; }
        //最小底面积的旋转姿态
        public BoxStackValue MinimumArea { get; }
        //最大底面积的旋转姿态
        public BoxStackValue MaximumArea { get; }
        public BoxItem BoxItem { get; set; }

        public Box(string id, string description, int dx, int dy, int dz, int weight, bool rotate3D)
        {
            Id = id;
            Description = description;
            Weight = weight;
            Volume = (long)dx * dy * dz;
            StackValues = ComputeStackValues(dx, dy, dz, rotate3D);

            MinimumArea = GetMinimumArea(StackValues);
            MaximumArea = GetMaximumArea(StackValues);

            foreach (var sv in StackValues)
                sv.Box = this;
        }

        private static BoxStackValue[] ComputeStackValues(int dx, int dy, int dz, bool rotate3D)
        {
            var list = new List<BoxStackValue>();

            if (dx == dy && dx == dz)
            {
                // Cube: all sides equal, only 1 rotation needed
                list.Add(new BoxStackValue(dx, dy, dz, list.Count));
            }
            else if (!rotate3D)
            {
                // 2D rotation only: keep height fixed, rotate base (dx, dy) if different
                // This maximizes volume utilization by allowing width/length swap
                list.Add(new BoxStackValue(dx, dy, dz, list.Count));
                if (dx != dy)
                {
                    list.Add(new BoxStackValue(dy, dx, dz, list.Count)); // Base rotation: swap length and width
                }
            }
            else if (dx == dy)
            {
                // Two square sides (bottom square)
                list.Add(new BoxStackValue(dx, dy, dz, list.Count)); // XY
                list.Add(new BoxStackValue(dx, dz, dx, list.Count)); // XZ 0
                list.Add(new BoxStackValue(dz, dx, dx, list.Count)); // XZ 90
            }
            else if (dz == dy)
            {
                // Two square sides (side square)
                list.Add(new BoxStackValue(dy, dy, dx, list.Count)); // YZ
                list.Add(new BoxStackValue(dx, dz, dz, list.Count)); // XY+XZ 0
                list.Add(new BoxStackValue(dz, dx, dz, list.Count)); // XY+XZ 90
            }
            else if (dx == dz)
            {
                // Two square sides (front square)
                list.Add(new BoxStackValue(dx, dx, dy, list.Count)); // XZ
                list.Add(new BoxStackValue(dx, dy, dx, list.Count)); // XY+YZ 0
                list.Add(new BoxStackValue(dy, dx, dx, list.Count)); // XY+YZ 90
            }
            else
            {
                // No equal edges: 6 rotations
                list.Add(new BoxStackValue(dx, dy, dz, list.Count)); // XY 0
                list.Add(new BoxStackValue(dy, dx, dz, list.Count)); // XY 90
                list.Add(new BoxStackValue(dx, dz, dy, list.Count)); // XZ 0
                list.Add(new BoxStackValue(dz, dx, dy, list.Count)); // XZ 90
                list.Add(new BoxStackValue(dz, dy, dx, list.Count)); // YZ 0
                list.Add(new BoxStackValue(dy, dz, dx, list.Count)); // YZ 90
            }

            if (list.Count == 0)
                throw new InvalidOperationException("Expected at least one stackable surface");

            return list.ToArray();
        }
        /// <summary>
        /// 判断这个物体（考虑旋转）是否能完整放入一个给定的的内部空间尺寸中
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <param name="dz"></param>
        /// <returns></returns>
        public bool FitsInside(int dx, int dy, int dz)
        {
            foreach (var sv in StackValues)
            {
                if (sv.FitsInside3D(dx, dy, dz))
                    return true;
            }
            return false;
        }
        /// <summary>
        /// 获取这个容器内部能够容纳的所有哈法旋转方案
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <param name="dz"></param>
        /// <returns></returns>
        public List<BoxStackValue> GetRotations(int dx, int dy, int dz)
        {
            var result = new List<BoxStackValue>();
            foreach (var sv in StackValues)
            {
                if (sv.FitsInside3D(dx, dy, dz))
                    result.Add(sv);
            }
            return result.Count > 0 ? result : null;
        }
        /// <summary>
        /// 或许可能旋转中底面积最小的方案
        /// </summary>
        /// <returns></returns>
        public long GetMinimumAreaValue()
        {
            return MinimumArea.Area;
        }
        /// <summary>
        /// 或许可能旋转中底面积最小的方案
        /// </summary>
        /// <returns></returns>
        public long GetMaximumAreaValue()
        {
            return MaximumArea.Area;
        }

        private static BoxStackValue GetMinimumArea(BoxStackValue[] values)
        {
            BoxStackValue min = null;
            foreach (var v in values)
            {
                if (min == null || v.Area < min.Area)
                    min = v;
            }
            return min;
        }

        private static BoxStackValue GetMaximumArea(BoxStackValue[] values)
        {
            BoxStackValue max = null;
            foreach (var v in values)
            {
                if (max == null || v.Area > max.Area)
                    max = v;
            }
            return max;
        }

        public override string ToString()
        {
            return $"Box {Id} ({Description}) [{Weight}w, {Volume}v]";
        }
    }
}
