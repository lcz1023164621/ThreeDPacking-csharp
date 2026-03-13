using System;
using System.Collections.Generic;
using ThreeDPacking.Core.Comparators;
using ThreeDPacking.Core.Models;
using ThreeDPacking.Core.Points;

namespace ThreeDPacking.Core.Packers
{
    /// <summary>
    /// Selects the best placement for the next box item by iterating over
    /// all available points × all items × all rotations and choosing the best
    /// combination according to comparators.
    /// </summary>
    public class PlacementSelector
    {
        private readonly IComparer<PlacementCandidate> _placementComparer;
        private readonly IComparer<BoxItem> _boxItemComparer;

        public PlacementSelector(IComparer<PlacementCandidate> placementComparer, IComparer<BoxItem> boxItemComparer)
        {
            _placementComparer = placementComparer;
            _boxItemComparer = boxItemComparer;
        }

        /// <summary>
        /// Find the best placement for any box item at any available point.
        /// Strategy: Prioritize filling the first level (Z=0) completely before moving up.
        /// </summary>
        /// <param name="source">Available box items.</param>
        /// <param name="calculator">Point calculator with available points.</param>
        /// <param name="container">The target container.</param>
        /// <param name="stack">Current placements in the container.</param>
        /// <param name="remainingWeight">Remaining weight capacity.</param>
        /// <param name="remainingVolume">Remaining volume capacity.</param>
        /// <returns>The best placement, or null if nothing fits.</returns>
        public Placement GetBestPlacement(
            BoxItemSource source,
            IPointCalculator calculator,
            Container container,
            PackStack stack,
            int remainingWeight,
            long remainingVolume)
        {
            // First, try to find placements on the first level (Z=0) to maximize base coverage
            var firstLevelPlacement = GetBestFirstLevelPlacement(
                source, calculator, container, stack, remainingWeight, remainingVolume);
            
            // If we found a valid first-level placement, use it
            if (firstLevelPlacement != null)
                return firstLevelPlacement;
            
            // Otherwise, fall back to normal placement strategy for higher levels
            return GetBestHigherLevelPlacement(
                source, calculator, container, stack, remainingWeight, remainingVolume);
        }

        /// <summary>
        /// Find the best placement specifically for the first level (Z=0).
        /// Prioritizes largest area to maximize base coverage.
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
                
                // Only consider first level points (Z=0)
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
                        
                        // Verify placement doesn't exceed container boundaries
                        if (placement.AbsoluteEndX >= container.LoadDx ||
                            placement.AbsoluteEndY >= container.LoadDy ||
                            placement.AbsoluteEndZ >= container.LoadDz)
                            continue;
                        
                        // Verify placement doesn't intersect with existing placements
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

                            // First level is always on ground, no support check needed
                        }
                        
                        // Prioritize by largest area for first level coverage
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
        /// Find the best placement for higher levels (Z>0).
        /// Uses the standard placement comparer.
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
                        
                        // Verify placement doesn't exceed container boundaries
                        if (placement.AbsoluteEndX >= container.LoadDx ||
                            placement.AbsoluteEndY >= container.LoadDy ||
                            placement.AbsoluteEndZ >= container.LoadDz)
                            continue;
                        
                        // Verify placement doesn't intersect with existing placements
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

                            // Verify placement has sufficient support
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
        /// Find the best placement specifically for the first item of a new level.
        /// Prioritizes by area to get the best base for the level.
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
                        
                        // Create placement to check boundaries
                        var placement = new Placement(sv, point.MinX, point.MinY, point.MinZ, boxItem);
                        
                        // Verify placement doesn't exceed container boundaries
                        if (placement.AbsoluteEndX >= container.LoadDx ||
                            placement.AbsoluteEndY >= container.LoadDy ||
                            placement.AbsoluteEndZ >= container.LoadDz)
                            continue;

                        // Get stack from container for collision and support checks
                        var stack = container.Stack;
                        
                        // Verify placement doesn't intersect with existing placements
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

                            // Verify placement has sufficient support
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
        /// 最严格的方案：要求100%底面被支撑，不允许任何悬空
        /// 1. 底面必须100%被下层物体直接支撑
        /// 2. 底面中心点必须被支撑
        /// 3. 底面四个角点必须全部被支撑
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

                // ★★★ 关键修复：必须紧贴上一层（existing的顶部必须正好在placement底部下方）
                if (existing.AbsoluteEndZ == placementBottomZ - 1)
                {
                    // 计算2D重叠面积（X-Y平面）
                    long overlapArea = CalculateOverlapArea2D(placement, existing);
                    supportedArea += overlapArea;
                }
            }

            // 条件1：支撑面积必须100%（不允许任何悬空）
            if (supportedArea < bottomArea)
                return false;

            // 条件2：底面中心点必须被支撑
            int centerX = placement.X + placement.StackValue.Dx / 2;
            int centerY = placement.Y + placement.StackValue.Dy / 2;

            if (!IsPointSupported(centerX, centerY, placementBottomZ, existingPlacements))
                return false;

            // 条件3：底面四个角点必须全部被支撑
            int dx = placement.StackValue.Dx;
            int dy = placement.StackValue.Dy;
            
            var corners = new (int x, int y)[]
            {
                (placement.X, placement.Y),
                (placement.X + dx - 1, placement.Y),
                (placement.X, placement.Y + dy - 1),
                (placement.X + dx - 1, placement.Y + dy - 1)
            };

            foreach (var corner in corners)
            {
                if (!IsPointSupported(corner.x, corner.y, placementBottomZ, existingPlacements))
                    return false;
            }

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
