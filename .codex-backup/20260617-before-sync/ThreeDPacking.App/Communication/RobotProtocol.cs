using System;
using System.Collections.Generic;
using System.Linq;
using ThreeDPacking.Core.Models;

namespace ThreeDPacking.App.Communication
{
    public static class RobotProtocol
    {
        public const string CmdLoadPositions = "DATA_PACK_POS";
        public const string CmdPickScan = "CMD_PICK_SCAN";
        public const string CmdPlaceHeld = "CMD_PLACE_HELD";
        public const string CmdBufferHeld = "CMD_BUFFER_HELD";
        public const string CmdTakeBuffer = "CMD_TAKE_BUFFER";
        public const string CmdDone = "CMD_DONE";
        public const string CmdAbort = "CMD_ABORT";

        public const string EvtRobotReady = "EVT_ROBOT_READY";
        public const string EvtScanReady = "EVT_SCAN_READY";
        public const string EvtActionDone = "EVT_ACTION_DONE";
        public const string EvtDone = "EVT_DONE";
        public const string EvtError = "EVT_ERROR";
        public const string EvtPhotoAtPose = "EVT_PHOTO_AT_POSE";
        public const string EvtVisionWait = "EVT_VISION_WAIT";

        public const string VisionOk = "VISION_OK";
        public const string VisionRotate = "VISION_ROTATE";
        public const string VisionFail = "VISION_FAIL";

        public static string GetAction(string line)
        {
            line = FirstLine(line);
            int split = line.IndexOf('|');
            return split < 0 ? line : line.Substring(0, split);
        }

        public static string FirstLine(string raw)
        {
            if (string.IsNullOrEmpty(raw))
                return string.Empty;

            string[] lines = raw.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            return lines.Length == 0 ? string.Empty : lines[0].Trim();
        }

        public static Dictionary<string, string> ParseFields(string line)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            line = FirstLine(line);
            if (line.Length == 0)
                return result;

            string[] parts = line.Split('|');
            for (int i = 1; i < parts.Length; i++)
            {
                int eq = parts[i].IndexOf('=');
                if (eq <= 0)
                    continue;

                string key = parts[i].Substring(0, eq).Trim();
                string value = parts[i].Substring(eq + 1).Trim();
                if (key.Length > 0)
                    result[key] = value;
            }

            return result;
        }

        public static string BuildLoadPositions(IEnumerable<ArmPackingStep> steps)
        {
            var list = steps.ToList();
            var parts = new List<string>
            {
                CmdLoadPositions,
                "count=" + list.Count
            };

            for (int i = 0; i < list.Count; i++)
            {
                ArmPackingStep step = list[i];
                string value = string.Join(",",
                    step.TargetX,
                    step.TargetY,
                    step.TargetZ,
                    step.Dx,
                    step.Dy,
                    step.Dz,
                    CleanToken(step.ItemId));
                parts.Add("p" + (i + 1) + "=" + value);
            }

            return string.Join("|", parts);
        }

        public static string BuildPickScan()
        {
            return CmdPickScan;
        }

        public static string BuildPlaceHeld(ArmPackingStep step, string poseMode)
        {
            return string.Join("|",
                CmdPlaceHeld,
                "item=" + CleanToken(step.ItemId),
                "place=" + step.PlaceIndex,
                "pose=" + CleanToken(poseMode));
        }

        public static string BuildBufferHeld(ArmPackingStep step, int slot)
        {
            return string.Join("|",
                CmdBufferHeld,
                "item=" + CleanToken(step.ItemId),
                "slot=" + slot);
        }

        public static string BuildTakeBuffer(ArmPackingStep step, int slot, string poseMode)
        {
            return string.Join("|",
                CmdTakeBuffer,
                "item=" + CleanToken(step.ItemId),
                "slot=" + slot,
                "place=" + step.PlaceIndex,
                "pose=" + CleanToken(poseMode));
        }

        public static string BuildAbort(string reason)
        {
            return CmdAbort + "|reason=" + CleanToken(reason);
        }

        public static string CleanToken(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value
                .Trim()
                .Replace("|", "_")
                .Replace("=", "_")
                .Replace(",", "_")
                .Replace("\r", "")
                .Replace("\n", "");
        }
    }

    public sealed class ArmPackingStep
    {
        public int PlaceIndex { get; set; }
        public int ContainerIndex { get; set; }
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public int TargetX { get; set; }
        public int TargetY { get; set; }
        public int TargetZ { get; set; }
        public int Dx { get; set; }
        public int Dy { get; set; }
        public int Dz { get; set; }

        public static ArmPackingStep FromPlacement(Placement placement, int placeIndex, int containerIndex)
        {
            string id = placement?.StackValue?.Box?.Id ?? string.Empty;
            string name = placement?.StackValue?.Box?.Description ?? id;

            int dx = placement?.StackValue?.Dx ?? 0;
            int dy = placement?.StackValue?.Dy ?? 0;
            int dz = placement?.StackValue?.Dz ?? 0;

            return new ArmPackingStep
            {
                PlaceIndex = placeIndex,
                ContainerIndex = containerIndex,
                ItemId = id,
                ItemName = name,
                TargetX = (placement?.X ?? 0) + dx / 2,
                TargetY = (placement?.Y ?? 0) + dy / 2,
                TargetZ = (placement?.Z ?? 0) + dz,
                Dx = dx,
                Dy = dy,
                Dz = dz
            };
        }

        public bool MatchesBarcode(string barcode)
        {
            string normalized = NormalizeBarcode(barcode);
            if (normalized.Length == 0)
                return false;

            return normalized == NormalizeBarcode(ItemId)
                || normalized == NormalizeBarcode(ItemName)
                || normalized == NormalizeBarcode(GetBaseCode(ItemId))
                || normalized == NormalizeBarcode(GetBaseCode(ItemName));
        }

        public string DisplayText =>
            $"{PlaceIndex}. {ItemId} -> ({TargetX},{TargetY},{TargetZ}) [{Dx}x{Dy}x{Dz}]";

        public override string ToString()
        {
            return DisplayText;
        }

        private static string GetBaseCode(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            int hash = value.IndexOf('#');
            return hash > 0 ? value.Substring(0, hash) : value;
        }

        private static string NormalizeBarcode(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToUpperInvariant();
        }
    }
}
