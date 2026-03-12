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
        private const double VolumeGateRatio = 0.82;

        private readonly IPackager _plainPackager;
        private readonly IPackager _laffPackager;

        public PackingOrchestrator()
        {
            _plainPackager = new PlainPackager();
            _laffPackager = new LaffPackager();
        }

        /// <summary>
        /// Run the full multi-round packing process.
        /// </summary>
        /// <param name="selectedItems">Items to pack (will be modified as items are packed).</param>
        /// <param name="containerCandidates">Available container types.</param>
        /// <param name="randomSeed">Random seed for shuffle strategies.</param>
        /// <param name="log">Optional action to log messages.</param>
        /// <returns>List of packed containers.</returns>
        public List<Container> Run(
            List<ItemCandidate> selectedItems,
            List<ContainerCandidate> containerCandidates,
            long randomSeed,
            Action<string> log = null)
        {
            var remainingItems = new List<ItemCandidate>(selectedItems);
            var packedResults = new List<PackingAttemptResult>();
            int round = 1;

            while (remainingItems.Count > 0)
            {
                log?.Invoke($"\n========== Round {round} ==========");
                log?.Invoke($"Remaining items: {remainingItems.Count}");

                var attempts = new List<PackingAttemptResult>();

                foreach (var candidate in containerCandidates)
                {
                    // Plain packager strategies
                    attempts.Add(PackWithStrategy(remainingItems, candidate, _plainPackager, true, "Plain-MaxArea", randomSeed));
                    attempts.Add(PackWithStrategy(remainingItems, candidate, _plainPackager, true, "Plain-MaxVolume", randomSeed));
                    attempts.Add(PackWithStrategy(remainingItems, candidate, _plainPackager, true, "Plain-MaxDim", randomSeed));
                    attempts.Add(PackWithStrategy(remainingItems, candidate, _plainPackager, false, "Plain-MaxArea_Raw", randomSeed));
                    attempts.Add(PackWithStrategy(remainingItems, candidate, _plainPackager, false, "Plain-MaxDim_Raw", randomSeed));

                    // LAFF packager strategies
                    attempts.Add(PackWithStrategy(remainingItems, candidate, _laffPackager, true, "Laff-MaxArea", randomSeed));
                    attempts.Add(PackWithStrategy(remainingItems, candidate, _laffPackager, true, "Laff-MaxVolume", randomSeed));
                    attempts.Add(PackWithStrategy(remainingItems, candidate, _laffPackager, true, "Laff-MaxDim", randomSeed));
                    attempts.Add(PackWithStrategy(remainingItems, candidate, _laffPackager, false, "Laff-MaxArea_Raw", randomSeed));
                    attempts.Add(PackWithStrategy(remainingItems, candidate, _laffPackager, false, "Laff-MaxDim_Raw", randomSeed));

                    // Random shuffle strategies
                    for (int k = 0; k < RandomTrialCount; k++)
                    {
                        attempts.Add(PackWithStrategy(remainingItems, candidate, _plainPackager, true, "Plain-Shuffle_" + k, randomSeed));
                        attempts.Add(PackWithStrategy(remainingItems, candidate, _laffPackager, true, "Laff-Shuffle_" + k, randomSeed));
                    }
                }

                long remainingVolume = CalcItemsVolume(remainingItems);
                var chosen = SelectBestAttempt(attempts, remainingItems.Count, remainingVolume);

                if (chosen == null)
                {
                    log?.Invoke("Error: Cannot pack remaining items into any container.");
                    break;
                }

                log?.Invoke($">> Container: {chosen.ContainerCandidate.Name} | Strategy: {chosen.Strategy} | Packed: {chosen.PackedCount} | Utilization: {chosen.Utilization:F4}");

                packedResults.Add(chosen);
                RemovePackedItems(remainingItems, chosen.PackedItems);
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
            foreach (var p in packedContainer.Stack.Placements)
            {
                string id = p.StackValue.Box?.Id;
                if (id == null) continue;

                foreach (var item in items)
                {
                    if ((item.Name + "#" + item.InstanceId) == id)
                    {
                        packedItems.Add(item);
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

                if (forceFlat)
                {
                    var dims = new[] { item.Dx, item.Dy, item.Dz };
                    Array.Sort(dims);
                    dx = dims[2];
                    dy = dims[1];
                    dz = dims[0];
                }

                string id = item.Name + "#" + item.InstanceId;
                var box = new Box(id, id, dx, dy, dz, 1, true);
                products.Add(new BoxItem(box, 1));
            }
            return products;
        }

        private PackingAttemptResult SelectBestAttempt(List<PackingAttemptResult> attempts, int remainingCount, long remainingVolume)
        {
            int bestCount = 0;
            long bestVolume = 0;

            foreach (var a in attempts)
            {
                if (a == null || a.PackedCount == 0) continue;
                if (a.PackedCount > bestCount) bestCount = a.PackedCount;
                if (a.PackedVolume > bestVolume) bestVolume = a.PackedVolume;
            }

            if (bestCount == 0) return null;

            int dynamicMinCount = GetDynamicMinPackedCount(remainingCount);
            int countThreshold = Math.Min(bestCount, dynamicMinCount);

            long volumeTarget = GetDynamicVolumeTarget(remainingCount, remainingVolume);
            long volumeThreshold = Math.Min(bestVolume,
                Math.Max((long)Math.Ceiling(bestVolume * VolumeGateRatio), volumeTarget));

            // First pass: both thresholds
            PackingAttemptResult chosen = null;
            foreach (var a in attempts)
            {
                if (a == null || a.PackedCount < countThreshold || a.PackedVolume < volumeThreshold) continue;
                if (IsBetterAttempt(a, chosen, remainingCount))
                    chosen = a;
            }

            if (chosen != null) return chosen;

            // Second pass: count threshold only
            foreach (var a in attempts)
            {
                if (a == null || a.PackedCount < countThreshold) continue;
                if (IsBetterAttempt(a, chosen, remainingCount))
                    chosen = a;
            }

            return chosen;
        }

        private bool IsBetterAttempt(PackingAttemptResult candidate, PackingAttemptResult current, int remainingCount)
        {
            if (current == null) return true;

            int candidateBoxes = EstimateTotalBoxCount(remainingCount, candidate.PackedCount);
            int currentBoxes = EstimateTotalBoxCount(remainingCount, current.PackedCount);
            if (candidateBoxes != currentBoxes)
                return candidateBoxes < currentBoxes;

            if (candidate.PackedCount != current.PackedCount)
                return candidate.PackedCount > current.PackedCount;

            if (candidate.PackedVolume != current.PackedVolume)
                return candidate.PackedVolume > current.PackedVolume;

            int utilCmp = candidate.Utilization.CompareTo(current.Utilization);
            if (utilCmp != 0)
                return utilCmp > 0;

            return candidate.ContainerCandidate.Volume < current.ContainerCandidate.Volume;
        }

        private int EstimateTotalBoxCount(int remainingCount, int packedCount)
        {
            if (packedCount <= 0) return int.MaxValue;
            return (remainingCount + packedCount - 1) / packedCount;
        }

        private int GetDynamicMinPackedCount(int remaining)
        {
            if (remaining >= 24) return 4;
            if (remaining >= 12) return 3;
            if (remaining >= 5) return 2;
            return 1;
        }

        private long GetDynamicVolumeTarget(int remaining, long remainingVolume)
        {
            if (remaining >= 24) return remainingVolume / 6;
            if (remaining >= 12) return remainingVolume / 5;
            if (remaining >= 5) return remainingVolume / 4;
            return remainingVolume / 2;
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
