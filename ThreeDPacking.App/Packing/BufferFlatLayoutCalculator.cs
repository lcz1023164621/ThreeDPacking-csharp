using System;
using System.Collections.Generic;
using System.Linq;

namespace ThreeDPacking.App.Packing
{
    /// <summary>
    /// 暂存台平面平铺排布（不叠放），台面默认 500×500mm。
    /// </summary>
    public static class BufferFlatLayoutCalculator
    {
        public const int DefaultPlatformWidthMm = 500;
        public const int DefaultPlatformHeightMm = 500;

        public sealed class LayoutItem
        {
            public int Sequence { get; set; }
            public int Dx { get; set; }
            public int Dy { get; set; }
            public int Dz { get; set; }
        }

        public sealed class LayoutSlot
        {
            public int Sequence { get; set; }
            public int TopCenterX { get; set; }
            public int TopCenterY { get; set; }
            public int TopCenterZ { get; set; }
            public int PlacedDx { get; set; }
            public int PlacedDy { get; set; }
            public bool FitsOnPlatform { get; set; }
        }

        public static List<LayoutSlot> Layout(
            IEnumerable<LayoutItem> items,
            int platformWidthMm = DefaultPlatformWidthMm,
            int platformHeightMm = DefaultPlatformHeightMm)
        {
            var result = new List<LayoutSlot>();
            if (items == null)
                return result;

            int cursorX = 0;
            int cursorY = 0;
            int rowHeight = 0;

            foreach (LayoutItem item in items.OrderBy(i => i.Sequence))
            {
                if (item == null || item.Sequence <= 0 || item.Dx <= 0 || item.Dy <= 0 || item.Dz <= 0)
                    continue;

                int dx = item.Dx;
                int dy = item.Dy;
                int dz = item.Dz;

                if (!FitsInRow(cursorX, dx, platformWidthMm) && !FitsInRow(cursorX, dy, platformWidthMm))
                {
                    cursorX = 0;
                    cursorY += rowHeight;
                    rowHeight = 0;
                }

                if (!TryChooseOrientation(cursorX, ref dx, ref dy, platformWidthMm))
                {
                    cursorX = 0;
                    cursorY += rowHeight;
                    rowHeight = 0;
                    dx = item.Dx;
                    dy = item.Dy;
                    TryChooseOrientation(cursorX, ref dx, ref dy, platformWidthMm);
                }

                bool fits = cursorY + dy <= platformHeightMm && cursorX + dx <= platformWidthMm;
                var slot = new LayoutSlot
                {
                    Sequence = item.Sequence,
                    TopCenterX = cursorX + dx / 2,
                    TopCenterY = cursorY + dy / 2,
                    TopCenterZ = dz,
                    PlacedDx = dx,
                    PlacedDy = dy,
                    FitsOnPlatform = fits
                };
                result.Add(slot);

                cursorX += dx;
                rowHeight = Math.Max(rowHeight, dy);
            }

            return result;
        }

        private static bool FitsInRow(int cursorX, int size, int platformWidthMm)
        {
            return cursorX + size <= platformWidthMm;
        }

        private static bool TryChooseOrientation(int cursorX, ref int dx, ref int dy, int platformWidthMm)
        {
            if (FitsInRow(cursorX, dx, platformWidthMm))
                return true;

            if (dx != dy && FitsInRow(cursorX, dy, platformWidthMm))
            {
                int tmp = dx;
                dx = dy;
                dy = tmp;
                return true;
            }

            return false;
        }
    }
}
