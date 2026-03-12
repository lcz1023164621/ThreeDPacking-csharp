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
                        bool intersects = false;
                        foreach (var existing in stack.Placements)
                        {
                            if (placement.Intersects3D(existing))
                            {
                                intersects = true;
                                break;
                            }
                        }
                        if (intersects)
                            continue;
                        
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
    }
}
