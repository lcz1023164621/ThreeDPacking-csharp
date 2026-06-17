using System;

namespace ThreeDPacking.App.Packing
{
    /// <summary>
    /// PC -> 机械臂动作命令，替代 6/7/8/9/10 这类数字标志位。
    /// </summary>
    public static class PackingCommandProtocol
    {
        public const string PickScan = "CMD_PICK_SCAN";
        public const string PlaceHeld = "CMD_PLACE_HELD";
        public const string BufferHeld = "CMD_BUFFER_HELD";
        public const string TakeBuffer = "CMD_TAKE_BUFFER";

        public const string PoseDirect = "DIRECT";
        public const string PoseSwapLongShort = "SWAP_LONG_SHORT";

        public static string BuildPickScan()
        {
            return PickScan;
        }

        public static string BuildPlaceHeld(PackingPlacementAction action)
        {
            return BuildWithPose(PlaceHeld, action);
        }

        public static string BuildBufferHeld(PackingPlacementAction action)
        {
            return BuildBase(BufferHeld, action);
        }

        public static string BuildTakeBuffer(PackingPlacementAction action)
        {
            return BuildWithPose(TakeBuffer, action);
        }

        public static string GetPose(PackingPlacementAction action)
        {
            return action != null && action.IsLongShortSwapped
                ? PoseSwapLongShort
                : PoseDirect;
        }

        private static string BuildWithPose(string command, PackingPlacementAction action)
        {
            return BuildBase(command, action) + "|pose=" + GetPose(action);
        }

        private static string BuildBase(string command, PackingPlacementAction action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            return command
                + "|seq=" + action.PackingSequence
                + "|box=" + CleanToken(action.PackingBoxId);
        }

        private static string CleanToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value.Trim()
                .Replace("|", "_")
                .Replace("=", "_")
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty);
        }
    }
}
