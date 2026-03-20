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
    public class LayerFillPaddingPaperPacker
    {
        // 基础支撑比例：与旧策略保持一致（再由“切分奖励”提升填充倾向）
        private const float MinSupportRatio = 0.01f;
        private const float MinSideSupportRatio = MinSupportRatio;

        // 与旧策略保持一致：避免因极值点 Dz 保守裁剪导致跳过上层可行候选
        private const int MinPointDzForPadding = 0;

        // 最小长度：用于控制牛皮纸沿可变方向的最短铺放长度（>=200mm）
        private const int MinPaddingLengthForPlacement = PaddingPaper.MinLength; // 200

        // 扫描长度的步进：避免在长区间上做太多可行性判断
        private const int LengthScanStep = 5;
        // 在接近最小长度时，用更细的步长补齐“最后一段刚好能拼上的长度”
        private const int FineScanTail = 20;

        private static (bool splitPossible, int remainderAlongVariable) GetSplitInfo(
            ExtremePoint point,
            PaddingPaper paper)
        {
            // A朝向：宽=110固定在Y方向，长度=Dx（X方向可变）
            // B朝向：宽=110固定在X方向，长度=Dy（Y方向可变）
            int remainder = 0;
            if (paper.Dy == PaddingPaper.DefaultWidth)
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
            {
                Log(log, $"\n[分层牛皮纸] 容器 {container.Id} Stack为空，跳过");
                return;
            }

            var itemPlacements = stack.Placements.Where(p => !p.IsPadding).ToList();
            var paddingPapers = new List<PaddingPaper>();

            Log(log, $"\n[分层牛皮纸] 容器: {container.Id} 尺寸: {container.LoadDx}x{container.LoadDy}x{container.LoadDz} 物品: {itemPlacements.Count}个");

            // 关键：尽量放宽点生成，实际可行性仍由碰撞与支撑兜底
            var pointCalc = new PointCalculator3D(0f);
            pointCalc.ClearToSize(container.LoadDx, container.LoadDy, container.LoadDz);
            RebuildPointsFromPlacements(pointCalc, itemPlacements, log);

            int maxIterations = 1000;
            int iteration = 0;

            while (iteration < maxIterations && !pointCalc.IsEmpty)
            {
                // 选出“下一层”的第一个牛皮纸：全局最小Z优先，同Z下体积最大
                PaddingPaper firstPaper = null;
                int firstPointIndex = -1;
                long firstBestScore = long.MinValue;
                int firstBestZ = int.MaxValue;
                bool firstBestSplitPossible = false;
                int firstBestRemainder = -1;
                int feasibleFirstCandidates = 0;

                int pointCount = pointCalc.PointCount;
                for (int i = 0; i < pointCount; i++)
                {
                    var point = pointCalc.GetPoint(i);
                    var paper = TryCreatePaddingAtExtremePoint(point, container, itemPlacements, paddingPapers, log, iteration: iteration, epIndex: i);
                    if (paper == null) continue;
                    feasibleFirstCandidates++;

                    var (splitPossible, remainderAlongVariable) = GetSplitInfo(point, paper);
                    long volume = paper.Volume;

                    bool isBetter = false;
                    if (paper.Z < firstBestZ)
                    {
                        isBetter = true;
                    }
                    else if (paper.Z == firstBestZ)
                    {
                        // 优先：可切分剩余（splitPossible）
                        if (splitPossible != firstBestSplitPossible)
                        {
                            isBetter = splitPossible;
                        }
                        else if (remainderAlongVariable != firstBestRemainder)
                        {
                            // 无论 splitPossible 与否，都偏好“剩余更大”，以改变后续极值点。
                            isBetter = remainderAlongVariable > firstBestRemainder;
                        }
                        else
                        {
                            // 最后：体积更大
                            isBetter = volume > firstBestScore;
                        }
                    }

                    if (isBetter)
                    {
                        firstBestZ = paper.Z;
                        firstBestScore = volume;
                        firstBestSplitPossible = splitPossible;
                        firstBestRemainder = remainderAlongVariable;
                        firstPaper = paper;
                        firstPointIndex = i;
                    }
                }

                if (firstPaper == null)
                {
                    Log(log, $"[分层牛皮纸] 停止: it={iteration} pointCount={pointCalc.PointCount} feasiblePaperCandidates={feasibleFirstCandidates} firstPaper=null");
                    if (log != null)
                    {
                        Log(log, "[分层牛皮纸] 停止时 EP 列表（用于定位为何无法继续）:");
                        for (int ei = 0; ei < pointCalc.PointCount; ei++)
                        {
                            var ep = pointCalc.GetPoint(ei);
                            Log(log, $"  EP{ei}: Min=({ep.MinX},{ep.MinY},{ep.MinZ}) Size=({ep.Dx}x{ep.Dy}x{ep.Dz}) zFits={(ep.MinZ + PaddingPaper.DefaultHeight <= container.LoadDz ? "Y" : "N")}");
                        }
                    }
                    break;
                }

                // 之后尽量把“该层Z=firstPaper.Z”填满（LAFF：同层持续放置，直到放不下再起新层）
                int levelZ = firstPaper.Z;
                iteration++;
                paddingPapers.Add(firstPaper);
                {
                    var placement = firstPaper.ToPlacement();
                    stack.Add(placement);
                    pointCalc.Add(firstPointIndex, placement);
                }
                Log(log, $"[分层牛皮纸] 放置: it={iteration} EP=({firstPaper.X},{firstPaper.Y},{firstPaper.Z}) 尺寸({firstPaper.Dx}x{firstPaper.Dy}x{firstPaper.Dz}) 体积={firstPaper.Volume} 可行候选={feasibleFirstCandidates}");

                // 内层：同一 levelZ 继续找可放置牛皮纸
                while (iteration < maxIterations && !pointCalc.IsEmpty)
                {
                    PaddingPaper bestPaperSameZ = null;
                    int bestPointIndexSameZ = -1;
                    long bestScoreSameZ = long.MinValue;
                    bool bestSplitPossibleSameZ = false;
                    int bestRemainderSameZ = -1;
                    int feasibleCandidatesSameZ = 0;

                    int pc = pointCalc.PointCount;
                    for (int i = 0; i < pc; i++)
                    {
                        var point = pointCalc.GetPoint(i);
                        var paper = TryCreatePaddingAtExtremePoint(point, container, itemPlacements, paddingPapers, log, iteration: iteration, epIndex: i);
                        if (paper == null) continue;
                        if (paper.Z != levelZ) continue;
                        feasibleCandidatesSameZ++;

                        var (splitPossible, remainderAlongVariable) = GetSplitInfo(point, paper);
                        long volume = paper.Volume;

                        bool isBetter = false;
                        if (bestPaperSameZ == null)
                        {
                            isBetter = true;
                        }
                        else if (splitPossible != bestSplitPossibleSameZ)
                        {
                            // 优先可切分
                            isBetter = splitPossible;
                        }
                        else if (remainderAlongVariable != bestRemainderSameZ)
                        {
                            // 偏好剩余更大（即便都不可切分，也会改变后续分裂形态）
                            isBetter = remainderAlongVariable > bestRemainderSameZ;
                        }
                        else
                        {
                            // 再用体积打破平局
                            isBetter = volume > bestScoreSameZ;
                        }

                        if (isBetter)
                        {
                            bestPaperSameZ = paper;
                            bestPointIndexSameZ = i;
                            bestScoreSameZ = volume;
                            bestSplitPossibleSameZ = splitPossible;
                            bestRemainderSameZ = remainderAlongVariable;
                        }
                    }

                    if (bestPaperSameZ == null)
                        break;

                    iteration++;
                    paddingPapers.Add(bestPaperSameZ);
                    var placement2 = bestPaperSameZ.ToPlacement();
                    stack.Add(placement2);
                    pointCalc.Add(bestPointIndexSameZ, placement2);

                    Log(log, $"[分层牛皮纸][同层] 放置: it={iteration} EP=({bestPaperSameZ.X},{bestPaperSameZ.Y},{bestPaperSameZ.Z}) 尺寸({bestPaperSameZ.Dx}x{bestPaperSameZ.Dy}x{bestPaperSameZ.Dz}) 体积={bestPaperSameZ.Volume} 同层可行候选={feasibleCandidatesSameZ}");
                }
            }

            PrintPaddingInfo(container, paddingPapers, log);
        }

        private void RebuildPointsFromPlacements(PointCalculator3D pointCalc, List<Placement> placements, Action<string> log)
        {
            // 关键：PointCalculator3D 的 Add/切割对“添加顺序”敏感。
            // 这里保持 placements 的原始放置顺序（来自 stack.Placements），避免重建出与真实剩余空间不一致的极值点。
            var ordered = placements
                .Where(p => p != null && p.StackValue != null)
                .ToList();

            int skipped = 0;
            foreach (var placement in ordered)
            {
                int pointIndex = FindContainingPointIndex(pointCalc, placement);
                if (pointIndex < 0)
                {
                    skipped++;
                    continue;
                }
                pointCalc.Add(pointIndex, placement);
            }

            if (skipped > 0 && log != null)
                Log(log, $"[分层牛皮纸] 重建极值点：跳过了 {skipped} 个物品（未找到包含该物品的极值点）");
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

        private PaddingPaper TryCreatePaddingAtExtremePoint(
            ExtremePoint point,
            Container container,
            List<Placement> itemPlacements,
            List<PaddingPaper> paddingPapers,
            Action<string> log,
            int iteration,
            int epIndex)
        {
            int x = point.MinX, y = point.MinY;
            int zStartMin = point.MinZ;
            bool debug = log != null && iteration >= 2;

            if (x < 0 || y < 0 || zStartMin < 0)
            {
                if (debug) Log(log, $"[分层牛皮纸][it={iteration}] EP{epIndex} 失败: 坐标为负");
                return null;
            }
            if (x >= container.LoadDx || y >= container.LoadDy)
            {
                if (debug) Log(log, $"[分层牛皮纸][it={iteration}] EP{epIndex} 失败: 越界 x/y (MaxLoad)");
                return null;
            }

            int paperHeight = PaddingPaper.DefaultHeight;
            // 思路3：在同一个 EP 内尝试不同的 z 起点（关键是让纸底避开下面已占用的高度段）
            // zStartMax：paperBottomZ + (paperHeight-1) <= point.MaxZ 且 <= container.LoadDz-1
            int zStartMaxByPoint = point.MaxZ - (paperHeight - 1);
            int zStartMaxByContainer = container.LoadDz - paperHeight;
            int zStartMax = Math.Min(zStartMaxByPoint, zStartMaxByContainer);

            if (zStartMin < 0 || zStartMax < zStartMin)
            {
                if (debug) Log(log, $"[分层牛皮纸][it={iteration}] EP{epIndex} 失败: z范围无效 (zMin={zStartMin}, zMax={zStartMax})");
                return null;
            }

            if (point.Dz < MinPointDzForPadding)
            {
                if (debug) Log(log, $"[分层牛皮纸][it={iteration}] EP{epIndex} 失败: point.Dz={point.Dz} 小于阈值");
                return null;
            }

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
            int bestZ = int.MaxValue;
            bool bestSplitPossible = false;
            int bestRemainder = -1;
            long bestVolume = long.MinValue;

            foreach (var zCandidate in zCandidates.OrderBy(z => z))
            {
                var paper = CreateBestFeasiblePadding(
                    x, y, zCandidate, maxDx, maxDy,
                    itemPlacements, paddingPapers,
                    log, debug, epIndex);

                if (paper == null) continue;

                var (splitPossible, remainderAlongVariable) = GetSplitInfo(point, paper);
                long volume = paper.Volume;

                bool isBetter = false;
                if (paper.Z < bestZ)
                {
                    isBetter = true;
                }
                else if (paper.Z == bestZ)
                {
                    if (splitPossible != bestSplitPossible)
                        isBetter = splitPossible;
                    else if (splitPossible && remainderAlongVariable != bestRemainder)
                        isBetter = remainderAlongVariable > bestRemainder;
                    else
                        isBetter = volume > bestVolume;
                }

                if (isBetter)
                {
                    best = paper;
                    bestZ = paper.Z;
                    bestSplitPossible = splitPossible;
                    bestRemainder = remainderAlongVariable;
                    bestVolume = volume;
                }
            }

            if (debug)
            {
                if (best == null)
                    Log(log, $"[分层牛皮纸][it={iteration}] EP{epIndex} 失败: 多 z候选均无解（通常是 collision/support 不可行）");
            }

            return best;
        }

        private PaddingPaper CreateBestFeasiblePadding(
            int x, int y, int z,
            int maxDx, int maxDy,
            List<Placement> itemPlacements,
            List<PaddingPaper> paddingPapers,
            Action<string> log,
            bool debug,
            int epIndex)
        {
            int dz = PaddingPaper.DefaultHeight;

            PaddingPaper best = null;
            long bestVolume = long.MinValue;
            bool bestSplitPossible = false;
            int bestRemainder = -1;
            float bestSupportRatio = -1f;

            // 朝向A：宽=110放在Y方向，长度=dx（X方向可变）
            if (maxDy >= PaddingPaper.DefaultWidth && maxDx >= MinPaddingLengthForPlacement)
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
                    var paper = new PaddingPaper(x, y, z, dx, PaddingPaper.DefaultWidth, dz);
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
                    else if (splitPossible != bestSplitPossible)
                    {
                        isBetter = splitPossible;
                    }
                    else if (remainderAlongVariable != bestRemainder)
                    {
                        isBetter = remainderAlongVariable > bestRemainder;
                    }
                    else if (volume != bestVolume)
                    {
                        isBetter = volume > bestVolume;
                    }
                    else
                    {
                        // 完全平局时，用支撑比例打破平局
                        isBetter = supportRatio > bestSupportRatio;
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
            if (maxDx >= PaddingPaper.DefaultWidth && maxDy >= MinPaddingLengthForPlacement)
            {
                var candidateLengths = new List<int>();
                for (int dy = maxDy; dy >= MinPaddingLengthForPlacement; dy -= LengthScanStep)
                    candidateLengths.Add(dy);

                int tailStart = Math.Min(maxDy, MinPaddingLengthForPlacement + FineScanTail);
                for (int dy = tailStart; dy >= MinPaddingLengthForPlacement; dy--)
                    candidateLengths.Add(dy);

                foreach (var dy in candidateLengths.Distinct().OrderByDescending(v => v))
                {
                    var paper = new PaddingPaper(x, y, z, PaddingPaper.DefaultWidth, dy, dz);
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
                    else if (splitPossible != bestSplitPossible)
                    {
                        isBetter = splitPossible;
                    }
                    else if (remainderAlongVariable != bestRemainder)
                    {
                        isBetter = remainderAlongVariable > bestRemainder;
                    }
                    else if (volume != bestVolume)
                    {
                        isBetter = volume > bestVolume;
                    }
                    else
                    {
                        isBetter = supportRatio > bestSupportRatio;
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

            if (debug && best == null && log != null)
            {
                Log(log,
                    $"[分层牛皮纸][it?][EP{epIndex}] CreateBest=null 说明：候选长度全部被 collision/support 拒绝（或不满足 MinPaddingLength）");
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

        private void PrintPaddingInfo(Container container, List<PaddingPaper> paddingPapers, Action<string> log)
        {
            Log(log, $"\n===== 分层牛皮纸填充信息 =====");
            Log(log, $"容器: {container.Id} ({container.LoadDx}x{container.LoadDy}x{container.LoadDz})");
            Log(log, $"填充纸数量: {paddingPapers.Count}");

            if (paddingPapers.Count > 0)
            {
                Console.WriteLine();
                long totalPaddingVolume = 0;
                foreach (var paper in paddingPapers)
                {
                    Log(log, $"  填充纸: 位置({paper.X}, {paper.Y}, {paper.Z}) 尺寸({paper.Dx} x {paper.Dy} x {paper.Dz})");
                    totalPaddingVolume += paper.Volume;
                }

                Log(log, $"\n填充纸总体积: {totalPaddingVolume}");
                Log(log, $"========================\n");
            }
            else
            {
                Log(log, $"没有生成任何填充纸\n========================\n");
            }
        }

        private static void Log(Action<string> log, string msg)
        {
            if (log != null) log(msg);
            else Console.WriteLine(msg);
        }
    }
}

