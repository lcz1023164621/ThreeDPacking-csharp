using System;
using System.Collections.Generic;
using WindowsFormsApp1;

namespace ThreeDPacking.App.Packing
{
    public enum PackingPlacementActionType
    {
        DirectPack,
        StoreToBuffer,
        PackFromBuffer,
        ContinuePick
    }

    public sealed class PackingPlacementAction
    {
        public PackingPlacementActionType Type { get; set; }
        public int PackingSequence { get; set; }
        public string PackingBoxId { get; set; }
        public bool IsLongShortSwapped { get; set; }
    }

    public sealed class BufferSlotState
    {
        public int PackingSequence { get; set; }
        public string PackingBoxId { get; set; }
        public bool IsLongShortSwapped { get; set; }
        public bool IsOccupied { get; set; }
    }

    public sealed class PackingFlowDecision
    {
        public List<PackingPlacementAction> Actions { get; } = new List<PackingPlacementAction>();
        public string PlacementMode { get; set; }
        public string Message { get; set; }
        public bool HasError { get; set; }
    }

    /// <summary>
    /// 按装箱顺序协调直装、暂存与回取动作链。
    /// </summary>
    public sealed class PackingFlowCoordinator
    {
        private int _nextExpectedPackingSequence = 1;
        private readonly Dictionary<int, BufferSlotState> _bufferSlots =
            new Dictionary<int, BufferSlotState>();

        public int NextExpectedPackingSequence => _nextExpectedPackingSequence;

        public int BufferedItemCount => _bufferSlots.Count;

        public IReadOnlyDictionary<int, BufferSlotState> BufferSlots => _bufferSlots;

        public void Reset()
        {
            _nextExpectedPackingSequence = 1;
            _bufferSlots.Clear();
        }

        public PackingFlowDecision Decide(CommittedProductionRecord record)
        {
            var decision = new PackingFlowDecision();
            if (record == null)
            {
                decision.HasError = true;
                decision.Message = "扫码记录为空，跳过装箱动作。";
                return decision;
            }

            if (record.PackingSequence <= 0)
            {
                decision.HasError = true;
                decision.Message = "未找到本件装箱顺序，跳过动作信号。";
                return decision;
            }

            int sequence = record.PackingSequence;
            if (sequence < _nextExpectedPackingSequence)
            {
                decision.HasError = true;
                decision.Message = "扫码顺序 " + sequence + " 早于当前应装顺序 "
                    + _nextExpectedPackingSequence + "，不发动作信号。";
                return decision;
            }

            if (sequence == _nextExpectedPackingSequence)
            {
                decision.PlacementMode = PlacementModes.Direct;
                decision.Actions.Add(CreateDirectPack(record));
                _nextExpectedPackingSequence++;

                while (_bufferSlots.ContainsKey(_nextExpectedPackingSequence))
                {
                    BufferSlotState slot = _bufferSlots[_nextExpectedPackingSequence];
                    decision.Actions.Add(CreatePackFromBuffer(slot));
                    _bufferSlots.Remove(_nextExpectedPackingSequence);
                    _nextExpectedPackingSequence++;
                    decision.PlacementMode = PlacementModes.RetrievedFromBuffer;
                }
            }
            else if (sequence > _nextExpectedPackingSequence)
            {
                if (_bufferSlots.ContainsKey(sequence))
                {
                    decision.HasError = true;
                    decision.Message = "暂存槽位 " + sequence + " 已被占用，不发动作信号。";
                    return decision;
                }

                decision.PlacementMode = PlacementModes.Buffered;
                _bufferSlots[sequence] = new BufferSlotState
                {
                    PackingSequence = sequence,
                    PackingBoxId = record.PackingBoxId ?? string.Empty,
                    IsLongShortSwapped = record.IsPackingLongShortSwapped,
                    IsOccupied = true
                };
                decision.Actions.Add(CreateStoreToBuffer(record));
            }

            decision.Actions.Add(new PackingPlacementAction
            {
                Type = PackingPlacementActionType.ContinuePick
            });
            return decision;
        }

        private static PackingPlacementAction CreateDirectPack(CommittedProductionRecord record)
        {
            return new PackingPlacementAction
            {
                Type = PackingPlacementActionType.DirectPack,
                PackingSequence = record.PackingSequence,
                PackingBoxId = record.PackingBoxId ?? string.Empty,
                IsLongShortSwapped = record.IsPackingLongShortSwapped
            };
        }

        private static PackingPlacementAction CreateStoreToBuffer(CommittedProductionRecord record)
        {
            return new PackingPlacementAction
            {
                Type = PackingPlacementActionType.StoreToBuffer,
                PackingSequence = record.PackingSequence,
                PackingBoxId = record.PackingBoxId ?? string.Empty,
                IsLongShortSwapped = record.IsPackingLongShortSwapped
            };
        }

        private static PackingPlacementAction CreatePackFromBuffer(BufferSlotState slot)
        {
            return new PackingPlacementAction
            {
                Type = PackingPlacementActionType.PackFromBuffer,
                PackingSequence = slot.PackingSequence,
                PackingBoxId = slot.PackingBoxId ?? string.Empty,
                IsLongShortSwapped = slot.IsLongShortSwapped
            };
        }
    }

    public static class PlacementModes
    {
        public const string Direct = "Direct";
        public const string Buffered = "Buffered";
        public const string RetrievedFromBuffer = "RetrievedFromBuffer";
    }
}
