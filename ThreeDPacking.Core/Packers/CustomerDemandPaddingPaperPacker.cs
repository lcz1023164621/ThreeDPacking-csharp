using System;
using System.Collections.Generic;
using System.Linq;
using ThreeDPacking.Core.Models;

namespace ThreeDPacking.Core.Packers
{
    /// <summary>
    /// 客户实际需求装填策略：
    /// - 底部预铺由 Orchestrator 按条件处理
    /// - 装箱后优先在顶部叠放；若顶部无法放置，则回退尝试中间层
    /// - 顶部/中间层放置不要求支撑面积，只要不碰撞且尺寸可放入即可
    /// </summary>
    public class CustomerDemandPaddingPaperPacker : IPaddingPaperPacker
    {
        private const int MinPaddingLengthForPlacement = PaddingPaper.MinLength;

        private readonly int _minPaddingWidth;

        public CustomerDemandPaddingPaperPacker(int minPaddingWidth = PaddingPaper.DefaultWidth)
        {
            _minPaddingWidth = Math.Max(PaddingPaper.MinSize, minPaddingWidth);
        }

        public void FillWithPaddingPaper(Container container)
        {
            FillWithPaddingPaper(container, null);
        }

        public void FillWithPaddingPaper(Container container, Action<string> log)
        {
            if (container?.Stack == null || container.Stack.IsEmpty)
                return;

            var stack = container.Stack;
            var itemPlacements = stack.Placements.Where(p => p != null && !p.IsPadding).ToList();
            var paddingPapers = stack.Placements
                .Where(p => p != null && p.IsPadding && p.StackValue != null)
                .Select(p => new PaddingPaper(p.X, p.Y, p.Z, p.StackValue.Dx, p.StackValue.Dy, p.StackValue.Dz))
                .ToList();

            int topLevelZ = GetTopStartZ(stack);
            bool placedAtTop = false;

            if (CanPlaceAtLevel(container, topLevelZ))
            {
                placedAtTop = FillSingleLevel(container, topLevelZ, itemPlacements, paddingPapers, stack, log, "顶部");
            }

            // 顶层放不下时，回退尝试中间层（从高到低）
            if (placedAtTop)
                return;

            var middleLevels = BuildMiddleLevelCandidates(stack, container, topLevelZ);
            foreach (int level in middleLevels)
            {
                if (FillSingleLevel(container, level, itemPlacements, paddingPapers, stack, log, "中间层"))
                    break;
            }
        }

        private static int GetTopStartZ(PackStack stack)
        {
            int maxEndZ = -1;
            foreach (var p in stack.Placements)
            {
                if (p?.StackValue == null)
                    continue;
                maxEndZ = Math.Max(maxEndZ, p.AbsoluteEndZ);
            }
            return maxEndZ + 1;
        }

        private static bool CanPlaceAtLevel(Container container, int z)
        {
            return z >= 0 && z + PaddingPaper.DefaultHeight <= container.LoadDz;
        }

        private static List<int> BuildMiddleLevelCandidates(PackStack stack, Container container, int topLevelZ)
        {
            var levels = new HashSet<int>();
            foreach (var p in stack.Placements)
            {
                if (p?.StackValue == null)
                    continue;

                int z = p.AbsoluteEndZ + 1;
                if (z > 0 && z < topLevelZ && z + PaddingPaper.DefaultHeight <= container.LoadDz)
                    levels.Add(z);
            }

            return levels.OrderByDescending(v => v).ToList();
        }

        private bool FillSingleLevel(
            Container container,
            int z,
            List<Placement> itemPlacements,
            List<PaddingPaper> paddingPapers,
            PackStack stack,
            Action<string> log,
            string levelName)
        {
            bool placedAny = false;
            int safetyCounter = 0;
            const int maxIterations = 200;

            while (safetyCounter < maxIterations)
            {
                safetyCounter++;
                var best = TryCreateBestPaddingAtLevel(container, z, itemPlacements, paddingPapers);
                if (best == null)
                    break;

                paddingPapers.Add(best);
                stack.Add(best.ToPlacement());
                placedAny = true;
                Log(log, $"[牛皮纸-{levelName}] 位置({best.X},{best.Y},{best.Z}) 尺寸({best.Dx}x{best.Dy}x{best.Dz})");
            }

            return placedAny;
        }

