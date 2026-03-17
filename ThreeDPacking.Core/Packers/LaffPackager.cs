using System;
using System.Collections.Generic;
using ThreeDPacking.Core.Comparators;
using ThreeDPacking.Core.Models;
using ThreeDPacking.Core.Points;

namespace ThreeDPacking.Core.Packers
{
    /// <summary>
    /// 核心算法
    /// 每次新起一层时，先选底面积最大的箱子（_firstSelector 使用 LargestAreaBoxItemComparer）
    /// 把它放在当前层能放的最左下位置（或排序靠前的点）
    /// 用这个箱子定义当前层的地板高度
    /// 后续所有箱子都必须放在这一层（Z坐标相同），直到这一层放不下任何剩余箱子
    /// 再起新层，重复以上步骤
    /// </summary>
    public class LaffPackager : IPackager
    {
        private readonly PlacementSelector _selector;
        private readonly PlacementSelector _firstSelector;

        public LaffPackager()
        {
            _selector = new PlacementSelector(
                new VolumeWeightAreaPointComparer(),
                new LargestAreaBoxItemComparer());
            _firstSelector = new PlacementSelector(
                new VolumeWeightAreaPointComparer(),
                new LargestAreaBoxItemComparer());
        }

        public Container Pack(List<BoxItem> items, Container container)
        {
            var result = container.Clone();
            var stack = result.Stack;
            var source = new BoxItemSource(items);

            var pointCalc = new PointCalculator2D();
            pointCalc.ClearToSize(container.LoadDx, container.LoadDy, container.LoadDz);

            // Remove items that don't fit
            for (int i = source.Size - 1; i >= 0; i--)
            {
                if (!container.FitsInside(source.Get(i).Box))
                    source.Remove(i);
            }

            if (source.IsEmpty)
                return result;

            pointCalc.SetMinimumAreaAndVolumeLimit(source.GetMinArea(), source.GetMinVolume());

            int remainingWeight = container.MaxLoadWeight;
            long remainingVolume = container.MaxLoadVolume;
            int levelOffset = 0;
            bool newLevel = true;

            while (remainingWeight > 0 && remainingVolume > 0 && !source.IsEmpty)
            {
                Placement placement;

                if (newLevel)
                {
                    // Get first box for new level - prefer largest area
                    // 使用 result 而不是 container，以便支撑检查能访问已放置的物品
                    placement = _firstSelector.GetFirstPlacement(
                        source, pointCalc, result, remainingWeight, remainingVolume);

                    if (placement == null)
                        break;

                    // 对新层级的第一个物品也进行支撑检查（如果不是地面层）
                    if (levelOffset > 0)
                    {
                        if (!HasSufficientSupport(placement, stack.Placements))
                        {
                            // 支撑不足，从候选源中临时移除此物品，尝试其他物品
                            // 找到并移除该物品，避免重复尝试
                            for (int i = 0; i < source.Size; i++)
                            {
                                if (source.Get(i).Box.Id == placement.BoxItem.Box.Id)
                                {
                                    source.Remove(i);
                                    break;
                                }
                            }
                            continue;
                        }
                    }

                    // Create the level floor
                    int layerHeight = placement.StackValue.Dz;
                    var levelFloor = new ExtremePoint(
                        0, 0, levelOffset,
                        container.LoadDx - 1, container.LoadDy - 1,
                        layerHeight - 1 + levelOffset);

                    pointCalc.SetPoints(new List<ExtremePoint> { levelFloor });
                    pointCalc.Clear();

                    levelOffset += layerHeight;
                    newLevel = false;
                }
                else
                {
                    // Continue filling current level
                    // 使用 result 而不是 container，以便支撑检查能访问已放置的物品
                    placement = _selector.GetBestPlacement(
                        source, pointCalc, result, stack, remainingWeight, remainingVolume);

                    if (placement == null)
                    {
                        newLevel = true;

                        int remainingDz = container.LoadDz - levelOffset;
                        if (remainingDz <= 0)
                            break;

                        // Prepare points for new level spanning remaining height
                        var levelFloor = new ExtremePoint(
                            0, 0, levelOffset,
                            container.LoadDx - 1, container.LoadDy - 1,
                            container.LoadDz - 1);

                        pointCalc.SetPoints(new List<ExtremePoint> { levelFloor });
                        pointCalc.Clear();

                        // Remove items too big for remaining space
                        long maxArea = pointCalc.GetMaxArea();
                        long maxVolume = pointCalc.GetMaxVolume();
                        for (int i = source.Size - 1; i >= 0; i--)
                        {
                            var box = source.Get(i).Box;
                            if (box.Volume > maxVolume || box.GetMinimumAreaValue() > maxArea)
                                source.Remove(i);
                        }

                        continue;
                    }
                }

                // 对非地面层级的放置进行额外的支撑检查
                if (placement.Z > 0)
                {
                    if (!HasSufficientSupport(placement, stack.Placements))
                    {
                        // 支撑不足，跳过此放置并继续尝试其他位置
                        continue;
                    }
                }

                // Find point index
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

                // Find and decrement box item
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
                        pointCalc.SetMinimumAreaAndVolumeLimit(source.GetMinArea(), source.GetMinVolume());
                }
            }

