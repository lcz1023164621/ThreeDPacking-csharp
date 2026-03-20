using System;
using System.Collections.Generic;
using System.Linq;
using ThreeDPacking.Core.Models;
using ThreeDPacking.Core.Points;

namespace ThreeDPacking.Core.Packers
{
    /// <summary>
    /// 牛皮纸填充器 - 复用与装箱一致的极值点算法（PointCalculator3D）
    /// 策略：
    /// 1. 装箱完全完成后，基于已放置的物品生成极值点
    /// 2. 使用与装箱相同的贪心策略：遍历所有极值点，选择能放置最大体积牛皮纸的位置
    /// 3. 放置牛皮纸后，在其右侧、前方、上方生成新的极值点
    /// 4. 执行约束切割，删除被包含的极值点
    /// 5. 重复直到无法再放置
    /// 约束：
    /// 1. 牛皮纸宽度固定110，高度固定70，长度可变（底面可旋转：110xN 或 Nx110）
    /// 2. 最小支撑比例10%
    /// 3. 底面中心点必须有支撑
    /// </summary>
    public class PaddingPaperPacker
    {
        // 最小支撑比例：越低越容易放置在支撑较弱的位置（更贴近用户“继续放宽”诉求）
        private const float MinSupportRatio = 0.05f;
        // point.Dz 是极值点算法给出的“可用高度”，在当前实现中可能对局部空间做保守裁剪。
        // 牛皮纸高度固定为 70，但为了避免错过“实际上不碰撞”的候选点，这里允许一定 slack。
        // 经验值：允许 point.Dz >= 60（即 70 - 10），碰撞检查仍会兜底拒绝真正不可行的放置。
        private const int MinPointDzForPadding = PaddingPaper.DefaultHeight - 10; // 70 -> 60

        public void FillWithPaddingPaper(Container container)
        {
            var stack = container.Stack;
            if (stack.IsEmpty)
            {
                Console.WriteLine($"\n[牛皮纸] 容器 {container.Id} Stack为空，跳过");
                return;
            }

            var itemPlacements = stack.Placements.Where(p => !p.IsPadding).ToList();
            var paddingPapers = new List<PaddingPaper>();

            Console.WriteLine($"\n[牛皮纸] 容器: {container.Id} 尺寸: {container.LoadDx}x{container.LoadDy}x{container.LoadDz} 物品: {itemPlacements.Count}个");

            // 使用与装箱一致的3D极值点计算器来重建当前容器的可用空间
            // 对牛皮纸填充：放宽极值点生成时的“顶部支撑”要求，避免因 PointCalculator3D 过保守导致找不到可用空隙
            // 实际悬空/支撑仍由 PaddingPaperPacker 的碰撞检查与支撑比例 MinSupportRatio 二次兜底。
            var pointCalc = new PointCalculator3D(MinSupportRatio);
            pointCalc.ClearToSize(container.LoadDx, container.LoadDy, container.LoadDz);

            // 通过“找包含原点的点 + Add”的方式，把已装物品占用空间写入极值点计算器
            // 这样后续牛皮纸放置就会走与装箱一致的：Add->ConstrainPoints->RemoveEclipsedPoints
            RebuildPointsFromPlacements(pointCalc, itemPlacements);

            bool placedAny = true;
            int maxIterations = 1000;
            int iteration = 0;

            while (placedAny && iteration < maxIterations && !pointCalc.IsEmpty)
            {
                placedAny = false;
                iteration++;

                PaddingPaper bestPaper = null;
                int bestPointIndex = -1;
                long bestVolume = 0;
                int bestZ = int.MaxValue;

                // 当前可用极值点快照（循环内会被Add更新）
                int pointCount = pointCalc.PointCount;
                for (int i = 0; i < pointCount; i++)
                {
                    var point = pointCalc.GetPoint(i);
                    var paper = TryCreatePaddingAtExtremePoint(point, container, itemPlacements, paddingPapers);
                    if (paper == null)
                        continue;

                    // 激进但有序的填充策略：
                    // 1) 先按 Z 从低到高（优先把低层可放空间吃干净，避免过早占用顶层）
                    // 2) 同层再按体积最大优先
                    bool isBetter = false;
                    if (paper.Z < bestZ)
                    {
                        isBetter = true;
                    }
                    else if (paper.Z == bestZ && paper.Volume > bestVolume)
                    {
                        isBetter = true;
                    }

                    if (isBetter)
                    {
                        bestZ = paper.Z;
                        bestVolume = paper.Volume;
                        bestPaper = paper;
                        bestPointIndex = i;
                    }
                }

                if (bestPaper != null)
                {
                    paddingPapers.Add(bestPaper);
                    placedAny = true;

                    // 将牛皮纸作为Placement写入stack和pointCalc，持续堆叠找最大体积方案
                    var placement = bestPaper.ToPlacement();
                    stack.Add(placement);
                    pointCalc.Add(bestPointIndex, placement);

                    Console.WriteLine($"[牛皮纸] 放置: ({bestPaper.X},{bestPaper.Y},{bestPaper.Z}) 尺寸({bestPaper.Dx}x{bestPaper.Dy}x{bestPaper.Dz}) 体积={bestPaper.Volume}");
                }
            }

            PrintPaddingInfo(container, paddingPapers);
        }

