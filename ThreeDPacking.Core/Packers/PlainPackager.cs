using System;
using System.Collections.Generic;
using ThreeDPacking.Core.Comparators;
using ThreeDPacking.Core.Models;
using ThreeDPacking.Core.Points;

namespace ThreeDPacking.Core.Packers
{
    /// <summary>
    /// 3D free-form stacking packager. Places items at any valid 3D extreme point.
    /// Corresponds to Java PlainPackager + AbstractControlPackager.pack().
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
                var placement = _selector.GetBestPlacement(
                    source, pointCalc, container, stack, remainingWeight, remainingVolume);

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
