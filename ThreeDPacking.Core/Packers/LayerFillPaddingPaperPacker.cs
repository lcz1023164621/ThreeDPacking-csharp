using System;
using System.Collections.Generic;
using System.Linq;
using ThreeDPacking.Core.Models;
using ThreeDPacking.Core.Points;

namespace ThreeDPacking.Core.Packers
{
    /// <summary>
    /// 分层填充优先的牛皮纸填充器（相对 <see cref="PaddingPaperPacker"/> 的差异点）：
    /// 1) 外层仍使用极值点逐步放置，但同一层（同Z起点）的选择更强调“可切分”带来的持续填充能力。
    /// 2) 在同一极值点处，不再“第一个可行就 break”，而是扫描更多候选长度，
    ///    让选择更倾向于：留出 >= MinLength 的剩余长度，便于后续继续铺满。
    /// 3) 支撑判定复用 PaddingPaperPacker 的“底面支撑 + 侧面接触支撑”逻辑。
    /// </summary>
    public class LayerFillPaddingPaperPacker : IPaddingPaperPacker
    {
        // StableLayerFill 模式：支撑阈值更保守，优先稳定。
        private const float MinSupportRatio = 0.15f;
        private const float MinSideSupportRatio = 0.10f;

        // 与旧策略保持一致：避免因极值点 Dz 保守裁剪导致跳过上层可行候选
        private const int MinPointDzForPadding = 0;

        // 最小长度：用于控制牛皮纸沿可变方向的最短铺放长度（>=200mm）
        private const int MinPaddingLengthForPlacement = PaddingPaper.MinLength; // 200
        private readonly int _minPaddingWidth;

        // 扫描长度的步进：避免在长区间上做太多可行性判断
        private const int LengthScanStep = 5;
        // 在接近最小长度时，用更细的步长补齐“最后一段刚好能拼上的长度”
        private const int FineScanTail = 20;

        public LayerFillPaddingPaperPacker(int minPaddingWidth = PaddingPaper.DefaultWidth)
        {
            _minPaddingWidth = Math.Max(PaddingPaper.MinSize, minPaddingWidth);
        }

        private (bool splitPossible, int remainderAlongVariable) GetSplitInfo(
            ExtremePoint point,
            PaddingPaper paper)
        {
            // A朝向：宽=110固定在Y方向，长度=Dx（X方向可变）
            // B朝向：宽=110固定在X方向，长度=Dy（Y方向可变）
            int remainder = 0;
            if (paper.Dy == _minPaddingWidth)
            {
                remainder = point.Dx - paper.Dx;
            }
            else
            {
                remainder = point.Dy - paper.Dy;
            }

            bool splitPossible = remainder >= MinPaddingLengthForPlacement;
            return (splitPossible, remainder);
        }

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

            // 关键：尽量放宽点生成，实际可行性仍由碰撞与支撑兜底
            var pointCalc = new PointCalculator3D(0f);
            pointCalc.ClearToSize(container.LoadDx, container.LoadDy, container.LoadDz);
            RebuildPointsFromPlacements(pointCalc, itemPlacements);

            int maxIterations = 1000;
            int iteration = 0;