        private PaddingPaper TryCreateBestPaddingAtLevel(
            Container container,
            int z,
            List<Placement> itemPlacements,
            List<PaddingPaper> paddingPapers)
        {
            PaddingPaper best = null;
            long bestVolume = -1;

            var xAnchors = new HashSet<int> { 0 };
            var yAnchors = new HashSet<int> { 0 };

            foreach (var item in itemPlacements)
            {
                int nx = item.AbsoluteEndX + 1;
                int ny = item.AbsoluteEndY + 1;
                if (nx >= 0 && nx < container.LoadDx) xAnchors.Add(nx);
                if (ny >= 0 && ny < container.LoadDy) yAnchors.Add(ny);
            }

            foreach (var paper in paddingPapers)
            {
                int nx = paper.AbsoluteEndX + 1;
                int ny = paper.AbsoluteEndY + 1;
                if (nx >= 0 && nx < container.LoadDx) xAnchors.Add(nx);
                if (ny >= 0 && ny < container.LoadDy) yAnchors.Add(ny);
            }

            foreach (int x in xAnchors.OrderBy(v => v))
            {
                foreach (int y in yAnchors.OrderBy(v => v))
                {
                    int maxDx = container.LoadDx - x;
                    int maxDy = container.LoadDy - y;
                    if (maxDx <= 0 || maxDy <= 0)
                        continue;

                    var paper = CreateBestFeasiblePadding(x, y, z, maxDx, maxDy, itemPlacements, paddingPapers);
                    if (paper == null)
                        continue;

                    bool isBetter = best == null
                        || paper.Volume > bestVolume
                        || (paper.Volume == bestVolume && (paper.X < best.X || (paper.X == best.X && paper.Y < best.Y)));

                    if (isBetter)
                    {
                        best = paper;
                        bestVolume = paper.Volume;
                    }
                }
            }

            return best;
        }

        private PaddingPaper CreateBestFeasiblePadding(
            int x,
            int y,
            int z,
            int maxDx,
            int maxDy,
            List<Placement> itemPlacements,
            List<PaddingPaper> paddingPapers)
        {
            int dz = PaddingPaper.DefaultHeight;
            PaddingPaper best = null;
            long bestVolume = -1;

            if (maxDy >= _minPaddingWidth && maxDx >= MinPaddingLengthForPlacement)
            {
                for (int dx = maxDx; dx >= MinPaddingLengthForPlacement; dx--)
                {
                    var paper = new PaddingPaper(x, y, z, dx, _minPaddingWidth, dz);
                    if (HasCollisionWithItems(paper, itemPlacements) || HasCollisionWithPadding(paper, paddingPapers))
                        continue;

                    best = paper;
                    bestVolume = paper.Volume;
                    break;
                }
            }

            if (maxDx >= _minPaddingWidth && maxDy >= MinPaddingLengthForPlacement)
            {
                for (int dy = maxDy; dy >= MinPaddingLengthForPlacement; dy--)
                {
                    var paper = new PaddingPaper(x, y, z, _minPaddingWidth, dy, dz);
                    if (HasCollisionWithItems(paper, itemPlacements) || HasCollisionWithPadding(paper, paddingPapers))
                        continue;

                    long volume = paper.Volume;
                    bool isBetter = best == null || volume > bestVolume;
                    if (isBetter)
                    {
                        best = paper;
                        bestVolume = volume;
                    }
                    break;
                }
            }

            return best;
        }

        private static bool HasCollisionWithItems(PaddingPaper paper, List<Placement> items)
        {
            foreach (var item in items)
            {
                if (paper.Intersects3D(item))
                    return true;
            }
            return false;
        }

        private static bool HasCollisionWithPadding(PaddingPaper paper, List<PaddingPaper> existingPapers)
        {
            foreach (var existing in existingPapers)
            {
                if (paper.Intersects3D(existing))
                    return true;
            }
            return false;
        }

        private static void Log(Action<string> log, string message)
        {
            if (log != null) log(message);
            else Console.WriteLine(message);
        }
    }
}
