using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ThreeDPacking.Core.Models;
using ThreeDPacking.Core.Packers;

namespace ThreeDPacking.Core.IO
{
    /// <summary>
    /// 装箱流程编排器，协调整个装箱流程
    /// </summary>
    public class PackingOrchestrator
    {
        private const int RandomTrialCount = 60;

        private readonly IPackager _plainPackager;
        private readonly IPackager _laffPackager;
        private readonly IPackager _hybridPackager;

        public PackingOrchestrator()
        {
            //创建三个Packager实例：
            //_plainPackager（3D极端点算法，处理全空间放置）
            //_laffPackager（LAFF层级算法，先填层再堆高，强调底面覆盖）
            //_hybridPackager（混合算法，高度分组+空隙回填，兼顾稳定性和空间利用率）
            _plainPackager = new PlainPackager();
            _laffPackager = new LaffPackager();
            _hybridPackager = new HybridPackager();
        }

        /// <summary>
        /// Run the full multi-round packing process.
        /// Tries containers from smallest to largest. If all items fit in a container, uses it.
        /// Otherwise moves to next larger container. If max container can't fit all, packs as many as possible.
        /// </summary>
        /// <param name="selectedItems">Items to pack (will be modified as items are packed).</param>
        /// <param name="containerCandidates">Available container types (will be sorted by volume ascending).</param>
        /// <param name="randomSeed">Random seed for shuffle strategies.</param>
        /// <param name="log">Optional action to log messages.</param>
        /// <returns>List of packed containers.</returns>
        public List<Container> Run(
            List<ItemCandidate> selectedItems,// 待装物品列表（会修改）
            List<ContainerCandidate> containerCandidates,// 容器候选列表
            long randomSeed,// 随机种子（用于Shuffle策略的可复现）
            Action<string> log = null,
            PaddingPaperFillStrategy paddingPaperFillStrategy = PaddingPaperFillStrategy.MaxUtilization,
            PackingOptions options = null)
        {
            var effectiveOptions = options ?? PackingOptions.Default;
            var effectivePaddingStrategy = effectiveOptions.PaddingPaperStrategy != PaddingPaperFillStrategy.MaxUtilization
                ? effectiveOptions.PaddingPaperStrategy
                : paddingPaperFillStrategy;
            var effectiveMinPaddingWidth = effectiveOptions.PaddingPaperMinWidth > 0
                ? effectiveOptions.PaddingPaperMinWidth
                : PaddingPaper.DefaultWidth;
            int effectiveContainerSafetyDistance = Math.Max(0, effectiveOptions.ContainerSafetyDistance);
            int effectiveItemSafetyDistance = Math.Max(0, effectiveOptions.ItemSafetyDistance);
            int effectivePaddingPaperSafetyDistance = Math.Max(0, effectiveOptions.PaddingPaperSafetyDistance);
            bool useCustomerDemandFill = effectivePaddingStrategy == PaddingPaperFillStrategy.CustomerDemandFill;

            // Sort containers by volume (smallest first)
            var sortedContainers = containerCandidates.OrderBy(c => c.Volume).ToList();
            
            var remainingItems = new List<ItemCandidate>(selectedItems);
            var packedResults = new List<PackingAttemptResult>();
            int round = 1;

            while (remainingItems.Count > 0)
            {
                log?.Invoke($"\n========== Round {round} ==========");
                log?.Invoke($"Remaining items: {remainingItems.Count}");

                PackingAttemptResult chosen = null;
                
                // 尝试所有容器，策略：
                // 1. 优先选装入物品数量最多的方案
                // 2. 同等数量时，选利用率最高的（即最小且刚好装得下的容器）
                foreach (var candidate in sortedContainers)
                {
                    log?.Invoke($"Trying container: {candidate.Name} (Volume: {candidate.Volume})");
                    
                    var attempt = TryPackInContainer(
                        remainingItems,
                        candidate,
                        randomSeed,
                        useCustomerDemandFill,
                        effectiveMinPaddingWidth,
                        effectiveContainerSafetyDistance,
                        effectiveItemSafetyDistance);
                    
                    if (attempt != null && attempt.PackedCount > 0)
                    {
                        bool isBetter = false;
                        if (chosen == null)
                        {
                            isBetter = true;
                        }
                        else if (attempt.PackedCount > chosen.PackedCount)
                        {
                            // 装入更多物品，优先选这个
                            isBetter = true;
                        }
                        else if (attempt.PackedCount == chosen.PackedCount)
                        {
                            // 装入数量相同：先比较利用率，再做确定性 tie-break（避免微小差异导致不一致）
                            const double eps = 1e-9;
                            if (attempt.Utilization > chosen.Utilization + eps)
                            {
                                isBetter = true;
                            }
                            else if (Math.Abs(attempt.Utilization - chosen.Utilization) <= eps)
                            {
                                // 利用率完全相同：偏好体积更小的容器（更“紧凑”）
                                if (attempt.ContainerCandidate.Volume < chosen.ContainerCandidate.Volume)
                                    isBetter = true;
                                else if (attempt.ContainerCandidate.Volume == chosen.ContainerCandidate.Volume &&
                                         string.CompareOrdinal(attempt.Strategy, chosen.Strategy) < 0)
                                    isBetter = true;
                            }
                        }
                        
                        if (isBetter)
                        {
                            chosen = attempt;
                            log?.Invoke($"  Better: {candidate.Name} packed={attempt.PackedCount} util={attempt.Utilization:F4}");
                        }
                    }
                }

                if (chosen == null || chosen.PackedCount == 0)
                {
                    // 现有容器无法装入剩余物品，尝试使用最大容器
                    var largestContainer = sortedContainers.LastOrDefault();
                    if (largestContainer != null && remainingItems.Count > 0)
                    {
                        log?.Invoke($"No suitable container found. Trying largest container: {largestContainer.Name} for remaining {remainingItems.Count} items");
                        
                        var attempt = TryPackInContainer(
                            remainingItems,
                            largestContainer,
                            randomSeed,
                            useCustomerDemandFill,
                            effectiveMinPaddingWidth,
                            effectiveContainerSafetyDistance,
                            effectiveItemSafetyDistance);
                        if (attempt != null && attempt.PackedCount > 0)
                        {
                            chosen = attempt;
                            log?.Invoke($"  Success: Packed {attempt.PackedCount} items into largest container");
                        }
                        else
                        {
                            // 即使最大容器也无法装入任何物品，强制为每个剩余物品创建单独容器
                            log?.Invoke($"  Warning: Cannot pack items together. Creating individual containers for {remainingItems.Count} remaining items.");
                            bool anyPacked = false;
                            
                            // 按体积从大到小排序，优先装入大物品
                            var sortedRemaining = remainingItems.OrderByDescending(i => i.Volume).ToList();
                            
                            foreach (var item in sortedRemaining.ToList())
                            {
                                var singleItemResult = ForcePackSingleItem(
                                    item,
                                    largestContainer,
                                    useCustomerDemandFill,
                                    effectiveMinPaddingWidth,
                                    effectiveContainerSafetyDistance,
                                    effectiveItemSafetyDistance);
                                if (singleItemResult != null)
                                {
                                    packedResults.Add(singleItemResult);
                                    RemovePackedItems(remainingItems, singleItemResult.PackedItems);
                                    log?.Invoke($"    Packed single item: {item.Name}#{item.InstanceId}");
                                    anyPacked = true;
                                }
                                else
                                {
                                    log?.Invoke($"    ERROR: Cannot pack item {item.Name}#{item.InstanceId} even in largest container!");
                                }
                            }
                            
                            if (anyPacked)
                            {
                                round++;
                                continue;
                            }
                            else
                            {
                                log?.Invoke("Error: Cannot pack any remaining items even individually.");
                                break;
                            }
                        }
                    }
                    else
                    {
                        log?.Invoke("Error: No container available or no remaining items.");
                        break;
                    }
                }

                log?.Invoke($">> Selected: {chosen.ContainerCandidate.Name} | Strategy: {chosen.Strategy} | Packed: {chosen.PackedCount}/{remainingItems.Count} | Utilization: {chosen.Utilization:F4}");

                packedResults.Add(chosen);
                
                // 安全检查：确保确实装箱了物品
                int itemsBeforeRemoval = remainingItems.Count;
                RemovePackedItems(remainingItems, chosen.PackedItems);
                int itemsAfterRemoval = remainingItems.Count;
                
                if (itemsBeforeRemoval == itemsAfterRemoval)
                {
                    log?.Invoke("Warning: No items were removed, breaking to avoid infinite loop.");
                    break;
                }
                
                round++;
            }

            // Collect all packed containers
            var allContainers = new List<Container>();
            foreach (var r in packedResults)
            {
                if (r.PackedContainer != null)
                    allContainers.Add(r.PackedContainer);
            }

            // 对每个容器填充牛皮纸（装箱完成后，使用极值点算法）
            var paddingPaperPacker = PaddingPaperPackerFactory.Create(effectivePaddingStrategy, effectiveMinPaddingWidth);
            foreach (var container in allContainers)
            {
                FillPaddingPaperWithSafetyDistance(
                    container,
                    paddingPaperPacker,
                    effectiveContainerSafetyDistance,
                    effectivePaddingPaperSafetyDistance,
                    log);
            }

            long totalPaddingPaperVolume = allContainers
                .SelectMany(c => c.Stack.Placements)
                .Where(p => p.IsPadding && p.StackValue != null)
                .Sum(p => (long)p.StackValue.Dx * p.StackValue.Dy * p.StackValue.Dz);
            log?.Invoke($"[牛皮纸] 总体积: {totalPaddingPaperVolume}");

            return allContainers;
        }