            while (iteration < maxIterations && !pointCalc.IsEmpty)
            {
                // 先做“落底优先”：底层还有可放空间时，先从地面铺起。
                var floorPaper = TryCreateBestFloorPadding(container, itemPlacements, paddingPapers, out _);
                if (floorPaper != null)
                {
                    iteration++;
                    paddingPapers.Add(floorPaper);
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
                    iteration++;
                    paddingPapers.Add(anchoredPaper);
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

                // 选出“下一层”的第一个牛皮纸：全局最小Z优先，同Z下体积最大
                PaddingPaper firstPaper = null;
                int firstPointIndex = -1;
                double firstBestScore = double.NegativeInfinity;
                int pointCount = pointCalc.PointCount;
                for (int i = 0; i < pointCount; i++)
                {
                    var point = pointCalc.GetPoint(i);
                    var paper = TryCreatePaddingAtExtremePoint(point, container, itemPlacements, paddingPapers);
                    if (paper == null) continue;

                    if (!TryGetSupportRatio(paper, itemPlacements, paddingPapers, out var supportRatio))
                        continue;

                    double score = EvaluateStableLayerFillScore(point, paper, supportRatio, container, itemPlacements, paddingPapers);
                    if (score > firstBestScore)
                    {
                        firstBestScore = score;
                        firstPaper = paper;
                        firstPointIndex = i;
                    }
                }

                if (firstPaper == null)
                    break;

                // 之后尽量把“该层Z=firstPaper.Z”填满（LAFF：同层持续放置，直到放不下再起新层）
                int levelZ = firstPaper.Z;
                iteration++;
                paddingPapers.Add(firstPaper);
                {
                    var placement = firstPaper.ToPlacement();
                    stack.Add(placement);
                    if (firstPointIndex >= 0)
                    {
                        pointCalc.Add(firstPointIndex, placement);
                    }
                    else
                    {
                        RebuildPointsFromCurrentState(pointCalc, container, itemPlacements, paddingPapers);
                    }
                }

                // 内层：同一 levelZ 继续找可放置牛皮纸
                while (iteration < maxIterations && !pointCalc.IsEmpty)
                {
                    PaddingPaper bestPaperSameZ = null;
                    int bestPointIndexSameZ = -1;
                    double bestScoreSameZ = double.NegativeInfinity;
                    int pc = pointCalc.PointCount;
                    for (int i = 0; i < pc; i++)
                    {
                        var point = pointCalc.GetPoint(i);
                        var paper = TryCreatePaddingAtExtremePoint(point, container, itemPlacements, paddingPapers);
                        if (paper == null) continue;
                        if (paper.Z != levelZ) continue;

                        if (!TryGetSupportRatio(paper, itemPlacements, paddingPapers, out var supportRatio))
                            continue;

                        double score = EvaluateStableLayerFillScore(point, paper, supportRatio, container, itemPlacements, paddingPapers);
                        if (score > bestScoreSameZ)
                        {
                            bestPaperSameZ = paper;
                            bestPointIndexSameZ = i;
                            bestScoreSameZ = score;
                        }
                    }

                    if (bestPaperSameZ == null)
                        break;

                    iteration++;
                    paddingPapers.Add(bestPaperSameZ);
                    var placement2 = bestPaperSameZ.ToPlacement();
                    stack.Add(placement2);
                    if (bestPointIndexSameZ >= 0)
                    {
                        pointCalc.Add(bestPointIndexSameZ, placement2);
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
                // 必须完整包含 placement 的体积（否则点重建会偏差）
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

                    bool isBetter = best == null
                        || supportRatio > bestSupportRatio
                        || (Math.Abs(supportRatio - bestSupportRatio) < 1e-6f && paper.Volume > bestVolume)
                        || (Math.Abs(supportRatio - bestSupportRatio) < 1e-6f && paper.Volume == bestVolume &&
                            (paper.X < best.X || (paper.X == best.X && paper.Y < best.Y)));

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

                        bool isBetter = best == null
                            || paper.Z < bestZ
                            || (paper.Z == bestZ && supportRatio > bestSupportRatio)
                            || (paper.Z == bestZ && Math.Abs(supportRatio - bestSupportRatio) < 1e-6f && paper.Volume > bestVolume)
                            || (paper.Z == bestZ && Math.Abs(supportRatio - bestSupportRatio) < 1e-6f && paper.Volume == bestVolume &&
                                (paper.X < best.X || (paper.X == best.X && paper.Y < best.Y)));

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

            int paperHeight = PaddingPaper.DefaultHeight;
            // 思路3：在同一个 EP 内尝试不同的 z 起点（关键是让纸底避开下面已占用的高度段）
            // zStartMax：paperBottomZ + (paperHeight-1) <= point.MaxZ 且 <= container.LoadDz-1
            int zStartMaxByPoint = point.MaxZ - (paperHeight - 1);
            int zStartMaxByContainer = container.LoadDz - paperHeight;
            int zStartMax = Math.Min(zStartMaxByPoint, zStartMaxByContainer);

            if (zStartMin < 0 || zStartMax < zStartMin)
                return null;

            if (point.Dz < MinPointDzForPadding)
                return null;

            // 思路2：严格用 EP 给出的剩余矩形范围做上限
            // 这样避免把“其实被障碍占用的区域”也纳入扫描候选，导致所有候选最后全被 collision/支撑否决。
            int maxDx = point.Dx;
            int maxDy = point.Dy;
            if (maxDx < PaddingPaper.MinSize && maxDy < PaddingPaper.MinSize) return null;

            // 构造 z 起点候选：优先用 zMin，其次尝试“最近障碍顶部+1”
            // 这样可大幅提升找到可放置位置的概率（例如你日志里的 EP2：需要从 70 上移到 80）。
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

            // Z 越小越优先（与外层逻辑保持一致）
            PaddingPaper best = null;
            double bestScore = double.NegativeInfinity;

            foreach (var zCandidate in zCandidates.OrderBy(z => z))
            {
                var paper = CreateBestFeasiblePadding(
                    x, y, zCandidate, maxDx, maxDy,
                    itemPlacements, paddingPapers);

                if (paper == null) continue;

                if (!TryGetSupportRatio(paper, itemPlacements, paddingPapers, out var supportRatio))
                    continue;
                double score = EvaluateStableLayerFillScore(point, paper, supportRatio, container, itemPlacements, paddingPapers);
                if (score > bestScore)
                {
                    best = paper;
                    bestScore = score;
                }
            }

            return best;
        }

        private PaddingPaper CreateBestFeasiblePadding(
            int x, int y, int z,
            int maxDx, int maxDy,
            List<Placement> itemPlacements,
            List<PaddingPaper> paddingPapers)
        {
            int dz = PaddingPaper.DefaultHeight;

            PaddingPaper best = null;
            long bestVolume = long.MinValue;
            bool bestSplitPossible = false;
            int bestRemainder = -1;
            float bestSupportRatio = -1f;

            // 朝向A：宽=110放在Y方向，长度=dx（X方向可变）
            if (maxDy >= _minPaddingWidth && maxDx >= MinPaddingLengthForPlacement)
            {
                // 生成“更多候选长度”，而不是找到第一个可行就 break。
                // 这样才能真正把“切分/铺满能力”引入到决策里。
                var candidateLengths = new List<int>();
                for (int dx = maxDx; dx >= MinPaddingLengthForPlacement; dx -= LengthScanStep)
                    candidateLengths.Add(dx);

                int tailStart = Math.Min(maxDx, MinPaddingLengthForPlacement + FineScanTail);
                for (int dx = tailStart; dx >= MinPaddingLengthForPlacement; dx--)
                    candidateLengths.Add(dx);

                // 排序 + 去重，保证遍历顺序确定性
                foreach (var dx in candidateLengths.Distinct().OrderByDescending(v => v))
                {
                    var paper = new PaddingPaper(x, y, z, dx, _minPaddingWidth, dz);
                    if (HasCollisionWithItems(paper, itemPlacements) || HasCollisionWithPadding(paper, paddingPapers))
                        continue;

                    if (!TryGetSupportRatio(paper, itemPlacements, paddingPapers, out var supportRatio))
                        continue;

                    int remainderAlongVariable = maxDx - dx;
                    bool splitPossible = remainderAlongVariable >= MinPaddingLengthForPlacement;

                    long volume = paper.Volume;
                    bool isBetter = false;
                    if (best == null)
                    {
                        isBetter = true;
                    }
                    else if (volume != bestVolume)
                    {
                        // 主目标：同一可行位置里优先更大体积（即更长）
                        isBetter = volume > bestVolume;
                    }
                    else if (supportRatio != bestSupportRatio)
                    {
                        // 体积相同则稳定性更强优先
                        isBetter = supportRatio > bestSupportRatio;
                    }
                    else if (splitPossible != bestSplitPossible)
                    {
                        isBetter = splitPossible;
                    }
                    else
                    {
                        isBetter = remainderAlongVariable > bestRemainder;
                    }

                    if (isBetter)
                    {
                        best = paper;
                        bestVolume = volume;
                        bestSplitPossible = splitPossible;
                        bestRemainder = remainderAlongVariable;
                        bestSupportRatio = supportRatio;
                    }
                }
            }

            // 朝向B：宽=110放在X方向，长度=dy（Y方向可变）
            if (maxDx >= _minPaddingWidth && maxDy >= MinPaddingLengthForPlacement)
            {
                var candidateLengths = new List<int>();
                for (int dy = maxDy; dy >= MinPaddingLengthForPlacement; dy -= LengthScanStep)
                    candidateLengths.Add(dy);

                int tailStart = Math.Min(maxDy, MinPaddingLengthForPlacement + FineScanTail);
                for (int dy = tailStart; dy >= MinPaddingLengthForPlacement; dy--)
                    candidateLengths.Add(dy);

                foreach (var dy in candidateLengths.Distinct().OrderByDescending(v => v))
                {
                    var paper = new PaddingPaper(x, y, z, _minPaddingWidth, dy, dz);
                    if (HasCollisionWithItems(paper, itemPlacements) || HasCollisionWithPadding(paper, paddingPapers))
                        continue;

                    if (!TryGetSupportRatio(paper, itemPlacements, paddingPapers, out var supportRatio))
                        continue;

                    int remainderAlongVariable = maxDy - dy;
                    bool splitPossible = remainderAlongVariable >= MinPaddingLengthForPlacement;

                    long volume = paper.Volume;
                    bool isBetter = false;
                    if (best == null)
                    {
                        isBetter = true;
                    }
                    else if (volume != bestVolume)
                    {
                        // 主目标：同一可行位置里优先更大体积（即更长）
                        isBetter = volume > bestVolume;
                    }
                    else if (supportRatio != bestSupportRatio)
                    {
                        isBetter = supportRatio > bestSupportRatio;
                    }
                    else if (splitPossible != bestSplitPossible)
                    {
                        isBetter = splitPossible;
                    }
                    else
                    {
                        isBetter = remainderAlongVariable > bestRemainder;
                    }

                    if (isBetter)
                    {
                        best = paper;
                        bestVolume = volume;
                        bestSplitPossible = splitPossible;
                        bestRemainder = remainderAlongVariable;
                        bestSupportRatio = supportRatio;
                    }
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

            // 底面正下方支撑（旧逻辑）
            // 关键改动：允许“悬空/间隙支撑”，取纸底下方最近支撑高度的投影重叠面积
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
            if (bottomSupportRatio >= MinSupportRatio)
            {
                supportRatio = bottomSupportRatio;
                return true;
            }

            // 底面不足时允许侧面接触支撑（左/右或前/后）
            long sideSupportAreaX = 0;
            long sideFaceAreaX = (long)paper.Dy * paper.Dz;
            if (sideFaceAreaX > 0)
            {
                foreach (var item in itemPlacements)
                {
                    if (item == null) continue;
                    if (item.AbsoluteEndX + 1 == paper.X)
                        sideSupportAreaX += CalculateSideContactAreaYZ(paper, item);
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

            long sideSupportAreaY = 0;
            long sideFaceAreaY = (long)paper.Dx * paper.Dz;
            if (sideFaceAreaY > 0)
            {
                foreach (var item in itemPlacements)
                {
                    if (item == null) continue;
                    if (item.AbsoluteEndY + 1 == paper.Y)
                        sideSupportAreaY += CalculateSideContactAreaXZ(paper, item);
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

            if (sideSupportRatioX >= MinSideSupportRatio || sideSupportRatioY >= MinSideSupportRatio)
            {
                supportRatio = Math.Max(bottomSupportRatio, Math.Max(sideSupportRatioX, sideSupportRatioY));
                return true;
            }

            return false;
        }

        private static long CalculateSideContactAreaYZ(PaddingPaper paper, Placement placement)
        {
            int overlapYStart = Math.Max(paper.Y, placement.Y);
            int overlapYEnd = Math.Min(paper.AbsoluteEndY, placement.AbsoluteEndY);
            int overlapY = overlapYEnd - overlapYStart + 1;
            if (overlapY <= 0) return 0;

            int overlapZStart = Math.Max(paper.Z, placement.Z);
            int overlapZEnd = Math.Min(paper.AbsoluteEndZ, placement.AbsoluteEndZ);
            int overlapZ = overlapZEnd - overlapZStart + 1;
            if (overlapZ <= 0) return 0;

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

        private double EvaluateStableLayerFillScore(
            ExtremePoint point,
            PaddingPaper paper,
            float supportRatio,
            Container container,
            List<Placement> itemPlacements,
            List<PaddingPaper> paddingPapers)
        {
            float sideSupportRatio = EvaluateSideSupportRatio(paper, itemPlacements, paddingPapers);
            double continuityScore = EvaluateLayerContinuityScore(point, paper, container, itemPlacements, paddingPapers);
            double fragmentPenalty = EvaluateFragmentPenalty(point, paper);
            double volumeScore = paper.Volume / 10000.0; // 降低体积权重，避免压过稳定性目标

            // 优先级：低层 > 底面支撑 > 侧面接触 > 层连续性 > 体积
            return (-paper.Z * 1_000_000.0)
                   + (supportRatio * 100_000.0)
                   + (sideSupportRatio * 20_000.0)
                   + (continuityScore * 2_000.0)
                   - (fragmentPenalty * 1_000.0)
                   + volumeScore;
        }

        private double EvaluateLayerContinuityScore(
            ExtremePoint point,
            PaddingPaper paper,
            Container container,
            List<Placement> itemPlacements,
            List<PaddingPaper> paddingPapers)
        {
            var splitInfo = GetSplitInfo(point, paper);
            int variableCap = paper.Dy == _minPaddingWidth ? point.Dx : point.Dy;
            int remainder = splitInfo.remainderAlongVariable;

            double canContinue = splitInfo.splitPossible ? 1.0 : 0.0;
            double nearPerfect = remainder <= 0 ? 1.0 : 1.0 / (1.0 + remainder / 100.0);
            double compactness = EvaluateCompactnessAtLayer(paper, container, itemPlacements, paddingPapers);

            return canContinue * 2.0 + nearPerfect + compactness + variableCap / 1000.0;
        }

        private double EvaluateFragmentPenalty(ExtremePoint point, PaddingPaper paper)
        {
            int remainder = GetSplitInfo(point, paper).remainderAlongVariable;
            if (remainder <= 0 || remainder >= MinPaddingLengthForPlacement)
            {
                return 0.0;
            }

            // 对“小于最小可放长度”的碎缝给予惩罚，越接近 MinLength/2 惩罚越高。
            double ratio = (double)remainder / MinPaddingLengthForPlacement;
            return 1.0 + (1.0 - Math.Abs(ratio - 0.5));
        }

        private float EvaluateSideSupportRatio(PaddingPaper paper, List<Placement> itemPlacements, List<PaddingPaper> paddingPapers)
        {
            long sideSupportAreaX = 0;
            long sideFaceAreaX = (long)paper.Dy * paper.Dz;
            long sideSupportAreaY = 0;
            long sideFaceAreaY = (long)paper.Dx * paper.Dz;

            foreach (var item in itemPlacements)
            {
                if (item == null) continue;
                if (item.AbsoluteEndX + 1 == paper.X || paper.AbsoluteEndX + 1 == item.X)
                    sideSupportAreaX += CalculateSideContactAreaYZ(paper, item);
                if (item.AbsoluteEndY + 1 == paper.Y || paper.AbsoluteEndY + 1 == item.Y)
                    sideSupportAreaY += CalculateSideContactAreaXZ(paper, item);
            }

            foreach (var existingPaper in paddingPapers)
            {
                if (existingPaper == null) continue;
                if (existingPaper.AbsoluteEndX + 1 == paper.X || paper.AbsoluteEndX + 1 == existingPaper.X)
                    sideSupportAreaX += CalculateSideContactAreaYZ(paper, existingPaper);
                if (existingPaper.AbsoluteEndY + 1 == paper.Y || paper.AbsoluteEndY + 1 == existingPaper.Y)
                    sideSupportAreaY += CalculateSideContactAreaXZ(paper, existingPaper);
            }

            float ratioX = sideFaceAreaX > 0 ? (float)sideSupportAreaX / sideFaceAreaX : 0f;
            float ratioY = sideFaceAreaY > 0 ? (float)sideSupportAreaY / sideFaceAreaY : 0f;
            return Math.Min(1f, Math.Max(ratioX, ratioY));
        }

        private double EvaluateCompactnessAtLayer(
            PaddingPaper paper,
            Container container,
            List<Placement> itemPlacements,
            List<PaddingPaper> paddingPapers)
        {
            int z = paper.Z;
            int layerTop = z + PaddingPaper.DefaultHeight - 1;
            int occupiedBoundary = 0;

            foreach (var item in itemPlacements)
            {
                if (item == null) continue;
                if (item.Z > layerTop || item.AbsoluteEndZ < z) continue;
                occupiedBoundary += Math.Max(item.StackValue.Dx, item.StackValue.Dy);
            }

            foreach (var existingPaper in paddingPapers)
            {
                if (existingPaper == null) continue;
                if (existingPaper.Z > layerTop || existingPaper.AbsoluteEndZ < z) continue;
                occupiedBoundary += Math.Max(existingPaper.Dx, existingPaper.Dy);
            }

            int reference = Math.Max(1, container.LoadDx + container.LoadDy);
            return Math.Min(2.0, (double)occupiedBoundary / reference);
        }
    }
}

