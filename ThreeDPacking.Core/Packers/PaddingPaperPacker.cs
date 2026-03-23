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
        private const float MinSupportRatio = 0.01f;
        // 侧面接触支撑：用于“由侧面接触形成悬空”的场景
        // 仍复用同一个比例阈值，保持行为可控；如果后续仍太保守再单独调小。
        private const float MinSideSupportRatio = MinSupportRatio;
        // point.Dz 是极值点算法给出的“可用高度”，在当前实现中可能对局部空间做保守裁剪。
        // 牛皮纸高度固定为 70，但为了避免错过“实际上不碰撞”的候选点，这里允许一定 slack。
        // 经验值：允许 point.Dz >= 60（即 70 - 10），碰撞检查仍会兜底拒绝真正不可行的放置。
        // 极值点的 Dz 可能因为点算法的保守裁剪而偏小；
        // 为避免错过“实际上可碰撞”的上层候选，让它尽可能放宽。
        private const int MinPointDzForPadding = 0;
        // 最小长度：用于控制牛皮纸沿可变方向的最短铺放长度（>=200mm）
        private const int MinPaddingLengthForPlacement = PaddingPaper.MinLength; // 200

        public void FillWithPaddingPaper(Container container)
        {
            FillWithPaddingPaper(container, null);
        }

        public void FillWithPaddingPaper(Container container, Action<string> log)
        {
            var stack = container.Stack;
            if (stack.IsEmpty)
                return;

            var itemPlacements = stack.Placements.Where(p => !p.IsPadding).ToList();
            var paddingPapers = new List<PaddingPaper>();

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

                // 先做“落底优先”：只要底面还能放牛皮纸，就先吃掉底层空隙，再考虑上层。
                var floorPaper = TryCreateBestFloorPadding(container, itemPlacements, paddingPapers, out _);
                if (floorPaper != null)
                {
                    paddingPapers.Add(floorPaper);
                    placedAny = true;

                    var floorPlacement = floorPaper.ToPlacement();
                    stack.Add(floorPlacement);

                    int floorPointIndex = FindContainingPointIndex(pointCalc, floorPlacement);
                    if (floorPointIndex >= 0)
                    {
                        pointCalc.Add(floorPointIndex, floorPlacement);
                    }
                    else
                    {
                        RebuildPointsFromCurrentState(pointCalc, container, itemPlacements, paddingPapers);
                    }

                    continue;
                }

                // EP 可能漏掉“未被新点触达”的可行空隙；用锚点扫描补偿上层可放位。
                var anchoredPaper = TryCreateBestAnchoredPadding(container, itemPlacements, paddingPapers, out _);
                if (anchoredPaper != null)
                {
                    paddingPapers.Add(anchoredPaper);
                    placedAny = true;

                    var anchoredPlacement = anchoredPaper.ToPlacement();
                    stack.Add(anchoredPlacement);

                    int anchoredPointIndex = FindContainingPointIndex(pointCalc, anchoredPlacement);
                    if (anchoredPointIndex >= 0)
                    {
                        pointCalc.Add(anchoredPointIndex, anchoredPlacement);
                    }
                    else
                    {
                        RebuildPointsFromCurrentState(pointCalc, container, itemPlacements, paddingPapers);
                    }

                    continue;
                }

                PaddingPaper bestPaper = null;
                int bestPointIndex = -1;
                long bestVolume = 0;
                int bestZ = int.MaxValue;
                float bestSupportRatio = -1f;
                // 当前可用极值点快照（循环内会被Add更新）
                int pointCount = pointCalc.PointCount;
                for (int i = 0; i < pointCount; i++)
                {
                    var point = pointCalc.GetPoint(i);
                    var paper = TryCreatePaddingAtExtremePoint(point, container, itemPlacements, paddingPapers);
                    if (paper == null)
                        continue;
                    if (!TryGetSupportRatio(paper, itemPlacements, paddingPapers, out var supportRatio))
                        continue;

                    // 激进但有序的填充策略：
                    // 1) 先按 Z 从低到高（优先把低层可放空间吃干净，避免过早占用顶层）
                    // 2) 同层优先支撑更强（更稳定）
                    // 3) 再按体积最大优先
                    bool isBetter = false;
                    if (paper.Z < bestZ)
                    {
                        isBetter = true;
                    }
                    else if (paper.Z == bestZ && supportRatio > bestSupportRatio)
                    {
                        isBetter = true;
                    }
                    else if (paper.Z == bestZ && Math.Abs(supportRatio - bestSupportRatio) < 1e-6f && paper.Volume > bestVolume)
                    {
                        isBetter = true;
                    }

                    if (isBetter)
                    {
                        bestZ = paper.Z;
                        bestVolume = paper.Volume;
                        bestSupportRatio = supportRatio;
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
                    if (bestPointIndex >= 0)
                    {
                        pointCalc.Add(bestPointIndex, placement);
                    }
                    else
                    {
                        RebuildPointsFromCurrentState(pointCalc, container, itemPlacements, paddingPapers);
                    }
                }
            }

            PrintPaddingInfo(paddingPapers, log);
        }

        private void RebuildPointsFromPlacements(PointCalculator3D pointCalc, List<Placement> placements)
        {
            // 关键：PointCalculator3D 的 Add/切割对“添加顺序”敏感。
            // 这里保持 placements 的原始放置顺序（来自 stack.Placements），避免重建出与真实剩余空间不一致的极值点。
            var ordered = placements
                .Where(p => p != null && p.StackValue != null)
                .ToList();

            foreach (var placement in ordered)
            {
                int pointIndex = FindContainingPointIndex(pointCalc, placement);
                if (pointIndex < 0)
                    continue;
                pointCalc.Add(pointIndex, placement);
            }
        }

        private int FindContainingPointIndex(PointCalculator3D pointCalc, Placement placement)
        {
            for (int i = 0; i < pointCalc.PointCount; i++)
            {
                var pt = pointCalc.GetPoint(i);
                // 必须完整包含 placement 的体积（否则 Add 使用错误的极值点会导致后续点集偏差）
                if (pt.MinX <= placement.X && pt.MinY <= placement.Y && pt.MinZ <= placement.Z &&
                    pt.MaxX >= placement.AbsoluteEndX && pt.MaxY >= placement.AbsoluteEndY && pt.MaxZ >= placement.AbsoluteEndZ)
                    return i;
            }
            return -1;
        }

        private void RebuildPointsFromCurrentState(
            PointCalculator3D pointCalc,
            Container container,
            List<Placement> itemPlacements,
            List<PaddingPaper> paddingPapers)
        {
            pointCalc.ClearToSize(container.LoadDx, container.LoadDy, container.LoadDz);
            RebuildPointsFromPlacements(pointCalc, itemPlacements);

            foreach (var paper in paddingPapers)
            {
                var placement = paper.ToPlacement();
                int pointIndex = FindContainingPointIndex(pointCalc, placement);
                if (pointIndex < 0)
                    continue;
                pointCalc.Add(pointIndex, placement);
            }
        }

        private PaddingPaper TryCreateBestFloorPadding(
            Container container,
            List<Placement> itemPlacements,
            List<PaddingPaper> paddingPapers,
            out float bestSupportRatio)
        {
            PaddingPaper best = null;
            bestSupportRatio = -1f;
            long bestVolume = -1;

            var xAnchors = new HashSet<int> { 0 };
            var yAnchors = new HashSet<int> { 0 };

            foreach (var item in itemPlacements)
            {
                int nextX = item.AbsoluteEndX + 1;
                int nextY = item.AbsoluteEndY + 1;
                if (nextX >= 0 && nextX < container.LoadDx) xAnchors.Add(nextX);
                if (nextY >= 0 && nextY < container.LoadDy) yAnchors.Add(nextY);
            }

            foreach (var paper in paddingPapers)
            {
                int nextX = paper.AbsoluteEndX + 1;
                int nextY = paper.AbsoluteEndY + 1;
                if (nextX >= 0 && nextX < container.LoadDx) xAnchors.Add(nextX);
                if (nextY >= 0 && nextY < container.LoadDy) yAnchors.Add(nextY);
            }

            foreach (int x in xAnchors.OrderBy(v => v))
            {
                foreach (int y in yAnchors.OrderBy(v => v))
                {
                    int maxDx = container.LoadDx - x;
                    int maxDy = container.LoadDy - y;
                    if (maxDx <= 0 || maxDy <= 0)
                        continue;

                    var paper = CreateBestFeasiblePadding(
                        x, y, 0, maxDx, maxDy,
                        itemPlacements, paddingPapers);
                    if (paper == null)
                        continue;
                    if (!TryGetSupportRatio(paper, itemPlacements, paddingPapers, out var supportRatio))
                        continue;

                    bool isBetter = false;
                    if (best == null)
                    {
                        isBetter = true;
                    }
                    else if (supportRatio > bestSupportRatio)
                    {
                        isBetter = true;
                    }
                    else if (Math.Abs(supportRatio - bestSupportRatio) < 1e-6f)
                    {
                        if (paper.Volume > bestVolume)
                        {
                            isBetter = true;
                        }
                        else if (paper.Volume == bestVolume)
                        {
                            // 稳定且可复现：体积相同则靠左前优先。
                            if (paper.X < best.X || (paper.X == best.X && paper.Y < best.Y))
                                isBetter = true;
                        }
                    }

                    if (isBetter)
                    {
                        best = paper;
                        bestSupportRatio = supportRatio;
                        bestVolume = paper.Volume;
                    }
                }
            }

            return best;
        }

        private PaddingPaper TryCreateBestAnchoredPadding(
            Container container,
            List<Placement> itemPlacements,
            List<PaddingPaper> paddingPapers,
            out float bestSupportRatio)
        {
            bestSupportRatio = -1f;
            PaddingPaper best = null;
            long bestVolume = -1;
            int bestZ = int.MaxValue;

            var xAnchors = new HashSet<int> { 0 };
            var yAnchors = new HashSet<int> { 0 };
            var zAnchors = new HashSet<int> { 0 };

            foreach (var item in itemPlacements)
            {
                int nx = item.AbsoluteEndX + 1;
                int ny = item.AbsoluteEndY + 1;
                int nz = item.AbsoluteEndZ + 1;
                if (nx >= 0 && nx < container.LoadDx) xAnchors.Add(nx);
                if (ny >= 0 && ny < container.LoadDy) yAnchors.Add(ny);
                if (nz >= 0 && nz <= container.LoadDz - PaddingPaper.DefaultHeight) zAnchors.Add(nz);
            }

            foreach (var paper in paddingPapers)
            {
                int nx = paper.AbsoluteEndX + 1;
                int ny = paper.AbsoluteEndY + 1;
                int nz = paper.AbsoluteEndZ + 1;
                if (nx >= 0 && nx < container.LoadDx) xAnchors.Add(nx);
                if (ny >= 0 && ny < container.LoadDy) yAnchors.Add(ny);
                if (nz >= 0 && nz <= container.LoadDz - PaddingPaper.DefaultHeight) zAnchors.Add(nz);
            }

            foreach (int z in zAnchors.OrderBy(v => v))
            {
                foreach (int x in xAnchors.OrderBy(v => v))
                {
                    foreach (int y in yAnchors.OrderBy(v => v))
                    {
                        int maxDx = container.LoadDx - x;
                        int maxDy = container.LoadDy - y;
                        if (maxDx <= 0 || maxDy <= 0)
                            continue;

                        var paper = CreateBestFeasiblePadding(
                            x, y, z, maxDx, maxDy,
                            itemPlacements, paddingPapers);
                        if (paper == null)
                            continue;
                        if (!TryGetSupportRatio(paper, itemPlacements, paddingPapers, out var supportRatio))
                            continue;

                        bool isBetter = false;
                        if (best == null)
                        {
                            isBetter = true;
                        }
                        else if (paper.Z < bestZ)
                        {
                            isBetter = true;
                        }
                        else if (paper.Z == bestZ && supportRatio > bestSupportRatio)
                        {
                            isBetter = true;
                        }
                        else if (paper.Z == bestZ && Math.Abs(supportRatio - bestSupportRatio) < 1e-6f && paper.Volume > bestVolume)
                        {
                            isBetter = true;
                        }
                        else if (paper.Z == bestZ && Math.Abs(supportRatio - bestSupportRatio) < 1e-6f && paper.Volume == bestVolume)
                        {
                            if (paper.X < best.X || (paper.X == best.X && paper.Y < best.Y))
                                isBetter = true;
                        }

                        if (isBetter)
                        {
                            best = paper;
                            bestZ = paper.Z;
                            bestSupportRatio = supportRatio;
                            bestVolume = paper.Volume;
                        }
                    }
                }
            }

            return best;
        }

        private PaddingPaper TryCreatePaddingAtExtremePoint(
            ExtremePoint point,
            Container container,
            List<Placement> itemPlacements,
            List<PaddingPaper> paddingPapers)
        {
            int x = point.MinX, y = point.MinY;
            int zStartMin = point.MinZ;

            if (x < 0 || y < 0 || zStartMin < 0)
                return null;
            if (x >= container.LoadDx || y >= container.LoadDy)
                return null;
            if (point.Dz < MinPointDzForPadding)
                return null;

            int paperHeight = PaddingPaper.DefaultHeight;
            // zStartMax：paperBottomZ + (paperHeight-1) <= point.MaxZ 且 <= container.LoadDz-1
            int zStartMaxByPoint = point.MaxZ - (paperHeight - 1);
            int zStartMaxByContainer = container.LoadDz - paperHeight;
            int zStartMax = Math.Min(zStartMaxByPoint, zStartMaxByContainer);

            if (zStartMax < zStartMin)
                return null;

            // 思路2：严格用 EP 给出的剩余矩形范围做上限
            int maxDx = point.Dx;
            int maxDy = point.Dy;
            if (maxDx < PaddingPaper.MinSize && maxDy < PaddingPaper.MinSize)
                return null;

            // 思路3：同一 EP 内尝试不同的 z 起点（优先尝试更低的 zStart）
            var zCandidates = new HashSet<int>();
            zCandidates.Add(zStartMin);
            zCandidates.Add(zStartMax);

            foreach (var item in itemPlacements)
            {
                if (item == null) continue;
                int topPlus1 = item.AbsoluteEndZ + 1;
                if (topPlus1 >= zStartMin && topPlus1 <= zStartMax)
                    zCandidates.Add(topPlus1);
            }

            foreach (var existingPaper in paddingPapers)
            {
                if (existingPaper == null) continue;
                int topPlus1 = existingPaper.AbsoluteEndZ + 1;
                if (topPlus1 >= zStartMin && topPlus1 <= zStartMax)
                    zCandidates.Add(topPlus1);
            }

            foreach (var zCandidate in zCandidates.OrderBy(z => z))
            {
                var paper = CreateBestFeasiblePadding(
                    x, y, zCandidate, maxDx, maxDy,
                    itemPlacements, paddingPapers);

                if (paper != null) return paper;
            }

            return null;
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

            bool canTryA = maxDy >= PaddingPaper.DefaultWidth && maxDx >= MinPaddingLengthForPlacement;
            bool canTryB = maxDx >= PaddingPaper.DefaultWidth && maxDy >= MinPaddingLengthForPlacement;

            // 朝向A：宽度110放在Y方向，X方向（长度）可变
            if (canTryA)
            {
                for (int dx = maxDx; dx >= MinPaddingLengthForPlacement; dx--)
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
            if (canTryB)
            {
                for (int dy = maxDy; dy >= MinPaddingLengthForPlacement; dy--)
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
            int paperBottomZ = paper.Z;

            // 关键改动：允许“悬空/间隙支撑”
            // 不再只认紧贴正下方一层（AbsoluteEndZ == paperBottomZ-1），
            // 而是取纸底下方“最近的支撑面高度”（AbsoluteEndZ 最大的那一层），
            // 用该层与纸底的X-Y投影重叠面积作为 supportedArea。
            int nearestSupportEndZ = -1;
            long supportedArea = 0;

            foreach (var item in itemPlacements)
            {
                if (item == null) continue;
                if (item.AbsoluteEndZ >= paperBottomZ) continue; // 只统计下方

                long overlap = CalculateOverlapArea2D(paper, item);
                if (overlap <= 0) continue;

                if (item.AbsoluteEndZ > nearestSupportEndZ)
                {
                    nearestSupportEndZ = item.AbsoluteEndZ;
                    supportedArea = overlap;
                }
                else if (item.AbsoluteEndZ == nearestSupportEndZ)
                {
                    supportedArea += overlap;
                }
            }

            foreach (var existingPaper in paddingPapers)
            {
                if (existingPaper == null) continue;
                if (existingPaper.AbsoluteEndZ >= paperBottomZ) continue;

                long overlap = CalculateOverlapArea2D(paper, existingPaper);
                if (overlap <= 0) continue;

                if (existingPaper.AbsoluteEndZ > nearestSupportEndZ)
                {
                    nearestSupportEndZ = existingPaper.AbsoluteEndZ;
                    supportedArea = overlap;
                }
                else if (existingPaper.AbsoluteEndZ == nearestSupportEndZ)
                {
                    supportedArea += overlap;
                }
            }

            float bottomSupportRatio = bottomArea > 0 ? (float)supportedArea / bottomArea : 0f;

            // 允许的条件：
            // 1) 底面正下方有足够支撑（旧逻辑）
            if (bottomSupportRatio >= MinSupportRatio)
            {
                supportRatio = bottomSupportRatio;
                return true;
            }

            // 2) 底面不足时，允许“侧面接触支撑”来形成悬空
            // 左/右侧支撑面面积：paper.Dy * paper.Dz
            long sideSupportAreaX = 0;
            long sideFaceAreaX = (long)paper.Dy * paper.Dz;
            if (sideFaceAreaX > 0)
            {
                foreach (var item in itemPlacements)
                {
                    if (item == null) continue;

                    // item 在 paper 左侧（不相交，且正好贴到 paper 左边面）
                    if (item.AbsoluteEndX + 1 == paper.X)
                        sideSupportAreaX += CalculateSideContactAreaYZ(paper, item);

                    // item 在 paper 右侧（贴到 paper 右边面）
                    if (paper.AbsoluteEndX + 1 == item.X)
                        sideSupportAreaX += CalculateSideContactAreaYZ(paper, item);
                }

                foreach (var existingPaper in paddingPapers)
                {
                    if (existingPaper == null) continue;

                    if (existingPaper.AbsoluteEndX + 1 == paper.X)
                        sideSupportAreaX += CalculateSideContactAreaYZ(paper, existingPaper);

                    if (paper.AbsoluteEndX + 1 == existingPaper.X)
                        sideSupportAreaX += CalculateSideContactAreaYZ(paper, existingPaper);
                }
            }

            float sideSupportRatioX = sideFaceAreaX > 0 ? (float)sideSupportAreaX / sideFaceAreaX : 0f;
            sideSupportRatioX = Math.Min(sideSupportRatioX, 1f);

            // 前/后侧支撑面面积：paper.Dx * paper.Dz
            long sideSupportAreaY = 0;
            long sideFaceAreaY = (long)paper.Dx * paper.Dz;
            if (sideFaceAreaY > 0)
            {
                foreach (var item in itemPlacements)
                {
                    if (item == null) continue;

                    // item 在 paper 前侧（贴到 paper 前边面）
                    if (item.AbsoluteEndY + 1 == paper.Y)
                        sideSupportAreaY += CalculateSideContactAreaXZ(paper, item);

                    // item 在 paper 后侧（贴到 paper 后边面）
                    if (paper.AbsoluteEndY + 1 == item.Y)
                        sideSupportAreaY += CalculateSideContactAreaXZ(paper, item);
                }

                foreach (var existingPaper in paddingPapers)
                {
                    if (existingPaper == null) continue;

                    if (existingPaper.AbsoluteEndY + 1 == paper.Y)
                        sideSupportAreaY += CalculateSideContactAreaXZ(paper, existingPaper);

                    if (paper.AbsoluteEndY + 1 == existingPaper.Y)
                        sideSupportAreaY += CalculateSideContactAreaXZ(paper, existingPaper);
                }
            }

            float sideSupportRatioY = sideFaceAreaY > 0 ? (float)sideSupportAreaY / sideFaceAreaY : 0f;
            sideSupportRatioY = Math.Min(sideSupportRatioY, 1f);

            // 只要底面支撑或侧面支撑任一满足即可（取更大的作为最终 supportRatio 方便日志/调试）
            if (sideSupportRatioX >= MinSideSupportRatio || sideSupportRatioY >= MinSideSupportRatio)
            {
                supportRatio = Math.Max(bottomSupportRatio, Math.Max(sideSupportRatioX, sideSupportRatioY));
                return true;
            }

            return false;
        }

        private static long CalculateSideContactAreaYZ(PaddingPaper paper, Placement placement)
        {
            // side contact plane: X is fixed, overlap is in (Y, Z)
            int overlapYStart = Math.Max(paper.Y, placement.Y);
            int overlapYEnd = Math.Min(paper.AbsoluteEndY, placement.AbsoluteEndY);
            int overlapY = overlapYEnd - overlapYStart + 1;
            if (overlapY <= 0) return 0;

            int overlapZStart = Math.Max(paper.Z, placement.Z);
            int overlapZEnd = Math.Min(paper.AbsoluteEndZ, placement.AbsoluteEndZ);
            int overlapZ = overlapZEnd - overlapZStart + 1;
            if (overlapZ <= 0) return 0;

            // contact area = overlap in Y * overlap in Z
            return (long)overlapY * overlapZ;
        }

        private static long CalculateSideContactAreaYZ(PaddingPaper paper, PaddingPaper other)
        {
            int overlapYStart = Math.Max(paper.Y, other.Y);
            int overlapYEnd = Math.Min(paper.AbsoluteEndY, other.AbsoluteEndY);
            int overlapY = overlapYEnd - overlapYStart + 1;
            if (overlapY <= 0) return 0;

            int overlapZStart = Math.Max(paper.Z, other.Z);
            int overlapZEnd = Math.Min(paper.AbsoluteEndZ, other.AbsoluteEndZ);
            int overlapZ = overlapZEnd - overlapZStart + 1;
            if (overlapZ <= 0) return 0;

            return (long)overlapY * overlapZ;
        }

        private static long CalculateSideContactAreaXZ(PaddingPaper paper, Placement placement)
        {
            // side contact plane: Y is fixed, overlap is in (X, Z)
            int overlapXStart = Math.Max(paper.X, placement.X);
            int overlapXEnd = Math.Min(paper.AbsoluteEndX, placement.AbsoluteEndX);
            int overlapX = overlapXEnd - overlapXStart + 1;
            if (overlapX <= 0) return 0;

            int overlapZStart = Math.Max(paper.Z, placement.Z);
            int overlapZEnd = Math.Min(paper.AbsoluteEndZ, placement.AbsoluteEndZ);
            int overlapZ = overlapZEnd - overlapZStart + 1;
            if (overlapZ <= 0) return 0;

            return (long)overlapX * overlapZ;
        }

        private static long CalculateSideContactAreaXZ(PaddingPaper paper, PaddingPaper other)
        {
            int overlapXStart = Math.Max(paper.X, other.X);
            int overlapXEnd = Math.Min(paper.AbsoluteEndX, other.AbsoluteEndX);
            int overlapX = overlapXEnd - overlapXStart + 1;
            if (overlapX <= 0) return 0;

            int overlapZStart = Math.Max(paper.Z, other.Z);
            int overlapZEnd = Math.Min(paper.AbsoluteEndZ, other.AbsoluteEndZ);
            int overlapZ = overlapZEnd - overlapZStart + 1;
            if (overlapZ <= 0) return 0;

            return (long)overlapX * overlapZ;
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

        private void PrintPaddingInfo(List<PaddingPaper> paddingPapers, Action<string> log)
        {
            foreach (var paper in paddingPapers)
                Log(log, $"[牛皮纸] 位置({paper.X},{paper.Y},{paper.Z}) 尺寸({paper.Dx}x{paper.Dy}x{paper.Dz})");
        }

        private static void Log(Action<string> log, string msg)
        {
            if (log != null) log(msg);
            else Console.WriteLine(msg);
        }
    }
}