        private void RebuildPointsFromPlacements(PointCalculator3D pointCalc, List<Placement> placements)
        {
            // 采用确定性顺序，尽量稳定重建
            var sorted = placements
                .Where(p => p != null && p.StackValue != null)
                .OrderBy(p => p.Z)
                .ThenBy(p => p.X)
                .ThenBy(p => p.Y)
                .ToList();

            foreach (var placement in sorted)
            {
                int pointIndex = FindContainingPointIndex(pointCalc, placement.X, placement.Y, placement.Z);
                if (pointIndex < 0)
                {
                    // 正常情况下不应发生；跳过以避免崩溃
                    continue;
                }
                pointCalc.Add(pointIndex, placement);
            }
        }

        private int FindContainingPointIndex(PointCalculator3D pointCalc, int x, int y, int z)
        {
            for (int i = 0; i < pointCalc.PointCount; i++)
            {
                var pt = pointCalc.GetPoint(i);
                if (pt.MinX <= x && pt.MinY <= y && pt.MinZ <= z &&
                    pt.MaxX >= x && pt.MaxY >= y && pt.MaxZ >= z)
                    return i;
            }
            return -1;
        }

        private PaddingPaper TryCreatePaddingAtExtremePoint(ExtremePoint point, Container container,
            List<Placement> itemPlacements, List<PaddingPaper> paddingPapers)
        {
            int x = point.MinX, y = point.MinY, z = point.MinZ;

            if (x < 0 || y < 0 || z < 0) return null;
            if (x >= container.LoadDx || y >= container.LoadDy) return null;
            if (z + PaddingPaper.DefaultHeight > container.LoadDz) return null;
            // 只要求 Z 方向能覆盖牛皮纸高度；
            // 为了更激进地铺牛皮纸：X/Y 不先做“过度截断”，而是直接用容器边界作为上限，
            // 交由碰撞检测 + 支撑比例筛掉真正不可行的候选。
            if (point.Dz < MinPointDzForPadding)
                return null;

            int maxDx = container.LoadDx - x; // 沿 X 的最大长度上限
            int maxDy = container.LoadDy - y; // 沿 Y 的最大长度上限
            if (maxDx < PaddingPaper.MinSize && maxDy < PaddingPaper.MinSize)
                return null;

            // 关键：同时尝试两种底面朝向（160xN 与 Nx160），并在“支撑/碰撞”约束后选体积最大者
            // 否则会出现：第一种朝向因中心点支撑失败而被否决，但第二种朝向其实可行，却没有被尝试
            return CreateBestFeasiblePadding(x, y, z, maxDx, maxDy, itemPlacements, paddingPapers);
        }

