using System;
using System.Collections.Generic;
using System.Linq;
using ThreeDPacking.Core.Comparators;
using ThreeDPacking.Core.Models;
using ThreeDPacking.Core.Points;

namespace ThreeDPacking.Core.Packers
{
    /// <summary>
    /// 混合装箱算法：高度分组 + 空隙回填 + 底部优先
    /// 策略：
    /// 1. 将物品按高度分组，同组物品高度相近
    /// 2. 每组用 LAFF 风格逐层填充（减少层内高度差）
    /// 3. 每层完成后，立即用小物品填充当前层的剩余空间（底部优先）
    /// 4. 扁平物品优先放在低层，而不是被推到高层
    /// 5. 充分利用空间的同时保持结构稳定性
    /// </summary>
    public class HybridPackager : IPackager
    {
        private readonly PlacementSelector _selector;
        private readonly PlacementSelector _firstSelector;

        // 高度分组的容差比例（同组物品高度差不超过此比例）
        private const double HeightGroupTolerance = 0.3; // 30%

        // 底部优先：限制回填只在前N层进行，避免扁平物品被放到太高的位置
        private const int MaxGapFillLayers = 2; // 只在前2层进行空隙回填

        public HybridPackager()
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

            // 移除无法放入容器的物品
            for (int i = source.Size - 1; i >= 0; i--)
            {
                if (!container.FitsInside(source.Get(i).Box))
                    source.Remove(i);
            }

            if (source.IsEmpty)
                return result;

            int remainingWeight = container.MaxLoadWeight;
            long remainingVolume = container.MaxLoadVolume;
            int levelOffset = 0;

            int currentLayer = 0;

            // 主循环：按高度分组逐层填充
            while (remainingWeight > 0 && remainingVolume > 0 && !source.IsEmpty)
            {
                int remainingDz = container.LoadDz - levelOffset;
                if (remainingDz <= 0)
                    break;

                // 步骤1：获取当前可用物品并按高度分组
                var availableItems = GetAvailableItems(source, remainingWeight, remainingVolume, remainingDz);
                if (availableItems.Count == 0)
                    break;

                // 步骤2：选择最佳高度组
                // 底部优先原则：前两层优先选择高度较大的物品组，留出空间给扁平物品
                var heightGroups = GroupItemsByHeight(availableItems);
                var selectedGroup = SelectBestHeightGroup(heightGroups, remainingDz, currentLayer);

                if (selectedGroup.Count == 0)
                    break;

                // 步骤3：用选定组的物品填充当前层
                int layerHeight = GetGroupMaxHeight(selectedGroup);
                int itemsPackedInLayer = PackLayer(
                    source, result, stack,
                    selectedGroup, levelOffset, layerHeight,
                    container, ref remainingWeight, ref remainingVolume);

                if (itemsPackedInLayer == 0)
                {
                    // 当前组无法放置，尝试下一组或结束
                    break;
                }

                // 步骤4：底部优先 - 在当前层完成后，立即尝试在该层的顶部放置扁平物品
                // 这样扁平物品会被放在第二层，而不是被推到更高的层
                if (currentLayer < MaxGapFillLayers)
                {
                    int flatItemsFilled = FillLayerTopWithFlatItems(
                        source, result, stack,
                        levelOffset, layerHeight,
                        container, ref remainingWeight, ref remainingVolume);
                }

                // 步骤5：回填当前层内部的高度差空隙（仅在前几层进行）
                if (currentLayer < MaxGapFillLayers)
                {
                    int gapsFilled = FillHeightGaps(
                        source, result, stack,
                        levelOffset, layerHeight,
                        container, ref remainingWeight, ref remainingVolume);
                }

                // 移动到下一层
                levelOffset += layerHeight;
                currentLayer++;
            }

            return result;
        }

