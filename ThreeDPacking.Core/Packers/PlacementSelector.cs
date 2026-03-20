using System;
using System.Collections.Generic;
using ThreeDPacking.Core.Comparators;
using ThreeDPacking.Core.Models;
using ThreeDPacking.Core.Points;

namespace ThreeDPacking.Core.Packers
{
    /// <summary>
    /// 放置位置选择器，决定物品的最佳放置点
    /// </summary>
    public class PlacementSelector
    {
        //比较器注入：（体积 > 重量 > Z低 > 面积小）
        private readonly IComparer<PlacementCandidate> _placementComparer;
        private readonly IComparer<BoxItem> _boxItemComparer;

        public PlacementSelector(IComparer<PlacementCandidate> placementComparer, IComparer<BoxItem> boxItemComparer)
        {
            _placementComparer = placementComparer;
            _boxItemComparer = boxItemComparer;
        }

        /// <summary>
        /// 在所有可用点与可用物品中选择最佳放置。
        /// 策略：优先尽量铺满首层（Z=0），再向上层扩展。
        /// </summary>
        /// <param name="source">可用物品源。</param>
        /// <param name="calculator">包含可用极值点的计算器。</param>
        /// <param name="container">目标容器。</param>
        /// <param name="stack">容器内当前已放置结果。</param>
        /// <param name="remainingWeight">剩余承重。</param>
        /// <param name="remainingVolume">剩余体积。</param>
        /// <returns>最优放置；若无可行方案则返回 null。</returns>
        public Placement GetBestPlacement(
            BoxItemSource source,
            IPointCalculator calculator,
            Container container,
            PackStack stack,
            int remainingWeight,
            long remainingVolume)
        {
            // 先尝试在首层（Z=0）找放置，以最大化底面覆盖
            var firstLevelPlacement = GetBestFirstLevelPlacement(
                source, calculator, container, stack, remainingWeight, remainingVolume);
            
            // 若找到可用的首层放置，直接使用
            if (firstLevelPlacement != null)
                return firstLevelPlacement;
            
            // 否则回退到高层常规放置策略
            return GetBestHigherLevelPlacement(
                source, calculator, container, stack, remainingWeight, remainingVolume);
        }

        /// <summary>
        /// 专门为首层（Z=0）选择最佳放置。
        /// 优先底面积更大者，以提高首层覆盖率。
        /// </summary>
        private Placement GetBestFirstLevelPlacement(
            BoxItemSource source,
            IPointCalculator calculator,
            Container container,
            PackStack stack,
            int remainingWeight,
            long remainingVolume)
        {
            PlacementCandidate best = null;
            long bestArea = -1;

            int pointCount = calculator.PointCount;

            for (int pi = 0; pi < pointCount; pi++)
            {
                var point = calculator.GetPoint(pi);
                
                // 仅考虑首层点（Z=0）
                if (point.MinZ != 0)
                    continue;

                for (int bi = 0; bi < source.Size; bi++)
                {
                    var boxItem = source.Get(bi);
                    var box = boxItem.Box;

                    if (box.Volume > remainingVolume || box.Weight > remainingWeight)
                        continue;

                    var rotations = box.GetRotations(point.Dx, point.Dy, point.Dz);
                    if (rotations == null)
                        continue;

                    foreach (var sv in rotations)
                    {
                        var placement = new Placement(sv, point.MinX, point.MinY, point.MinZ, boxItem);
                        
                        // 校验放置后不越过容器边界
                        if (placement.AbsoluteEndX >= container.LoadDx ||
                            placement.AbsoluteEndY >= container.LoadDy ||
                            placement.AbsoluteEndZ >= container.LoadDz)
                            continue;
                        
                        // 校验放置后不与已有物品相交
                        if (stack?.Placements != null && stack.Placements.Count > 0)
                        {
                            bool intersects = false;
                            foreach (var existing in stack.Placements)
                            {
                                if (existing != null && placement.Intersects3D(existing))
                                {
                                    intersects = true;
                                    break;
                                }
                            }
                            if (intersects)
                                continue;

                            // 首层位于地面，无需支撑检查
                        }
                        
                        // 以底面积最大优先，提升首层铺满率
                        long area = sv.Area;
                        if (area > bestArea)
                        {
                            bestArea = area;
                            best = new PlacementCandidate(placement, point);
                        }
                    }
                }
            }

            return best?.Placement;
        }