        /// <summary>
        /// 计算从 (x,y,z) 出发可放置牛皮纸的最大底面范围 (maxDx, maxDy)。
        /// 只有与当前候选矩形 [x,maxX) x [y,maxY) 在 2D 上真正重叠的障碍才允许截断对应维度，
        /// 避免侧面障碍误把“可延长的 160×N”截成 160×160。
        /// </summary>
        private (int maxDx, int maxDy) CalculateMaxPaddingDimensions(int x, int y, int z, ExtremePoint point,
            Container container, List<Placement> itemPlacements, List<PaddingPaper> paddingPapers)
        {
            // 关键改动：
            // 极端点的 MaxX/MaxY 通常是“考虑了 point.Dz 整段”的保守可用范围。
            // 但牛皮纸的高度是固定 DefaultHeight，我们只关心 [z, z+DefaultHeight) 这一段 Z。
            // 因此这里允许底面在 X/Y 上重新扩张（只在本高度范围内被障碍截断），
            // 避免出现：上层物体把极端点过度截短，导致底部仍可继续铺牛皮纸但算法找不到。
            int maxX = container.LoadDx; // half-open boundary: [x, maxX)
            int maxY = container.LoadDy; // half-open boundary: [y, maxY)
            int paperTopZ = z + PaddingPaper.DefaultHeight - 1;

            // 与 [x,maxX) x [y,maxY) 在 2D 上重叠：item 的 Y 与 [y,maxY) 相交 且 item 的 X 与 [x,maxX) 相交
            bool overlapsRect(int ox, int oy, int oex, int oey) =>
                oex >= x && ox < maxX && oey >= y && oy < maxY;

            bool changed;
            do
            {
                changed = false;
                foreach (var item in itemPlacements)
                {
                    if (item.Z > paperTopZ || item.AbsoluteEndZ < z) continue;
                    if (!overlapsRect(item.X, item.Y, item.AbsoluteEndX, item.AbsoluteEndY)) continue;
                    // half-open 截断：当障碍从 nx 开始占用时，空闲区只能到 nx（即边界=nx）。
                    // 保持原策略为严格 > x：当障碍贴在起点（item.X == x）时，
                    // 可能仍存在通过缩小另一维（Y方向长度）来“绕开”的可行方案；
                    // 过度在这里截断会导致放置数量大幅下降。
                    if (item.X > x && item.X < maxX)
                    {
                        int nx = item.X;
                        if (nx < maxX) { maxX = nx; changed = true; }
                    }
                    if (item.Y > y && item.Y < maxY)
                    {
                        int ny = item.Y;
                        if (ny < maxY) { maxY = ny; changed = true; }
                    }
                }
                foreach (var paper in paddingPapers)
                {
                    if (paper.Z > paperTopZ || paper.AbsoluteEndZ < z) continue;
                    if (!overlapsRect(paper.X, paper.Y, paper.AbsoluteEndX, paper.AbsoluteEndY)) continue;
                    if (paper.X > x && paper.X < maxX)
                    {
                        int nx = paper.X;
                        if (nx < maxX) { maxX = nx; changed = true; }
                    }
                    if (paper.Y > y && paper.Y < maxY)
                    {
                        int ny = paper.Y;
                        if (ny < maxY) { maxY = ny; changed = true; }
                    }
                }
            } while (changed);

            return (maxX - x, maxY - y);
        }

        private PaddingPaper CreateBestFeasiblePadding(
            int x, int y, int z,
            int maxDx, int maxDy,
            List<Placement> itemPlacements,
            List<PaddingPaper> paddingPapers)
        {
            // 更激进策略：
            // 1) 对每种朝向（宽=110固定），把长度从“能到容器边界的最大值”开始向下扫描
            // 2) 第一个满足“无碰撞 + 支撑比例 >= MinSupportRatio”的候选即为该朝向下的最大长度
            // 3) 两种朝向再比较最终体积/支撑比例
            int dz = PaddingPaper.DefaultHeight;

            PaddingPaper best = null;
            long bestVolume = -1;
            float bestSupportRatio = -1f;

            // 朝向A：宽度110放在Y方向，X方向（长度）可变
            if (maxDy >= PaddingPaper.DefaultWidth && maxDx >= PaddingPaper.MinLength)
            {
                for (int dx = maxDx; dx >= PaddingPaper.MinLength; dx--)
                {
                    var paper = new PaddingPaper(x, y, z, dx, PaddingPaper.DefaultWidth, dz);
                    if (HasCollisionWithItems(paper, itemPlacements) || HasCollisionWithPadding(paper, paddingPapers))
                        continue;
                    if (!TryGetSupportRatio(paper, itemPlacements, paddingPapers, out var supportRatio))
                        continue;

                    // dx 从大到小扫描，当前命中即为朝向A下的最大长度
                    best = paper;
                    bestVolume = paper.Volume;
                    bestSupportRatio = supportRatio;
                    break;
                }
            }

            // 朝向B：宽度110放在X方向，Y方向（长度）可变
            if (maxDx >= PaddingPaper.DefaultWidth && maxDy >= PaddingPaper.MinLength)
            {
                for (int dy = maxDy; dy >= PaddingPaper.MinLength; dy--)
                {
                    var paper = new PaddingPaper(x, y, z, PaddingPaper.DefaultWidth, dy, dz);
                    if (HasCollisionWithItems(paper, itemPlacements) || HasCollisionWithPadding(paper, paddingPapers))
                        continue;
                    if (!TryGetSupportRatio(paper, itemPlacements, paddingPapers, out var supportRatio))
                        continue;

                    // dy 从大到小扫描，当前命中即为朝向B下的最大长度
                    long volume = paper.Volume;
                    if (best == null || volume > bestVolume || (volume == bestVolume && supportRatio > bestSupportRatio))
                    {
                        best = paper;
                        bestVolume = volume;
                        bestSupportRatio = supportRatio;
                    }
                    break;
                }
            }

            return best;
        }