        /// <summary>
        /// 获取当前可用的物品列表（满足重量、体积、高度限制）
        /// </summary>
        private List<BoxItem> GetAvailableItems(BoxItemSource source, int remainingWeight, long remainingVolume, int maxHeight)
        {
            var available = new List<BoxItem>();
            for (int i = 0; i < source.Size; i++)
            {
                var item = source.Get(i);
                var box = item.Box;
                if (box.Weight <= remainingWeight && box.Volume <= remainingVolume)
                {
                    // 检查是否有任何旋转方向的高度可以放入
                    // 从 StackValues 中获取最小高度
                    int minHeight = int.MaxValue;
                    foreach (var sv in box.StackValues)
                    {
                        if (sv.Dz < minHeight)
                            minHeight = sv.Dz;
                    }
                    if (minHeight <= maxHeight)
                    {
                        available.Add(item);
                    }
                }
            }
            return available;
        }

        /// <summary>
        /// 将物品按高度分组
        /// 同组物品的高度差不超过 HeightGroupTolerance
        /// </summary>
        private List<List<BoxItem>> GroupItemsByHeight(List<BoxItem> items)
        {
            if (items.Count == 0)
                return new List<List<BoxItem>>();

            // 按最小高度排序（物品可以旋转，取最小可能高度）
            var sortedItems = items
                .Select(i => new { 
                    Item = i, 
                    MinHeight = i.Box.StackValues.Min(sv => sv.Dz) 
                })
                .OrderBy(x => x.MinHeight)
                .ToList();

            var groups = new List<List<BoxItem>>();
            var currentGroup = new List<BoxItem>();
            int groupBaseHeight = sortedItems[0].MinHeight;

            foreach (var entry in sortedItems)
            {
                // 计算与组基准高度的差异
                double heightDiff = (double)(entry.MinHeight - groupBaseHeight) / groupBaseHeight;

                if (currentGroup.Count == 0 || heightDiff <= HeightGroupTolerance)
                {
                    currentGroup.Add(entry.Item);
                }
                else
                {
                    // 开始新组
                    groups.Add(currentGroup);
                    currentGroup = new List<BoxItem> { entry.Item };
                    groupBaseHeight = entry.MinHeight;
                }
            }

            if (currentGroup.Count > 0)
                groups.Add(currentGroup);

            return groups;
        }

        /// <summary>
        /// 选择最佳高度组
        /// 底部优先原则：前两层优先选择高度较大的物品组，留出空间给扁平物品
        /// </summary>
        private List<BoxItem> SelectBestHeightGroup(List<List<BoxItem>> groups, int remainingDz, int currentLayer)
        {
            if (groups.Count == 0)
                return new List<BoxItem>();

            List<BoxItem> bestGroup = null;
            double bestScore = -1;

            foreach (var group in groups)
            {
                int groupMaxHeight = GetGroupMaxHeight(group);
                if (groupMaxHeight > remainingDz)
                    continue; // 这组放不下

                // 计算组内总体积
                long totalVolume = group.Sum(i => i.Box.Volume * i.Count);

                // 高度利用率（组高度/剩余高度，越接近1越好）
                double heightUtilization = (double)groupMaxHeight / remainingDz;

                // 底部优先策略：
                // - 前两层：优先选择高度较大的物品组（heightBonus正向加成）
                // - 这样扁平物品会被留到后面，作为上层填充
                double heightBonus = 0;
                if (currentLayer < MaxGapFillLayers)
                {
                    // 前两层：高度越大越好（高度占比作为加成）
                    heightBonus = heightUtilization * 0.5;
                }

                // 综合评分：体积 + 高度利用率 + 底部优先加成
                double score = totalVolume * (0.3 + 0.3 * Math.Min(heightUtilization, 1.0) + heightBonus);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestGroup = group;
                }
            }