        /// <summary>
        /// 多策略尝试（并行执行）
        /// 1、固定策略
        /// 2、Raw变体：无预排序，直接用原序
        /// 3、Shuffle：随机打乱物品序
        /// </summary>
        private PackingAttemptResult TryPackInContainer(
            List<ItemCandidate> items,
            ContainerCandidate candidate,
            long randomSeed,
            bool reserveBottomPadding,
            int minPaddingWidth,
            int containerSafetyDistance,
            int itemSafetyDistance)
        {
            var attempts = new ConcurrentBag<PackingAttemptResult>();

            // 定义所有策略
            var strategies = new List<(IPackager packager, string strategyName)>();

            // Plain packager strategies
            strategies.Add((_plainPackager, "Plain-MaxArea"));
            strategies.Add((_plainPackager, "Plain-MaxVolume"));
            strategies.Add((_plainPackager, "Plain-MaxDim"));
            strategies.Add((_plainPackager, "Plain-MaxArea_Raw"));
            strategies.Add((_plainPackager, "Plain-MaxDim_Raw"));

            // LAFF packager strategies
            strategies.Add((_laffPackager, "Laff-MaxArea"));
            strategies.Add((_laffPackager, "Laff-MaxVolume"));
            strategies.Add((_laffPackager, "Laff-MaxDim"));
            strategies.Add((_laffPackager, "Laff-MaxArea_Raw"));
            strategies.Add((_laffPackager, "Laff-MaxDim_Raw"));

            // Hybrid packager strategies (高度分组+空隙回填)
            strategies.Add((_hybridPackager, "Hybrid-MaxArea"));
            strategies.Add((_hybridPackager, "Hybrid-MaxVolume"));
            strategies.Add((_hybridPackager, "Hybrid-MaxDim"));
            strategies.Add((_hybridPackager, "Hybrid-MaxArea_Raw"));
            strategies.Add((_hybridPackager, "Hybrid-MaxDim_Raw"));

            // Random shuffle strategies
            for (int k = 0; k < RandomTrialCount; k++)
            {
                strategies.Add((_plainPackager, "Plain-Shuffle_" + k));
                strategies.Add((_laffPackager, "Laff-Shuffle_" + k));
                strategies.Add((_hybridPackager, "Hybrid-Shuffle_" + k));
            }

            // 并行执行所有策略
            Parallel.ForEach(strategies, strategy =>
            {
                var result = PackWithStrategy(
                    items,
                    candidate,
                    strategy.packager,
                    strategy.strategyName,
                    randomSeed,
                    reserveBottomPadding,
                    minPaddingWidth,
                    containerSafetyDistance,
                    itemSafetyDistance);
                if (result != null && result.PackedCount > 0)
                {
                    attempts.Add(result);
                }
            });

            // Return the best attempt for this container (most items packed)
            // 注意：attempts 来自并行 ConcurrentBag，遍历顺序不稳定。
            // 必须做确定性 tie-break，保证同一 randomSeed 时结果可复现。
            if (reserveBottomPadding)
            {
                // 客户需求模式下，优先：
                // 1) 装入数量更多
                // 2) 最高装载高度更低（倾向同层并排，减少不必要上叠）
                // 3) 利用率更高
                // 4) 在同等条件下偏向 LAFF（再 Hybrid、再 Plain）
                return attempts
                    .OrderByDescending(a => a.PackedCount)
                    .ThenBy(a => GetNonPaddingMaxUsedZ(a.PackedContainer))
                    .ThenByDescending(a => a.Utilization)
                    .ThenBy(a => GetCustomerDemandStrategyRank(a.Strategy))
                    .ThenBy(a => a.Strategy, StringComparer.Ordinal)
                    .FirstOrDefault();
            }

            return attempts
                .OrderByDescending(a => a.PackedCount)
                .ThenByDescending(a => a.Utilization)
                .ThenBy(a => a.Strategy, StringComparer.Ordinal)
                .FirstOrDefault();
        }