        private bool TryGetSupportRatio(
            PaddingPaper paper,
            List<Placement> itemPlacements,
            List<PaddingPaper> paddingPapers,
            out float supportRatio)
        {
            supportRatio = 0f;

            if (paper.Z == 0)
            {
                supportRatio = 1f;
                return true;
            }

            long bottomArea = (long)paper.Dx * paper.Dy;
            long supportedArea = 0;
            int paperBottomZ = paper.Z;

            foreach (var item in itemPlacements)
            {
                if (item != null && item.AbsoluteEndZ == paperBottomZ - 1)
                    supportedArea += CalculateOverlapArea2D(paper, item);
            }

            foreach (var existingPaper in paddingPapers)
            {
                if (existingPaper != null && existingPaper.AbsoluteEndZ == paperBottomZ - 1)
                    supportedArea += CalculateOverlapArea2D(paper, existingPaper);
            }

            supportRatio = bottomArea > 0 ? (float)supportedArea / bottomArea : 0f;

            if (supportRatio < MinSupportRatio)
                return false;

            // 仅用支撑面积约束即可：
            // - supportedArea / bottomArea 已经衡量了底面与下一层投影重叠面积
            // - 采样中心/四角点进行“离散点支撑”容易产生误判（支撑可能是条带/不覆盖采样点）
            return true;
        }

        private long CalculateOverlapArea2D(PaddingPaper paper, Placement placement)
        {
            int overlapXStart = Math.Max(paper.X, placement.X);
            int overlapXEnd = Math.Min(paper.AbsoluteEndX, placement.AbsoluteEndX);
            int overlapX = overlapXEnd - overlapXStart + 1;

            int overlapYStart = Math.Max(paper.Y, placement.Y);
            int overlapYEnd = Math.Min(paper.AbsoluteEndY, placement.AbsoluteEndY);
            int overlapY = overlapYEnd - overlapYStart + 1;

            if (overlapX <= 0 || overlapY <= 0)
                return 0;

            return (long)overlapX * overlapY;
        }

        private long CalculateOverlapArea2D(PaddingPaper paper1, PaddingPaper paper2)
        {
            int overlapXStart = Math.Max(paper1.X, paper2.X);
            int overlapXEnd = Math.Min(paper1.AbsoluteEndX, paper2.AbsoluteEndX);
            int overlapX = overlapXEnd - overlapXStart + 1;

            int overlapYStart = Math.Max(paper1.Y, paper2.Y);
            int overlapYEnd = Math.Min(paper1.AbsoluteEndY, paper2.AbsoluteEndY);
            int overlapY = overlapYEnd - overlapYStart + 1;

            if (overlapX <= 0 || overlapY <= 0)
                return 0;

            return (long)overlapX * overlapY;
        }

        private bool IsPointSupported(int x, int y, int placementBottomZ, List<Placement> itemPlacements, List<PaddingPaper> paddingPapers)
        {
            foreach (var item in itemPlacements)
            {
                if (x >= item.X && x <= item.AbsoluteEndX &&
                    y >= item.Y && y <= item.AbsoluteEndY)
                {
                    if (item.AbsoluteEndZ + 1 == placementBottomZ)
                        return true;
                }
            }

            foreach (var paper in paddingPapers)
            {
                if (x >= paper.X && x <= paper.AbsoluteEndX &&
                    y >= paper.Y && y <= paper.AbsoluteEndY)
                {
                    if (paper.AbsoluteEndZ + 1 == placementBottomZ)
                        return true;
                }
            }

            return false;
        }

        private bool HasCollisionWithItems(PaddingPaper paper, List<Placement> items)
        {
            foreach (var item in items)
            {
                if (paper.Intersects3D(item))
                    return true;
            }
            return false;
        }

        private bool HasCollisionWithPadding(PaddingPaper paper, List<PaddingPaper> existingPapers)
        {
            foreach (var existing in existingPapers)
            {
                if (paper.Intersects3D(existing))
                    return true;
            }
            return false;
        }

        private void PrintPaddingInfo(Container container, List<PaddingPaper> paddingPapers)
        {
            Console.WriteLine($"\n===== 牛皮纸填充信息 =====");
            Console.WriteLine($"容器: {container.Id} ({container.LoadDx}x{container.LoadDy}x{container.LoadDz})");
            Console.WriteLine($"填充纸数量: {paddingPapers.Count}");

            if (paddingPapers.Count > 0)
            {
                Console.WriteLine();
                long totalPaddingVolume = 0;
                foreach (var paper in paddingPapers)
                {
                    Console.WriteLine($"  填充纸: 位置({paper.X}, {paper.Y}, {paper.Z}) 尺寸({paper.Dx} x {paper.Dy} x {paper.Dz})");
                    totalPaddingVolume += paper.Volume;
                }
                Console.WriteLine($"\n填充纸总体积: {totalPaddingVolume}");
                Console.WriteLine($"========================\n");
            }
            else
            {
                Console.WriteLine($"没有生成任何填充纸\n========================\n");
            }
        }
    }
}
