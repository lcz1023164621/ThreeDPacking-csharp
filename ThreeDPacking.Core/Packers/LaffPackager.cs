using System;
using System.Collections.Generic;
using ThreeDPacking.Core.Comparators;
using ThreeDPacking.Core.Models;
using ThreeDPacking.Core.Points;

namespace ThreeDPacking.Core.Packers
{
    /// <summary>
    /// Largest Area Fit First packager with layer-based approach.
    /// Uses 2D point calculator - only places boxes along the floor of each level.
    /// Corresponds to Java FastLargestAreaFitFirstPackager + AbstractLargestAreaFitFirstPackager.pack().
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
                    placement = _firstSelector.GetFirstPlacement(
                        source, pointCalc, container, remainingWeight, remainingVolume);

                    if (placement == null)
                        break;

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
                    placement = _selector.GetBestPlacement(
                        source, pointCalc, container, stack, remainingWeight, remainingVolume);

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
    }
}