        private PackingAttemptResult PackWithStrategy(
            List<ItemCandidate> items,
            ContainerCandidate candidate,
            IPackager packager,
            string strategyName,
            long randomSeed,
            bool reserveBottomPadding,
            int minPaddingWidth,
            int containerSafetyDistance,
            int itemSafetyDistance)
        {
            var sortedItems = new List<ItemCandidate>(items);

            if (strategyName.Contains("MaxArea"))
                sortedItems.Sort((a, b) => CalcMaxArea(b).CompareTo(CalcMaxArea(a)));
            else if (strategyName.Contains("MaxVolume"))
                sortedItems.Sort((a, b) => b.Volume.CompareTo(a.Volume));
            else if (strategyName.Contains("MaxDim"))
                sortedItems.Sort((a, b) => GetMaxDim(b).CompareTo(GetMaxDim(a)));
            else if (strategyName.Contains("Shuffle"))
                Shuffle(sortedItems, randomSeed + strategyName.GetHashCode() + candidate.Volume);

            return PackIntoContainer(
                sortedItems,
                candidate,
                packager,
                strategyName,
                reserveBottomPadding,
                minPaddingWidth,
                containerSafetyDistance,
                itemSafetyDistance);
        }

        private PackingAttemptResult PackIntoContainer(
            List<ItemCandidate> items,
            ContainerCandidate candidate,
            IPackager packager,
            string strategyName,
            bool useCustomerDemandFill,
            int minPaddingWidth,
            int containerSafetyDistance,
            int itemSafetyDistance)
        {
            int safeContainerMargin = Math.Max(0, containerSafetyDistance);
            int safeItemGap = Math.Max(0, itemSafetyDistance);
            int effectiveLoadDx = candidate.Dx - safeContainerMargin * 2 + safeItemGap;
            int effectiveLoadDy = candidate.Dy - safeContainerMargin * 2 + safeItemGap;

            if (effectiveLoadDx <= 0 || effectiveLoadDy <= 0)
                return null;

            var products = BuildProducts(items, safeItemGap);
            var container = new Container(
                candidate.Name, candidate.Name,
                candidate.Dx, candidate.Dy, candidate.Dz,
                candidate.EmptyWeight, candidate.MaxLoadWeight,
                effectiveLoadDx, effectiveLoadDy, candidate.Dz);

            Container packedContainer = packager.Pack(products, container);

            if (packedContainer == null || packedContainer.Stack.IsEmpty)
                return null;

            packedContainer = ConvertToPhysicalContainer(
                packedContainer,
                candidate,
                safeContainerMargin,
                safeItemGap);

            if (useCustomerDemandFill)
            {
                int packedWithoutBottomPaper = CountNonPaddingPlacements(packedContainer);
                bool allItemsFitWithoutBottom = packedWithoutBottomPaper >= items.Count;
                bool hasPackedBottomCandidate = NeedBottomPaddingForCustomerDemand(packedContainer);
                if (allItemsFitWithoutBottom)
                {
                    // 单箱可装完：仅当当前箱内已选物体满足铺底条件时，才尝试铺底。
                    // 否则不铺底，只在后续步骤铺顶部/中间层。
                    if (hasPackedBottomCandidate)
                    {
                        // 空间不足导致铺底失败时，保留不铺底结果（不抛弃可行方案）。
                        TryApplyBottomPaddingAndLiftItems(
                            packedContainer,
                            minPaddingWidth,
                            safeContainerMargin,
                            forceApplyBottom: true);
                    }
                }
                else
                {
                    // 多箱场景：先保留“不铺底”的装载结果；
                    // 仅当未装下的物品满足“最长边>300 且最短边<100”时，再尝试铺底。
                    if (NeedBottomPaddingForOverflowItems(items, packedContainer))
                    {
                        TryApplyBottomPaddingAndLiftItems(
                            packedContainer,
                            minPaddingWidth,
                            safeContainerMargin,
                            forceApplyBottom: true);
                    }
                }
            }

            // Match packed items back to ItemCandidates
            var packedItems = new List<ItemCandidate>();
            var matchedInstanceIds = new HashSet<int>();
            int unmatchedPlacements = 0;
            
            foreach (var p in packedContainer.Stack.Placements)
            {
                if (p == null || p.IsPadding)
                    continue;

                string id = p.StackValue.Box?.Id;
                if (id == null) 
                {
                    unmatchedPlacements++;
                    continue;
                }

                // 每个Placement对应一个实际装箱的物品实例
                // 需要在items中找到匹配的ItemCandidate
                bool matched = false;
                foreach (var item in items)
                {
                    string itemId = item.Name + "#" + item.InstanceId;
                    if (itemId == id && !matchedInstanceIds.Contains(item.InstanceId))
                    {
                        packedItems.Add(item);
                        matchedInstanceIds.Add(item.InstanceId);
                        matched = true;
                        break;
                    }
                }
                
                if (!matched)
                {
                    unmatchedPlacements++;
                }
            }

            // 验证：确保所有Placement都被正确匹配
            if (unmatchedPlacements > 0)
            {
                Console.WriteLine($"Warning: {unmatchedPlacements} placements could not be matched to items in strategy {strategyName}");
            }

            long packedVolume = CalcItemsVolume(packedItems);
            double utilization = candidate.Volume > 0 ? (double)packedVolume / candidate.Volume : 0;

            return new PackingAttemptResult(packedContainer, candidate, packedVolume, utilization, packedItems, strategyName);
        }

