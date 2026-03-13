using System;
using System.Collections.Generic;
using System.Linq;
using ThreeDPacking.Core.Models;
using ThreeDPacking.Core.Packers;

namespace ThreeDPacking.Core.IO
{
    /// <summary>
    /// Multi-round packing orchestrator. Translates the core logic of RandomExcelPackingTest.
    /// Tries multiple strategies and container types per round, selects the best, removes packed items, repeats.
    /// </summary>
    public class PackingOrchestrator
    {
        private const int RandomTrialCount = 60;

        private readonly IPackager _plainPackager;
        private readonly IPackager _laffPackager;

        public PackingOrchestrator()
        {
            _plainPackager = new PlainPackager();
            _laffPackager = new LaffPackager();
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
            List<ItemCandidate> selectedItems,
            List<ContainerCandidate> containerCandidates,
            long randomSeed,
            Action<string> log = null)
        {
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
                    
                    var attempt = TryPackInContainer(remainingItems, candidate, randomSeed);
                    
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
                        else if (attempt.PackedCount == chosen.PackedCount && attempt.Utilization > chosen.Utilization)
                        {
                            // 装入数量相同，选利用率更高的（容器体积更小）
                            isBetter = true;
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
                    log?.Invoke("Error: Cannot pack any remaining items into any container.");
                    break;
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

            return allContainers;
        }

        /// <summary>
        /// Try to pack items into a specific container using all strategies.
        /// Returns the best attempt for this container.
        /// </summary>
        private PackingAttemptResult TryPackInContainer(
            List<ItemCandidate> items,
            ContainerCandidate candidate,
            long randomSeed)
        {
            var attempts = new List<PackingAttemptResult>();

            // Plain packager strategies
            attempts.Add(PackWithStrategy(items, candidate, _plainPackager, true, "Plain-MaxArea", randomSeed));
            attempts.Add(PackWithStrategy(items, candidate, _plainPackager, true, "Plain-MaxVolume", randomSeed));
            attempts.Add(PackWithStrategy(items, candidate, _plainPackager, true, "Plain-MaxDim", randomSeed));
            attempts.Add(PackWithStrategy(items, candidate, _plainPackager, false, "Plain-MaxArea_Raw", randomSeed));
            attempts.Add(PackWithStrategy(items, candidate, _plainPackager, false, "Plain-MaxDim_Raw", randomSeed));

            // LAFF packager strategies
            attempts.Add(PackWithStrategy(items, candidate, _laffPackager, true, "Laff-MaxArea", randomSeed));
            attempts.Add(PackWithStrategy(items, candidate, _laffPackager, true, "Laff-MaxVolume", randomSeed));
            attempts.Add(PackWithStrategy(items, candidate, _laffPackager, true, "Laff-MaxDim", randomSeed));
            attempts.Add(PackWithStrategy(items, candidate, _laffPackager, false, "Laff-MaxArea_Raw", randomSeed));
            attempts.Add(PackWithStrategy(items, candidate, _laffPackager, false, "Laff-MaxDim_Raw", randomSeed));

            // Random shuffle strategies
            for (int k = 0; k < RandomTrialCount; k++)
            {
                attempts.Add(PackWithStrategy(items, candidate, _plainPackager, true, "Plain-Shuffle_" + k, randomSeed));
                attempts.Add(PackWithStrategy(items, candidate, _laffPackager, true, "Laff-Shuffle_" + k, randomSeed));
            }

            // Return the best attempt for this container (most items packed)
            PackingAttemptResult best = null;
            foreach (var attempt in attempts)
            {
                if (attempt == null || attempt.PackedCount == 0) continue;
                if (best == null || attempt.PackedCount > best.PackedCount)
                    best = attempt;
            }
            
            return best;
        }

        private PackingAttemptResult PackWithStrategy(
            List<ItemCandidate> items,
            ContainerCandidate candidate,
            IPackager packager,
            bool forceFlat,
            string strategyName,
            long randomSeed)
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

            return PackIntoContainer(sortedItems, candidate, packager, forceFlat, strategyName);
        }

        private PackingAttemptResult PackIntoContainer(
            List<ItemCandidate> items,
            ContainerCandidate candidate,
            IPackager packager,
            bool forceFlat,
            string strategyName)
        {
            var products = BuildProducts(items, forceFlat);
            var container = new Container(
                candidate.Name, candidate.Name,
                candidate.Dx, candidate.Dy, candidate.Dz,
                candidate.EmptyWeight, candidate.MaxLoadWeight);

            Container packedContainer = packager.Pack(products, container);

            if (packedContainer == null || packedContainer.Stack.IsEmpty)
                return null;

            // Match packed items back to ItemCandidates
            var packedItems = new List<ItemCandidate>();
            var matchedIds = new HashSet<string>();
            
            foreach (var p in packedContainer.Stack.Placements)
            {
                string id = p.StackValue.Box?.Id;
                if (id == null) continue;

                // 每个Placement对应一个实际装箱的物品实例
                // 需要在items中找到匹配的ItemCandidate
                foreach (var item in items)
                {
                    string itemId = item.Name + "#" + item.InstanceId;
                    if (itemId == id && !matchedIds.Contains(itemId + "_" + packedItems.Count))
                    {
                        packedItems.Add(item);
                        matchedIds.Add(itemId + "_" + (packedItems.Count - 1));
                        break;
                    }
                }
            }

            long packedVolume = CalcItemsVolume(packedItems);
            double utilization = candidate.Volume > 0 ? (double)packedVolume / candidate.Volume : 0;

            return new PackingAttemptResult(packedContainer, candidate, packedVolume, utilization, packedItems, strategyName);
        }

        private List<BoxItem> BuildProducts(List<ItemCandidate> items, bool forceFlat)
        {
            var products = new List<BoxItem>();
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
                var box = new Box(id, id, dx, dy, dz, 1, false);
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

        private void Shuffle<T>(List<T> list, long seed)
        {
            var rng = new Random((int)(seed & 0x7FFFFFFF));
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                var temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
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
