using System;
using System.Collections.Generic;
using ThreeDPacking.Core.Comparators;
using ThreeDPacking.Core.Models;
using ThreeDPacking.Core.Points;

namespace ThreeDPacking.Core.Packers
{
    /// <summary>
    /// 基础装箱算法实现：不刻意分层，全空间极端点搜索 + 贪心放置
    /// 初始化一个覆盖整个容器的大点 (0,0,0) ~ (maxX,maxY,maxZ)
    /// 每次选一个箱子 → 尝试所有极端点 → 选“最好”的点放置
    /// 放置后：删除被占用的点
    /// 在箱子右侧、前方、上方生成最多3个新极端点
    /// 执行约束切割（ConstrainPoints)
    /// 删除被完全包含的点（RemoveEclipsedPoints）
    /// </summary>
    public class PlainPackager : IPackager
    {
        private readonly PlacementSelector _selector;

        public PlainPackager()
        {
            _selector = new PlacementSelector(
                new VolumeWeightAreaPointComparer(),
                new LargestAreaBoxItemComparer());
        }

        public PlainPackager(PlacementSelector selector)
        {
            _selector = selector;
        }

        public Container Pack(List<BoxItem> items, Container container)
        {
            var result = container.Clone();
            var stack = result.Stack;
            var source = new BoxItemSource(items);

            var pointCalc = new PointCalculator3D();
            pointCalc.ClearToSize(container.LoadDx, container.LoadDy, container.LoadDz);

            // Remove items that don't fit the container at all
            for (int i = source.Size - 1; i >= 0; i--)
            {
                var boxItem = source.Get(i);
                if (!container.FitsInside(boxItem.Box))
                    source.Remove(i);
            }

            if (source.IsEmpty)
                return result;

            pointCalc.SetMinimumAreaAndVolumeLimit(source.GetMinArea(), source.GetMinVolume());

            int remainingWeight = container.MaxLoadWeight;
            long remainingVolume = container.MaxLoadVolume;

            while (remainingWeight > 0 && remainingVolume > 0 && !source.IsEmpty && !pointCalc.IsEmpty)
            {
                // 使用 result 而不是 container，以便支撑检查能访问已放置的物品
                var placement = _selector.GetBestPlacement(
                    source, pointCalc, result, stack, remainingWeight, remainingVolume);

                if (placement == null)
                    break;



                // Find the point index used
                int pointIndex = -1;
                for (int i = 0; i < pointCalc.PointCount; i++)
                {
                    var pt = pointCalc.GetPoint(i);
                    if (pt.MinX == placement.X && pt.MinY == placement.Y && pt.MinZ == placement.Z)
                    {
                        pointIndex = i;
                        break;
                    }
                }

                if (pointIndex < 0)
                    break;

                stack.Add(placement);
                pointCalc.Add(pointIndex, placement);

                remainingWeight -= placement.StackValue.Box.Weight;
                remainingVolume -= placement.StackValue.Box.Volume;

                // Find and decrement the box item
                for (int i = 0; i < source.Size; i++)
                {
                    if (source.Get(i).Box.Id == placement.BoxItem.Box.Id)
                    {
                        source.Decrement(i, 1);
                        break;
                    }
                }

                if (!source.IsEmpty)
                {
                    // Remove items too big for remaining capacity
                    for (int i = source.Size - 1; i >= 0; i--)
                    {
                        var box = source.Get(i).Box;
                        if (box.Volume > remainingVolume || box.Weight > remainingWeight)
                            source.Remove(i);
                    }

                    if (!source.IsEmpty)
                    {
                        pointCalc.SetMinimumAreaAndVolumeLimit(source.GetMinArea(), source.GetMinVolume());

                        // Remove items too big for available points
                        long maxPointArea = pointCalc.GetMaxArea();
                        long maxPointVolume = pointCalc.GetMaxVolume();
                        for (int i = source.Size - 1; i >= 0; i--)
                        {
                            var box = source.Get(i).Box;
                            if (box.Volume > maxPointVolume || box.GetMinimumAreaValue() > maxPointArea)
                                source.Remove(i);
                        }
                    }
                }
            }

            return result;
        }
    }
}