        private List<BoxItem> BuildProducts(List<ItemCandidate> items, int itemSafetyDistance)
        {
            var products = new List<BoxItem>();
            int safeItemGap = Math.Max(0, itemSafetyDistance);
            foreach (var item in items)
            {
                int dx = item.Dx, dy = item.Dy, dz = item.Dz;

                // 总是计算三个面的面积，选择底面积最大的摆放方式
                // 三个面的面积: dx*dy, dx*dz, dy*dz
                long areaXY = 1L * item.Dx * item.Dy;
                long areaXZ = 1L * item.Dx * item.Dz;
                long areaYZ = 1L * item.Dy * item.Dz;

                if (areaXY >= areaXZ && areaXY >= areaYZ)
                {
                    // XY面作为底面，Z作为高
                    dx = item.Dx;
                    dy = item.Dy;
                    dz = item.Dz;
                }
                else if (areaXZ >= areaXY && areaXZ >= areaYZ)
                {
                    // XZ面作为底面，Y作为高
                    dx = item.Dx;
                    dy = item.Dz;
                    dz = item.Dy;
                }
                else
                {
                    // YZ面作为底面，X作为高
                    dx = item.Dy;
                    dy = item.Dz;
                    dz = item.Dx;
                }

                string id = item.Name + "#" + item.InstanceId;
                // rotate3D=false 确保只使用底面旋转，保持底面积最大的面朝下
                var box = new Box(id, id, dx + safeItemGap, dy + safeItemGap, dz, 1, false);
                products.Add(new BoxItem(box, 1));
            }
            return products;
        }



