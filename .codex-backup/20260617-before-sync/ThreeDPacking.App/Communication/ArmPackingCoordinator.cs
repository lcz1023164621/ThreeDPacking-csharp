using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ThreeDPacking.Core.Models;

namespace ThreeDPacking.App.Communication
{
    public sealed class ArmPackingCoordinator
    {
        private readonly RoboticArmClient _client;
        private readonly Action<string> _log;
        private readonly List<ArmPackingStep> _plan = new List<ArmPackingStep>();
        private readonly Dictionary<int, BufferedStep> _bufferByPlace = new Dictionary<int, BufferedStep>();

        private PendingCommandKind _pendingCommand = PendingCommandKind.None;
        private ArmPackingStep _pendingStep;
        private int _nextPlanIndex;
        private int _nextBufferSlot;
        private bool _planSent;

        public ArmPackingCoordinator(RoboticArmClient client, Action<string> log)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _log = log ?? (_ => { });
            PoseMode = "BOX_ORIGIN";
        }

        public IReadOnlyList<ArmPackingStep> Plan => _plan;
        public bool IsRunning { get; private set; }
        public bool IsWaitingForScan { get; private set; }
        public string PoseMode { get; set; }

        public void LoadPlan(IEnumerable<Container> containers)
        {
            _plan.Clear();
            _bufferByPlace.Clear();
            _pendingCommand = PendingCommandKind.None;
            _pendingStep = null;
            _nextPlanIndex = 0;
            _nextBufferSlot = 1;
            _planSent = false;
            IsWaitingForScan = false;
            IsRunning = false;

            int placeIndex = 1;
            int containerIndex = 1;
            foreach (var container in containers ?? Enumerable.Empty<Container>())
            {
                if (container?.Stack?.Placements == null)
                    continue;

                var placements = container.Stack.Placements
                    .Where(p => p != null && !p.IsPadding && p.StackValue != null)
                    .OrderBy(p => p.Z)
                    .ThenBy(p => p.X)
                    .ThenBy(p => p.Y)
                    .ToList();

                foreach (var placement in placements)
                {
                    _plan.Add(ArmPackingStep.FromPlacement(placement, placeIndex, containerIndex));
                    placeIndex++;
                }

                containerIndex++;
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (!_client.IsConnected)
                throw new InvalidOperationException("请先连接机械臂 Socket。");
            if (_plan.Count == 0)
                throw new InvalidOperationException("没有可执行的装箱计划。请先运行算法装箱。");

            _bufferByPlace.Clear();
            _pendingCommand = PendingCommandKind.None;
            _pendingStep = null;
            _nextPlanIndex = 0;
            _nextBufferSlot = 1;
            IsRunning = true;
            IsWaitingForScan = false;

            await SendPlanAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task AbortAsync(CancellationToken cancellationToken = default)
        {
            IsRunning = false;
            IsWaitingForScan = false;
            _pendingCommand = PendingCommandKind.None;
            _pendingStep = null;

            if (_client.IsConnected)
                await _client.SendCommandAsync(RobotProtocol.BuildAbort("operator_abort"), cancellationToken)
                    .ConfigureAwait(false);
        }

        public async Task SendVisionResultAsync(string result, CancellationToken cancellationToken = default)
        {
            if (!_client.IsConnected)
                throw new InvalidOperationException("机械臂未连接。");

            await _client.SendCommandAsync(result, cancellationToken).ConfigureAwait(false);
        }

        public async Task SubmitBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
        {
            barcode = (barcode ?? string.Empty).Trim();
            if (barcode.Length == 0)
                throw new InvalidOperationException("扫码内容为空。");
            if (!IsRunning)
                throw new InvalidOperationException("实际装箱流程尚未启动。");
            if (!IsWaitingForScan)
                throw new InvalidOperationException("机械臂当前没有处于扫码等待状态。");

            ArmPackingStep matched = FindNextUnfinishedStep(barcode);
            if (matched == null)
                throw new InvalidOperationException("扫码结果未匹配装箱计划中的物品: " + barcode);

            IsWaitingForScan = false;

            ArmPackingStep expected = GetExpectedStep();
            if (expected != null && matched.PlaceIndex == expected.PlaceIndex)
            {
                await SendPlaceHeldAsync(matched, cancellationToken).ConfigureAwait(false);
                return;
            }

            int slot = _nextBufferSlot++;
            _bufferByPlace[matched.PlaceIndex] = new BufferedStep(matched, slot, barcode);
            await _client.SendCommandAsync(RobotProtocol.BuildBufferHeld(matched, slot), cancellationToken)
                .ConfigureAwait(false);
            _pendingCommand = PendingCommandKind.BufferHeld;
            _pendingStep = matched;
            _log($"[调度] 扫码 {barcode} 对应 {matched.ItemId}，还没轮到，暂存到 slot={slot}。");
        }

        public async Task HandleRobotLineAsync(string line, CancellationToken cancellationToken = default)
        {
            string action = RobotProtocol.GetAction(line);
            if (string.IsNullOrEmpty(action))
                return;

            switch (action)
            {
                case RobotProtocol.EvtRobotReady:
                    _log("[机械臂] 已就绪。");
                    break;

                case RobotProtocol.EvtScanReady:
                    _pendingCommand = PendingCommandKind.None;
                    _pendingStep = null;
                    IsWaitingForScan = true;
                    _log("[机械臂] 已到扫码位，请扫码。");
                    break;

                case RobotProtocol.EvtActionDone:
                    await CompletePendingActionAsync(cancellationToken).ConfigureAwait(false);
                    break;

                case RobotProtocol.EvtDone:
                    IsRunning = false;
                    IsWaitingForScan = false;
                    _pendingCommand = PendingCommandKind.None;
                    _pendingStep = null;
                    _log("[机械臂] 实际装箱流程结束。");
                    break;

                case RobotProtocol.EvtPhotoAtPose:
                case RobotProtocol.EvtVisionWait:
                    _log("[视觉] 机械臂等待视觉结果，可发送 VISION_OK / VISION_ROTATE / VISION_FAIL。");
                    break;

                case RobotProtocol.EvtError:
                    IsRunning = false;
                    IsWaitingForScan = false;
                    _log("[机械臂错误] " + line);
                    break;

                default:
                    _log("[机械臂事件] " + line);
                    break;
            }
        }

        private async Task SendPlanAsync(CancellationToken cancellationToken)
        {
            string planLine = RobotProtocol.BuildLoadPositions(_plan);
            await _client.SendCommandAsync(planLine, cancellationToken).ConfigureAwait(false);
            _planSent = true;
            _pendingCommand = PendingCommandKind.LoadPlan;
            _log($"[TX] 已发送装箱坐标，共 {_plan.Count} 个放置位。");
        }

        private async Task CompletePendingActionAsync(CancellationToken cancellationToken)
        {
            if (!IsRunning)
                return;

            if (_pendingCommand == PendingCommandKind.LoadPlan)
            {
                _log("[调度] 机械臂已接收装箱坐标。");
            }
            else if (_pendingCommand == PendingCommandKind.PlaceHeld ||
                _pendingCommand == PendingCommandKind.TakeBuffer)
            {
                if (_pendingStep != null && _pendingStep.PlaceIndex == _nextPlanIndex + 1)
                    _nextPlanIndex++;
                else
                    _nextPlanIndex = Math.Max(_nextPlanIndex + 1, _pendingStep?.PlaceIndex ?? _nextPlanIndex);

                _log($"[调度] 已完成 place={_nextPlanIndex}。");
            }
            else if (_pendingCommand == PendingCommandKind.BufferHeld)
            {
                _log("[调度] 当前物体已暂存。");
            }

            _pendingCommand = PendingCommandKind.None;
            _pendingStep = null;

            await SendNextCommandAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task SendNextCommandAsync(CancellationToken cancellationToken)
        {
            if (_nextPlanIndex >= _plan.Count)
            {
                await _client.SendCommandAsync(RobotProtocol.CmdDone, cancellationToken).ConfigureAwait(false);
                _pendingCommand = PendingCommandKind.Done;
                IsRunning = false;
                _log("[调度] 所有物品已完成，发送 CMD_DONE。");
                return;
            }

            ArmPackingStep expected = GetExpectedStep();
            if (expected != null && _bufferByPlace.TryGetValue(expected.PlaceIndex, out BufferedStep buffered))
            {
                _bufferByPlace.Remove(expected.PlaceIndex);
                await _client.SendCommandAsync(
                        RobotProtocol.BuildTakeBuffer(expected, buffered.Slot, PoseMode),
                        cancellationToken)
                    .ConfigureAwait(false);

                _pendingCommand = PendingCommandKind.TakeBuffer;
                _pendingStep = expected;
                _log($"[调度] 轮到 {expected.ItemId}，从暂存 slot={buffered.Slot} 取回并装箱。");
                return;
            }

            await SendPickScanAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task SendPickScanAsync(CancellationToken cancellationToken)
        {
            if (!_planSent)
                await SendPlanAsync(cancellationToken).ConfigureAwait(false);

            await _client.SendCommandAsync(RobotProtocol.BuildPickScan(), cancellationToken).ConfigureAwait(false);
            _pendingCommand = PendingCommandKind.PickScan;
            _pendingStep = null;
            IsWaitingForScan = false;
            _log("[调度] 发送 CMD_PICK_SCAN，等待机械臂抓取并到扫码位。");
        }

        private async Task SendPlaceHeldAsync(ArmPackingStep step, CancellationToken cancellationToken)
        {
            await _client.SendCommandAsync(RobotProtocol.BuildPlaceHeld(step, PoseMode), cancellationToken)
                .ConfigureAwait(false);
            _pendingCommand = PendingCommandKind.PlaceHeld;
            _pendingStep = step;
            _log($"[调度] 扫码匹配当前放置位，发送装箱命令：{step.ItemId} -> place={step.PlaceIndex}。");
        }

        private ArmPackingStep GetExpectedStep()
        {
            return _nextPlanIndex >= 0 && _nextPlanIndex < _plan.Count
                ? _plan[_nextPlanIndex]
                : null;
        }

        private ArmPackingStep FindNextUnfinishedStep(string barcode)
        {
            for (int i = _nextPlanIndex; i < _plan.Count; i++)
            {
                ArmPackingStep step = _plan[i];
                if (_bufferByPlace.ContainsKey(step.PlaceIndex))
                    continue;
                if (step.MatchesBarcode(barcode))
                    return step;
            }

            return null;
        }

        private enum PendingCommandKind
        {
            None,
            LoadPlan,
            PickScan,
            PlaceHeld,
            BufferHeld,
            TakeBuffer,
            Done
        }

        private sealed class BufferedStep
        {
            public BufferedStep(ArmPackingStep step, int slot, string barcode)
            {
                Step = step;
                Slot = slot;
                Barcode = barcode;
            }

            public ArmPackingStep Step { get; }
            public int Slot { get; }
            public string Barcode { get; }
        }
    }
}