            return bestGroup ?? (groups.Count > 0 ? groups[0] : new List<BoxItem>());
        }

        /// <summary>
        /// 获取组内物品的最大高度（作为层高）
        /// </summary>
        private int GetGroupMaxHeight(List<BoxItem> group)
        {
            int maxHeight = 0;
            foreach (var item in group)
            {
                // 取物品可能的最小高度（因为会选择底面积最大的朝向）
                int itemHeight = item.Box.StackValues.Min(sv => sv.Dz);
                if (itemHeight > maxHeight)
                    maxHeight = itemHeight;
            }
            return maxHeight;
        }

        /// <summary>
        /// 在指定层内填充物品（LAFF风格）
        /// </summary>
        private int PackLayer(
            BoxItemSource source,
            Container result,
            PackStack stack,
            List<BoxItem> groupItems,
            int levelOffset,
            int layerHeight,
            Container container,
            ref int remainingWeight,
            ref long remainingVolume)
        {
            var pointCalc = new PointCalculator2D();
            pointCalc.ClearToSize(container.LoadDx, container.LoadDy, container.LoadDz);

            // 创建当前层的地板
            var levelFloor = new ExtremePoint(
                0, 0, levelOffset,
                container.LoadDx - 1, container.LoadDy - 1,
                layerHeight - 1 + levelOffset);
            pointCalc.SetPoints(new List<ExtremePoint> { levelFloor });
            pointCalc.Clear();

            // 创建组内物品的ID集合，用于筛选
            var groupItemIds = new HashSet<string>(groupItems.Select(i => i.Box.Id));

            int packedCount = 0;
            bool isFirstInLayer = true;

            while (remainingWeight > 0 && remainingVolume > 0 && !source.IsEmpty)
            {
                Placement placement;

                if (isFirstInLayer)
                {
                    // 第一个物品：优先选底面积最大的
                    placement = GetFirstPlacementFromGroup(
                        source, pointCalc, result, groupItemIds,
                        remainingWeight, remainingVolume, levelOffset, stack.Placements);

                    if (placement == null)
                        break;

                    isFirstInLayer = false;
                }
                else
                {
                    // 后续物品：贪心选择最佳位置
                    placement = GetBestPlacementFromGroup(
                        source, pointCalc, result, stack, groupItemIds,
                        remainingWeight, remainingVolume, levelOffset);

                    if (placement == null)
                        break; // 当前层已满
                }

                // 支撑检查（非地面层）
                if (placement.Z > 0 && !HasSufficientSupport(placement, stack.Placements))
                    continue;

                // 查找点索引
                int pointIndex = FindPointIndex(pointCalc, placement);
                if (pointIndex < 0)
                    break;

                // 放置物品
                stack.Add(placement);
                pointCalc.Add(pointIndex, placement);

                remainingWeight -= placement.StackValue.Box.Weight;
                remainingVolume -= placement.StackValue.Box.Volume;
                packedCount++;

                // 从源中减少数量
                DecrementItem(source, placement.BoxItem.Box.Id);

                // 清理无法放入的物品
                CleanupOversizedItems(source, remainingWeight, remainingVolume, pointCalc);
            }

            return packedCount;
        }

        /// <summary>
        /// 底部优先：在当前层顶部放置扁平物品
        /// 这样扁平物品会被放在第二层（当前层顶部），而不是被推到更高的层
        /// </summary>
        private int FillLayerTopWithFlatItems(
            BoxItemSource source,
            Container result,
            PackStack stack,
            int levelOffset,
            int layerHeight,
            Container container,
            ref int remainingWeight,
            ref long remainingVolume)
        {
            if (source.IsEmpty)
                return 0;

            int filledCount = 0;
            int layerTopZ = levelOffset + layerHeight;

            // 找出当前层所有已放置物品
            var layerPlacements = stack.Placements
                .Where(p => p != null && p.Z >= levelOffset && p.Z < layerTopZ)
                .ToList();

            if (layerPlacements.Count == 0)
                return 0;

            // 创建用于层顶部填充的点计算器
            var topPointCalc = new PointCalculator2D();
            topPointCalc.ClearToSize(container.LoadDx, container.LoadDy, container.LoadDz);

            // 创建层顶部的地板
            var topFloor = new ExtremePoint(
                0, 0, layerTopZ,
                container.LoadDx - 1, container.LoadDy - 1,
                container.LoadDz - 1);
            topPointCalc.SetPoints(new List<ExtremePoint> { topFloor });
            topPointCalc.Clear();

            // 找出扁平物品（高度较小的物品）
            var flatItems = new List<BoxItem>();
            int avgHeight = layerHeight / 2; // 高度小于层高一半的视为扁平物品

            for (int i = 0; i < source.Size; i++)
            {
                var item = source.Get(i);
                var box = item.Box;
                if (box.Weight > remainingWeight || box.Volume > remainingVolume)
                    continue;

                int minHeight = item.Box.StackValues.Min(sv => sv.Dz);
                if (minHeight <= avgHeight)
                {
                    flatItems.Add(item);
                }
            }

            if (flatItems.Count == 0)
                return 0;

            var flatItemIds = new HashSet<string>(flatItems.Select(i => i.Box.Id));

            // 尝试在层顶部放置扁平物品
            bool keepTrying = true;
            while (keepTrying && remainingWeight > 0 && remainingVolume > 0 && !source.IsEmpty)
            {
                keepTrying = false;

                var placement = GetBestPlacementFromGroup(
                    source, topPointCalc, result, stack, flatItemIds,
                    remainingWeight, remainingVolume, layerTopZ);

                if (placement != null)
                {
                    // 检查是否与已有物品碰撞
                    if (!HasCollision(placement, stack.Placements))
                    {
                        // 检查支撑
                        if (HasSufficientSupport(placement, stack.Placements))
                        {
                            int pointIndex = FindPointIndex(topPointCalc, placement);
                            if (pointIndex >= 0)
                            {
                                stack.Add(placement);
                                topPointCalc.Add(pointIndex, placement);

                                remainingWeight -= placement.StackValue.Box.Weight;
                                remainingVolume -= placement.StackValue.Box.Volume;
                                filledCount++;

                                DecrementItem(source, placement.BoxItem.Box.Id);
                                keepTrying = true;
                            }
                        }
                    }
                }
            }

            return filledCount;
        }

        /// <summary>
        /// 回填当前层的高度差空隙
        /// </summary>
        private int FillHeightGaps(
            BoxItemSource source,
            Container result,
            PackStack stack,
            int levelOffset,
            int layerHeight,
            Container container,
            ref int remainingWeight,
            ref long remainingVolume)
        {
            if (source.IsEmpty)
                return 0;

            int filledCount = 0;
            int layerTopZ = levelOffset + layerHeight;

            // 找出当前层所有已放置物品
            var layerPlacements = stack.Placements
                .Where(p => p != null && p.Z >= levelOffset && p.Z < layerTopZ)
                .ToList();

            if (layerPlacements.Count == 0)
                return 0;

            // 对每个放置的物品，检查其顶部是否有空隙
            foreach (var basePlacement in layerPlacements)
            {
                int gapStartZ = basePlacement.AbsoluteEndZ + 1;
                int gapHeight = layerTopZ - gapStartZ;

                if (gapHeight <= 0)
                    continue; // 没有空隙

                // 尝试找一个能填入空隙的小物品
                var gapPlacement = FindItemForGap(
                    source, result, stack,
                    basePlacement.X, basePlacement.Y, gapStartZ,
                    basePlacement.StackValue.Dx, basePlacement.StackValue.Dy, gapHeight,
                    container, remainingWeight, remainingVolume);

                if (gapPlacement != null)
                {
                    stack.Add(gapPlacement);
                    remainingWeight -= gapPlacement.StackValue.Box.Weight;
                    remainingVolume -= gapPlacement.StackValue.Box.Volume;
                    filledCount++;

                    DecrementItem(source, gapPlacement.BoxItem.Box.Id);
                }
            }

            return filledCount;
        }

        /// <summary>
        /// 找一个能填入指定空隙的物品
        /// </summary>
        private Placement FindItemForGap(
            BoxItemSource source,
            Container result,
            PackStack stack,
            int gapX, int gapY, int gapZ,
            int gapDx, int gapDy, int gapDz,
            Container container,
            int remainingWeight, long remainingVolume)
        {
            Placement bestPlacement = null;
            long bestVolume = 0;

            for (int i = 0; i < source.Size; i++)
            {
                var boxItem = source.Get(i);
                var box = boxItem.Box;

                if (box.Weight > remainingWeight || box.Volume > remainingVolume)
                    continue;

                // 尝试所有旋转方向
                var rotations = box.GetRotations(gapDx, gapDy, gapDz);
                if (rotations == null)
                    continue;

                foreach (var sv in rotations)
                {
                    // 检查是否能放入空隙
                    if (sv.Dx > gapDx || sv.Dy > gapDy || sv.Dz > gapDz)
                        continue;

                    var placement = new Placement(sv, gapX, gapY, gapZ, boxItem);

                    // 验证边界
                    if (placement.AbsoluteEndX >= container.LoadDx ||
                        placement.AbsoluteEndY >= container.LoadDy ||
                        placement.AbsoluteEndZ >= container.LoadDz)
                        continue;

                    // 验证不与已有物品碰撞
                    if (HasCollision(placement, stack.Placements))
                        continue;

                    // 验证支撑（空隙填充物品应该总是有支撑，因为下面就是basePlacement）
                    if (!HasSufficientSupport(placement, stack.Placements))
                        continue;

                    // 选择体积最大的物品填充
                    if (sv.Volume > bestVolume)
                    {
                        bestVolume = sv.Volume;
                        bestPlacement = placement;
                    }
                }
            }

            return bestPlacement;
        }

        #region Helper Methods

        private Placement GetFirstPlacementFromGroup(
            BoxItemSource source,
            IPointCalculator calculator,
            Container container,
            HashSet<string> groupItemIds,
            int remainingWeight,
            long remainingVolume,
            int levelOffset,
            List<Placement> existingPlacements)
        {
            Placement bestPlacement = null;
            long bestArea = -1;

            int pointCount = calculator.PointCount;

            for (int pi = 0; pi < pointCount; pi++)
            {
                var point = calculator.GetPoint(pi);

                for (int bi = 0; bi < source.Size; bi++)
                {
                    var boxItem = source.Get(bi);
                    if (!groupItemIds.Contains(boxItem.Box.Id))
                        continue;

                    var box = boxItem.Box;
                    if (box.Volume > remainingVolume || box.Weight > remainingWeight)
                        continue;

                    var rotations = box.GetRotations(point.Dx, point.Dy, point.Dz);
                    if (rotations == null)
                        continue;

                    foreach (var sv in rotations)
                    {
                        var placement = new Placement(sv, point.MinX, point.MinY, point.MinZ, boxItem);

                        if (placement.AbsoluteEndX >= container.LoadDx ||
                            placement.AbsoluteEndY >= container.LoadDy ||
                            placement.AbsoluteEndZ >= container.LoadDz)
                            continue;

                        if (HasCollision(placement, existingPlacements))
                            continue;

                        // 非地面层需要支撑检查
                        if (levelOffset > 0 && !HasSufficientSupport(placement, existingPlacements))
                            continue;

                        long area = sv.Area;
                        if (area > bestArea)
                        {
                            bestArea = area;
                            bestPlacement = placement;
                        }
                    }
                }
            }

            return bestPlacement;
        }

        private Placement GetBestPlacementFromGroup(
            BoxItemSource source,
            IPointCalculator calculator,
            Container container,
            PackStack stack,
            HashSet<string> groupItemIds,
            int remainingWeight,
            long remainingVolume,
            int levelOffset)
        {
            Placement bestPlacement = null;
            long bestArea = -1;

            int pointCount = calculator.PointCount;

            for (int pi = 0; pi < pointCount; pi++)
            {
                var point = calculator.GetPoint(pi);

                for (int bi = 0; bi < source.Size; bi++)
                {
                    var boxItem = source.Get(bi);
                    if (!groupItemIds.Contains(boxItem.Box.Id))
                        continue;

                    var box = boxItem.Box;
                    if (box.Volume > remainingVolume || box.Weight > remainingWeight)
                        continue;

                    var rotations = box.GetRotations(point.Dx, point.Dy, point.Dz);
                    if (rotations == null)
                        continue;

                    foreach (var sv in rotations)
                    {
                        var placement = new Placement(sv, point.MinX, point.MinY, point.MinZ, boxItem);

                        if (placement.AbsoluteEndX >= container.LoadDx ||
                            placement.AbsoluteEndY >= container.LoadDy ||
                            placement.AbsoluteEndZ >= container.LoadDz)
                            continue;

                        if (HasCollision(placement, stack.Placements))
                            continue;

                        if (!HasSufficientSupport(placement, stack.Placements))
                            continue;

                        long area = sv.Area;
                        if (area > bestArea)
                        {
                            bestArea = area;
                            bestPlacement = placement;
                        }
                    }
                }
            }

            return bestPlacement;
        }

        private int FindPointIndex(IPointCalculator calculator, Placement placement)
        {
            for (int i = 0; i < calculator.PointCount; i++)
            {
                var pt = calculator.GetPoint(i);
                if (pt.MinX == placement.X && pt.MinY == placement.Y && pt.MinZ == placement.Z)
                    return i;
            }
            return -1;
        }

        private void DecrementItem(BoxItemSource source, string boxId)
        {
            for (int i = 0; i < source.Size; i++)
            {
                if (source.Get(i).Box.Id == boxId)
                {
                    source.Decrement(i, 1);
                    break;
                }
            }
        }

        private void CleanupOversizedItems(BoxItemSource source, int remainingWeight, long remainingVolume, IPointCalculator pointCalc)
        {
            if (source.IsEmpty)
                return;

            for (int i = source.Size - 1; i >= 0; i--)
            {
                var box = source.Get(i).Box;
                if (box.Volume > remainingVolume || box.Weight > remainingWeight)
                    source.Remove(i);
            }

            if (!source.IsEmpty)
                pointCalc.SetMinimumAreaAndVolumeLimit(source.GetMinArea(), source.GetMinVolume());
        }

        private bool HasCollision(Placement placement, List<Placement> existingPlacements)
        {
            foreach (var existing in existingPlacements)
            {
                if (existing != null && placement.Intersects3D(existing))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 检查放置位置是否有足够的支撑
        /// 要求至少50%底面被支撑 + 中心点必须被支撑
        /// </summary>
        private bool HasSufficientSupport(Placement placement, List<Placement> existingPlacements)
        {
            if (placement.Z == 0)
                return true;

            long bottomArea = (long)placement.StackValue.Dx * placement.StackValue.Dy;
            long supportedArea = 0;
            int placementBottomZ = placement.Z;

            foreach (var existing in existingPlacements)
            {
                if (existing == null) continue;

                if (existing.AbsoluteEndZ == placementBottomZ - 1)
                {
                    long overlapArea = CalculateOverlapArea2D(placement, existing);
                    supportedArea += overlapArea;
                }
            }

            // 条件1：支撑面积必须至少50%
            if (supportedArea * 100 < bottomArea * 50)
                return false;

            // 条件2：底面中心点必须被支撑
            int centerX = placement.X + placement.StackValue.Dx / 2;
            int centerY = placement.Y + placement.StackValue.Dy / 2;

            if (!IsPointSupported(centerX, centerY, placementBottomZ, existingPlacements))
                return false;

            return true;
        }

        private bool IsPointSupported(int x, int y, int placementBottomZ, List<Placement> existingPlacements)
        {
            foreach (var existing in existingPlacements)
            {
                if (existing == null) continue;

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

        #endregion
    }
}