        private long CalcMaxArea(ItemCandidate item)
        {
            return Math.Max(1L * item.Dx * item.Dy,
                Math.Max(1L * item.Dx * item.Dz, 1L * item.Dy * item.Dz));
        }

        private int GetMaxDim(ItemCandidate item)
        {
            return Math.Max(item.Dx, Math.Max(item.Dy, item.Dz));
        }

        private long CalcItemsVolume(List<ItemCandidate> items)
        {
            long sum = 0;
            foreach (var item in items) sum += item.Volume;
            return sum;
        }

        private void RemovePackedItems(List<ItemCandidate> remaining, List<ItemCandidate> packed)
        {
            foreach (var p in packed)
            {
                for (int i = 0; i < remaining.Count; i++)
                {
                    if (remaining[i].InstanceId == p.InstanceId)
                    {
                        remaining.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 线程安全的随机打乱方法
        /// </summary>
        private void Shuffle<T>(List<T> list, long seed)
        {
            // 使用线程本地存储确保每个线程有自己的Random实例
            var rng = new Random((int)(seed & 0x7FFFFFFF));
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                var temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        /// <summary>
        /// 强制将单个物品装入容器（创建单独容器）
        /// </summary>
        private PackingAttemptResult ForcePackSingleItem(
            ItemCandidate item,
            ContainerCandidate candidate,
            bool useCustomerDemandFill,
            int minPaddingWidth,
            int containerSafetyDistance,
            int itemSafetyDistance)
        {
            int safeContainerMargin = Math.Max(0, containerSafetyDistance);
            int safeItemGap = Math.Max(0, itemSafetyDistance);
            int effectiveLoadDx = candidate.Dx - safeContainerMargin * 2 + safeItemGap;
            int effectiveLoadDy = candidate.Dy - safeContainerMargin * 2 + safeItemGap;
            if (effectiveLoadDx <= 0 || effectiveLoadDy <= 0)
                return null;

            // 检查物品是否能放入容器（尝试所有旋转方式）
            bool canFit = false;
            int bestDx = item.Dx, bestDy = item.Dy, bestDz = item.Dz;
            
            // 尝试6种旋转方式
            var rotations = new (int dx, int dy, int dz)[]
            {
                (item.Dx, item.Dy, item.Dz),
                (item.Dx, item.Dz, item.Dy),
                (item.Dy, item.Dx, item.Dz),
                (item.Dy, item.Dz, item.Dx),
                (item.Dz, item.Dx, item.Dy),
                (item.Dz, item.Dy, item.Dx)
            };
            
            foreach (var rot in rotations)
            {
                if (rot.dx + safeItemGap <= effectiveLoadDx &&
                    rot.dy + safeItemGap <= effectiveLoadDy &&
                    rot.dz <= candidate.Dz)
                {
                    canFit = true;
                    bestDx = rot.dx;
                    bestDy = rot.dy;
                    bestDz = rot.dz;
                    break;
                }
            }
            
            if (!canFit)
                return null;
            
            // 使用能放入的旋转尺寸创建Box和Placement
            string id = item.Name + "#" + item.InstanceId;
            var box = new Box(id, id, bestDx, bestDy, bestDz, 1, false);
            var stackValue = new BoxStackValue(bestDx, bestDy, bestDz, 0);
            stackValue.Box = box;
            var boxItem = new BoxItem(box, 1);
            var placement = new Placement(stackValue, safeContainerMargin, safeContainerMargin, 0, boxItem);
            
            var container = new Container(
                candidate.Name, candidate.Name,
                candidate.Dx, candidate.Dy, candidate.Dz,
                candidate.EmptyWeight, candidate.MaxLoadWeight);
            container.Stack.Add(placement);

            if (useCustomerDemandFill)
            {
                // 单件单箱：按客户需求优先尝试铺底，失败则保留不铺底结果。
                TryApplyBottomPaddingAndLiftItems(
                    container,
                    minPaddingWidth,
                    safeContainerMargin,
                    forceApplyBottom: true);
            }
            
            var packedItems = new List<ItemCandidate> { item };
            long packedVolume = item.Volume;
            double utilization = candidate.Volume > 0 ? (double)packedVolume / candidate.Volume : 0;
            
            return new PackingAttemptResult(container, candidate, packedVolume, utilization, packedItems, "ForcePackSingle");
        }

        private Container ConvertToPhysicalContainer(
            Container packedContainer,
            ContainerCandidate candidate,
            int containerSafetyDistance,
            int itemSafetyDistance)
        {
            var converted = new Container(
                candidate.Name,
                candidate.Name,
                candidate.Dx,
                candidate.Dy,
                candidate.Dz,
                candidate.EmptyWeight,
                candidate.MaxLoadWeight);

            foreach (var placement in packedContainer.Stack.Placements)
            {
                if (placement == null || placement.StackValue == null)
                    continue;

                int shiftedX = placement.X + containerSafetyDistance;
                int shiftedY = placement.Y + containerSafetyDistance;
                int z = placement.Z;

                if (placement.IsPadding)
                {
                    var paddingCopy = new Placement(placement.StackValue, shiftedX, shiftedY, z, placement.BoxItem)
                    {
                        IsPadding = true
                    };
                    converted.Stack.Add(paddingCopy);
                    continue;
                }

                int realDx = Math.Max(1, placement.StackValue.Dx - itemSafetyDistance);
                int realDy = Math.Max(1, placement.StackValue.Dy - itemSafetyDistance);
                int realDz = placement.StackValue.Dz;

                var originalBox = placement.StackValue.Box;
                string id = originalBox?.Id ?? string.Empty;
                string description = originalBox?.Description ?? id;
                int weight = originalBox?.Weight ?? 1;

                var realBox = new Box(id, description, realDx, realDy, realDz, weight, false);
                var realStackValue = new BoxStackValue(realDx, realDy, realDz, 0);
                realStackValue.Box = realBox;
                var realBoxItem = new BoxItem(realBox, 1);

                var realPlacement = new Placement(realStackValue, shiftedX, shiftedY, z, realBoxItem);
                converted.Stack.Add(realPlacement);
            }

            return converted;
        }

        private void FillPaddingPaperWithSafetyDistance(
            Container container,
            IPaddingPaperPacker paddingPaperPacker,
            int containerSafetyDistance,
            int paddingPaperSafetyDistance,
            Action<string> log)
        {
            if (container?.Stack == null || paddingPaperPacker == null || container.Stack.IsEmpty)
                return;

            int margin = Math.Max(0, containerSafetyDistance);
            int gap = Math.Max(0, paddingPaperSafetyDistance);

            // 无安全距离时，保持原逻辑，避免不必要转换。
            if (margin == 0 && gap == 0)
            {
                paddingPaperPacker.FillWithPaddingPaper(container, log);
                return;
            }

            int effectiveLoadDx = container.LoadDx - margin * 2 + gap;
            int effectiveLoadDy = container.LoadDy - margin * 2 + gap;
            if (effectiveLoadDx <= 0 || effectiveLoadDy <= 0)
            {
                log?.Invoke($"[牛皮纸] 跳过：安全距离过大（container={margin}, padding={gap}）。");
                return;
            }

            var planningContainer = new Container(
                container.Id,
                container.Description,
                container.Dx,
                container.Dy,
                container.Dz,
                container.EmptyWeight,
                container.MaxLoadWeight,
                effectiveLoadDx,
                effectiveLoadDy,
                container.LoadDz);

            foreach (var placement in container.Stack.Placements)
            {
                var mapped = MapPlacementForPaddingPlanning(placement, margin, gap);
                if (mapped != null)
                    planningContainer.Stack.Add(mapped);
            }

            paddingPaperPacker.FillWithPaddingPaper(planningContainer, log);

            container.Stack.Clear();
            foreach (var placement in planningContainer.Stack.Placements)
            {
                var restored = RestorePlacementFromPaddingPlanning(placement, margin, gap);
                if (restored != null)
                    container.Stack.Add(restored);
            }
        }

        private static Placement MapPlacementForPaddingPlanning(Placement placement, int containerSafetyDistance, int paddingPaperSafetyDistance)
        {
            if (placement == null || placement.StackValue == null)
                return null;

            int x = placement.X - containerSafetyDistance;
            int y = placement.Y - containerSafetyDistance;
            int z = placement.Z;
            if (x < 0 || y < 0)
                return null;

            int dx = placement.StackValue.Dx + paddingPaperSafetyDistance;
            int dy = placement.StackValue.Dy + paddingPaperSafetyDistance;
            int dz = placement.StackValue.Dz;

            if (placement.IsPadding)
            {
                var planningPaper = new PaddingPaper(x, y, z, dx, dy, dz).ToPlacement();
                planningPaper.IsPadding = true;
                return planningPaper;
            }

            var sourceBox = placement.StackValue.Box;
            string id = sourceBox?.Id ?? string.Empty;
            string description = sourceBox?.Description ?? id;
            int weight = sourceBox?.Weight ?? 1;

            var planningBox = new Box(id, description, dx, dy, dz, weight, false);
            var planningValue = new BoxStackValue(dx, dy, dz, 0);
            planningValue.Box = planningBox;
            var planningItem = new BoxItem(planningBox, 1);
            return new Placement(planningValue, x, y, z, planningItem);
        }

        private static Placement RestorePlacementFromPaddingPlanning(Placement placement, int containerSafetyDistance, int paddingPaperSafetyDistance)
        {
            if (placement == null || placement.StackValue == null)
                return null;

            int x = placement.X + containerSafetyDistance;
            int y = placement.Y + containerSafetyDistance;
            int z = placement.Z;
            int dx = Math.Max(1, placement.StackValue.Dx - paddingPaperSafetyDistance);
            int dy = Math.Max(1, placement.StackValue.Dy - paddingPaperSafetyDistance);
            int dz = placement.StackValue.Dz;

            if (placement.IsPadding)
            {
                var restoredPaper = new PaddingPaper(x, y, z, dx, dy, dz).ToPlacement();
                restoredPaper.IsPadding = true;
                return restoredPaper;
            }

            var sourceBox = placement.StackValue.Box;
            string id = sourceBox?.Id ?? string.Empty;
            string description = sourceBox?.Description ?? id;
            int weight = sourceBox?.Weight ?? 1;

            var restoredBox = new Box(id, description, dx, dy, dz, weight, false);
            var restoredValue = new BoxStackValue(dx, dy, dz, 0);
            restoredValue.Box = restoredBox;
            var restoredItem = new BoxItem(restoredBox, 1);
            return new Placement(restoredValue, x, y, z, restoredItem);
        }

        private bool TryApplyBottomPaddingAndLiftItems(
            Container container,
            int minPaddingWidth,
            int containerSafetyDistance,
            bool forceApplyBottom = false)
        {
            if (container?.Stack == null)
                return false;

            if (!forceApplyBottom && !NeedBottomPaddingForCustomerDemand(container))
                return true;

            int safeMargin = Math.Max(0, containerSafetyDistance);
            int bottomLoadDx = container.LoadDx - safeMargin * 2;
            int bottomLoadDy = container.LoadDy - safeMargin * 2;
            if (bottomLoadDx <= 0 || bottomLoadDy <= 0)
                return false;

            // 铺底安全距离仅作用于 XY：在底面可用区域内铺纸，不改变 Z 方向高度规则。
            var basePapers = BuildBottomPaddingPapers(bottomLoadDx, bottomLoadDy, minPaddingWidth);
            if (basePapers.Count == 0)
                return false;

            // 铺底前预校验：首层物体抬高后，必须仍满足至少 50% 的底面支撑。
            // 若任一首层物体无法满足，则放弃本箱铺底，避免生成不稳定方案。
            if (!CanBottomLayerSupportFirstLevelItems(container, basePapers, safeMargin))
                return false;

            int liftHeight = PaddingPaper.DefaultHeight;

            // 先验证再执行，避免出现“部分物品已抬高后失败”的中间状态。
            foreach (var placement in container.Stack.Placements)
            {
                if (placement == null || placement.IsPadding)
                    continue;

                if (placement.AbsoluteEndZ + liftHeight >= container.LoadDz)
                    return false;
            }

            foreach (var placement in container.Stack.Placements)
            {
                if (placement == null || placement.IsPadding)
                    continue;

                placement.Z += liftHeight;
            }

            foreach (var paper in basePapers)
            {
                var shiftedPaper = new PaddingPaper(
                    paper.X + safeMargin,
                    paper.Y + safeMargin,
                    paper.Z,
                    paper.Dx,
                    paper.Dy,
                    paper.Dz);
                container.Stack.Add(shiftedPaper.ToPlacement());
            }

            return true;
        }

        private static bool CanBottomLayerSupportFirstLevelItems(Container container, List<PaddingPaper> basePapers, int safeMargin)
        {
            if (container?.Stack?.Placements == null || basePapers == null || basePapers.Count == 0)
                return false;

            var firstLevelItems = container.Stack.Placements
                .Where(p => p != null && !p.IsPadding && p.StackValue != null && p.Z == 0)
                .ToList();
            if (firstLevelItems.Count == 0)
                return true;

            foreach (var placement in firstLevelItems)
            {
                long itemArea = 1L * placement.StackValue.Dx * placement.StackValue.Dy;
                long supportedArea = 0;

                int itemX1 = placement.X;
                int itemX2 = placement.AbsoluteEndX;
                int itemY1 = placement.Y;
                int itemY2 = placement.AbsoluteEndY;

                foreach (var paper in basePapers)
                {
                    int paperX1 = paper.X + safeMargin;
                    int paperX2 = paperX1 + paper.Dx - 1;
                    int paperY1 = paper.Y + safeMargin;
                    int paperY2 = paperY1 + paper.Dy - 1;

                    int overlapX1 = Math.Max(itemX1, paperX1);
                    int overlapX2 = Math.Min(itemX2, paperX2);
                    int overlapY1 = Math.Max(itemY1, paperY1);
                    int overlapY2 = Math.Min(itemY2, paperY2);

                    int overlapDx = overlapX2 - overlapX1 + 1;
                    int overlapDy = overlapY2 - overlapY1 + 1;
                    if (overlapDx > 0 && overlapDy > 0)
                        supportedArea += (long)overlapDx * overlapDy;
                }

                if (supportedArea * 100 < itemArea * 50)
                    return false;
            }

            return true;
        }

        private bool NeedBottomPaddingForCustomerDemand(Container container)
        {
            var nonPaddingPlacements = container.Stack.Placements
                .Where(p => p != null && !p.IsPadding && p.StackValue != null)
                .ToList();
            if (nonPaddingPlacements.Count == 0)
                return false;

            int minZ = nonPaddingPlacements.Min(p => p.Z);
            foreach (var placement in nonPaddingPlacements.Where(p => p.Z == minZ))
            {
                int dx = placement.StackValue.Dx;
                int dy = placement.StackValue.Dy;
                int dz = placement.StackValue.Dz;
                int longest = Math.Max(dx, Math.Max(dy, dz));
                int shortest = Math.Min(dx, Math.Min(dy, dz));
                if (longest > 300 && shortest < 100)
                    return true;
            }

            return false;
        }

        private bool NeedBottomPaddingForOverflowItems(List<ItemCandidate> requestedItems, Container packedContainer)
        {
            if (requestedItems == null || requestedItems.Count == 0 || packedContainer?.Stack == null)
                return false;

            var packedIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var placement in packedContainer.Stack.Placements)
            {
                if (placement == null || placement.IsPadding)
                    continue;

                string id = placement.StackValue?.Box?.Id;
                if (!string.IsNullOrEmpty(id))
                    packedIds.Add(id);
            }

            foreach (var item in requestedItems)
            {
                string id = item.Name + "#" + item.InstanceId;
                if (packedIds.Contains(id))
                    continue;

                int longest = Math.Max(item.Dx, Math.Max(item.Dy, item.Dz));
                int shortest = Math.Min(item.Dx, Math.Min(item.Dy, item.Dz));
                if (longest > 300 && shortest < 100)
                    return true;
            }

            return false;
        }

        private List<PaddingPaper> BuildBottomPaddingPapers(int loadDx, int loadDy, int minPaddingWidth)
        {
            var result = new List<PaddingPaper>();
            int width = Math.Max(PaddingPaper.MinSize, minPaddingWidth);
            int height = PaddingPaper.DefaultHeight;

            bool canAlongX = loadDx >= PaddingPaper.MinLength && loadDy >= width;
            bool canAlongY = loadDy >= PaddingPaper.MinLength && loadDx >= width;

            if (!canAlongX && !canAlongY)
                return result;

            int stripsAlongX = canAlongX ? loadDy / width : 0;
            int stripsAlongY = canAlongY ? loadDx / width : 0;
            long areaAlongX = stripsAlongX > 0 ? (long)stripsAlongX * width * loadDx : 0;
            long areaAlongY = stripsAlongY > 0 ? (long)stripsAlongY * width * loadDy : 0;

            if (areaAlongX >= areaAlongY)
            {
                for (int i = 0; i < stripsAlongX; i++)
                {
                    int y = i * width;
                    result.Add(new PaddingPaper(0, y, 0, loadDx, width, height));
                }
            }
            else
            {
                for (int i = 0; i < stripsAlongY; i++)
                {
                    int x = i * width;
                    result.Add(new PaddingPaper(x, 0, 0, width, loadDy, height));
                }
            }

            return result;
        }

        private static int CountNonPaddingPlacements(Container container)
        {
            if (container?.Stack == null)
                return 0;

            int count = 0;
            foreach (var p in container.Stack.Placements)
            {
                if (p != null && !p.IsPadding && p.StackValue != null)
                    count++;
            }

            return count;
        }

        private static int GetNonPaddingMaxUsedZ(Container container)
        {
            if (container?.Stack == null)
                return int.MaxValue;

            int maxUsedZ = -1;
            foreach (var p in container.Stack.Placements)
            {
                if (p == null || p.IsPadding || p.StackValue == null)
                    continue;
                if (p.AbsoluteEndZ > maxUsedZ)
                    maxUsedZ = p.AbsoluteEndZ;
            }

            return maxUsedZ >= 0 ? maxUsedZ : int.MaxValue;
        }

        private static int GetCustomerDemandStrategyRank(string strategy)
        {
            if (string.IsNullOrEmpty(strategy))
                return 3;
            if (strategy.StartsWith("Laff-", StringComparison.Ordinal))
                return 0;
            if (strategy.StartsWith("Hybrid-", StringComparison.Ordinal))
                return 1;
            if (strategy.StartsWith("Plain-", StringComparison.Ordinal))
                return 2;
            return 3;
        }
    }

    public class PackingAttemptResult
    {
        public Container PackedContainer { get; }
        public ContainerCandidate ContainerCandidate { get; }
        public long PackedVolume { get; }
        public double Utilization { get; }
        public List<ItemCandidate> PackedItems { get; }
        public int PackedCount { get; }
        public string Strategy { get; set; }

        public PackingAttemptResult(Container packedContainer, ContainerCandidate containerCandidate,
            long packedVolume, double utilization, List<ItemCandidate> packedItems, string strategy)
        {
            PackedContainer = packedContainer;
            ContainerCandidate = containerCandidate;
            PackedVolume = packedVolume;
            Utilization = utilization;
            PackedItems = packedItems;
            PackedCount = packedItems.Count;
            Strategy = strategy;
        }
    }
}