        /// <summary>
        /// 为高层（Z>0）选择最佳放置。
        /// 使用标准放置比较器进行排序。
        /// </summary>
        private Placement GetBestHigherLevelPlacement(
            BoxItemSource source,
            IPointCalculator calculator,
            Container container,
            PackStack stack,
            int remainingWeight,
            long remainingVolume)
        {
            PlacementCandidate best = null;

            int pointCount = calculator.PointCount;

            for (int pi = 0; pi < pointCount; pi++)
            {
                var point = calculator.GetPoint(pi);

                for (int bi = 0; bi < source.Size; bi++)
                {
                    var boxItem = source.Get(bi);
                    var box = boxItem.Box;

                    if (box.Volume > remainingVolume || box.Weight > remainingWeight)
                        continue;

                    var rotations = box.GetRotations(point.Dx, point.Dy, point.Dz);
                    if (rotations == null)
                        continue;

                    foreach (var sv in rotations)
                    {
                        var placement = new Placement(sv, point.MinX, point.MinY, point.MinZ, boxItem);
                        
                        // 校验放置后不越过容器边界
                        if (placement.AbsoluteEndX >= container.LoadDx ||
                            placement.AbsoluteEndY >= container.LoadDy ||
                            placement.AbsoluteEndZ >= container.LoadDz)
                            continue;
                        
                        // 校验放置后不与已有物品相交
                        if (stack?.Placements != null && stack.Placements.Count > 0)
                        {
                            bool intersects = false;
                            foreach (var existing in stack.Placements)
                            {
                                if (existing != null && placement.Intersects3D(existing))
                                {
                                    intersects = true;
                                    break;
                                }
                            }
                            if (intersects)
                                continue;

                            // 校验放置后支撑是否充足
                            if (!HasSufficientSupport(placement, stack.Placements))
                                continue;
                        }
                        
                        var candidate = new PlacementCandidate(placement, point);

                        if (best == null || _placementComparer.Compare(candidate, best) < 0)
                        {
                            best = candidate;
                        }
                    }
                }
            }

            return best?.Placement;
        }

        /// <summary>
        /// 为新层的第一个物品选择最佳放置。
        /// 优先底面积更大者，形成更稳定的层基底。
        /// </summary>
        public Placement GetFirstPlacement(
            BoxItemSource source,
            IPointCalculator calculator,
            Container container,
            int remainingWeight,
            long remainingVolume)
        {
            Placement bestPlacement = null;
            long bestArea = -1;
            long bestVolume = -1;

            int pointCount = calculator.PointCount;

            for (int pi = 0; pi < pointCount; pi++)
            {
                var point = calculator.GetPoint(pi);

                for (int bi = 0; bi < source.Size; bi++)
                {
                    var boxItem = source.Get(bi);
                    var box = boxItem.Box;

                    if (box.Volume > remainingVolume || box.Weight > remainingWeight)
                        continue;

                    var rotations = box.GetRotations(point.Dx, point.Dy, point.Dz);
                    if (rotations == null)
                        continue;

                    foreach (var sv in rotations)
                    {
                        long area = sv.Area;
                        long volume = sv.Volume;
                        
                        // 先构造放置对象用于边界校验
                        var placement = new Placement(sv, point.MinX, point.MinY, point.MinZ, boxItem);
                        
                        // 校验放置后不越过容器边界
                        if (placement.AbsoluteEndX >= container.LoadDx ||
                            placement.AbsoluteEndY >= container.LoadDy ||
                            placement.AbsoluteEndZ >= container.LoadDz)
                            continue;

                        // 从容器获取 stack，用于碰撞与支撑校验
                        var stack = container.Stack;
                        
                        // 校验放置后不与已有物品相交
                        if (stack?.Placements != null && stack.Placements.Count > 0)
                        {
                            bool intersects = false;
                            foreach (var existing in stack.Placements)
                            {
                                if (existing != null && placement.Intersects3D(existing))
                                {
                                    intersects = true;
                                    break;
                                }
                            }
                            if (intersects)
                                continue;

                            // 校验放置后支撑是否充足
                            if (!HasSufficientSupport(placement, stack.Placements))
                                continue;
                        }

                        bool isBetter = false;
                        if (bestPlacement == null)
                        {
                            isBetter = true;
                        }
                        else if (area > bestArea)
                        {
                            isBetter = true;
                        }
                        else if (area == bestArea && volume > bestVolume)
                        {
                            isBetter = true;
                        }

                        if (isBetter)
                        {
                            bestPlacement = placement;
                            bestArea = area;
                            bestVolume = volume;
                        }
                    }
                }
            }

            return bestPlacement;
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
                    // 检查点是否在existing的X-Y范围内
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
            // 计算X方向重叠
            int overlapXStart = Math.Max(a.X, b.X);
            int overlapXEnd = Math.Min(a.AbsoluteEndX, b.AbsoluteEndX);
            int overlapX = overlapXEnd - overlapXStart + 1;

            // 计算Y方向重叠
            int overlapYStart = Math.Max(a.Y, b.Y);
            int overlapYEnd = Math.Min(a.AbsoluteEndY, b.AbsoluteEndY);
            int overlapY = overlapYEnd - overlapYStart + 1;

            if (overlapX <= 0 || overlapY <= 0)
                return 0;

            return (long)overlapX * overlapY;
        }
    }
}