            return result;
        }

        /// <summary>
        /// 检查放置位置是否有足够的支撑
        /// 允许最多50%悬空：要求至少50%底面被支撑
        /// 1. 底面至少50%被下层物体直接支撑
        /// 2. 底面中心点必须被支撑
        /// </summary>
        private bool HasSufficientSupport(Placement placement, List<Placement> existingPlacements)
        {
            // 如果放置在地面(Z=0)，则完全支撑
            if (placement.Z == 0)
                return true;

            // 计算放置物品的底面积
            long bottomArea = (long)placement.StackValue.Dx * placement.StackValue.Dy;
            long supportedArea = 0;

            // 检查与所有已放置物品的重叠
            int placementBottomZ = placement.Z;

            foreach (var existing in existingPlacements)
            {
                if (existing == null) continue;

                // 必须紧贴上一层（existing的顶部必须正好在placement底部下方）
                if (existing.AbsoluteEndZ == placementBottomZ - 1)
                {
                    // 计算2D重叠面积（X-Y平面）
                    long overlapArea = CalculateOverlapArea2D(placement, existing);
                    supportedArea += overlapArea;
                }
            }

            // 条件1：支撑面积必须至少50%（允许最多50%悬空）
            if (supportedArea * 100 < bottomArea * 50)
                return false;

            // 条件2：底面中心点必须被支撑
            int centerX = placement.X + placement.StackValue.Dx / 2;
            int centerY = placement.Y + placement.StackValue.Dy / 2;

            if (!IsPointSupported(centerX, centerY, placementBottomZ, existingPlacements))
                return false;

            return true;
        }

        /// <summary>
        /// 检查一个点是否被下方任何物体支撑
        /// ★★★ 关键修复：必须紧贴上一层
        /// </summary>
        private bool IsPointSupported(int x, int y, int placementBottomZ, List<Placement> existingPlacements)
        {
            foreach (var existing in existingPlacements)
            {
                if (existing == null) continue;

                // 必须紧贴上一层（existing的顶部必须正好在placement底部下方）
                if (existing.AbsoluteEndZ == placementBottomZ - 1)
                {
                    if (x >= existing.X && x <= existing.AbsoluteEndX &&
                        y >= existing.Y && y <= existing.AbsoluteEndY)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 计算两个放置在X-Y平面上的重叠面积
        /// </summary>
        private long CalculateOverlapArea2D(Placement a, Placement b)
        {
            int overlapXStart = Math.Max(a.X, b.X);
            int overlapXEnd = Math.Min(a.AbsoluteEndX, b.AbsoluteEndX);
            int overlapX = overlapXEnd - overlapXStart + 1;

            int overlapYStart = Math.Max(a.Y, b.Y);
            int overlapYEnd = Math.Min(a.AbsoluteEndY, b.AbsoluteEndY);
            int overlapY = overlapYEnd - overlapYStart + 1;

            if (overlapX <= 0 || overlapY <= 0)
                return 0;

            return (long)overlapX * overlapY;
        }
    }
}
