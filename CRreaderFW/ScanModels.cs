using System;

namespace WindowsFormsApp1
{
    internal sealed class OrderIndexEntry
    {
        public string OrderNo { get; set; }
        public string BoxCode { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string Status { get; set; }
        public string StatusDetail { get; set; }
    }

    public sealed class ScannedItemInfo
    {
        public string Name { get; set; }
        public string Dimensions { get; set; }
        public string Barcode { get; set; }
        public string Sku { get; set; }
        public int ScanSequence { get; set; }
        public int PackingSequence { get; set; }
        public string PackingBoxId { get; set; }
        public bool IsPackingLongShortSwapped { get; set; }
        public int Dx { get; set; }
        public int Dy { get; set; }
        public int Dz { get; set; }
        public string BufferPickCoordinate { get; set; }
        public string BufferDropCoordinate { get; set; }
        public string PlacementMode { get; set; }
    }

    public sealed class ScannerSettingsData
    {
        public string ReaderIp { get; set; }
        public int ReaderPort { get; set; }
        public int DeviceIndex { get; set; }
        public int ScanIntervalMs { get; set; }
        public bool AutoFocus { get; set; }
        public bool ExposureAuto { get; set; }
        public bool GainAuto { get; set; }
        public bool AutoReconnect { get; set; }
        public bool SaveRawImage { get; set; }
        public string ImageSavePath { get; set; }
        public string LightMode { get; set; }
        public float ExposureTimeUs { get; set; }
        public float GainDb { get; set; }
        public float AcquisitionFrameRate { get; set; }
        public bool UseAutoPacketSize { get; set; }
        public int GevSCPSPacketSize { get; set; }
        public int GevHeartbeatTimeoutMs { get; set; }
        public int JpegQuality { get; set; }
        public string ImageSaveFormat { get; set; }
        public string AutoFocusCommand { get; set; }
        public int AutoFocusWaitMs { get; set; }
        public int AutoConfig { get; set; }
        public int FocusModeSelector { get; set; }
        public int FocusPositionIndex { get; set; }
        public bool UseManualFocusPosition { get; set; }
        public int FocusStep { get; set; }
        public string SignalServerIp { get; set; }
        public int SignalServerPort { get; set; }
        public string SignalReceiveServerIp { get; set; }
        public int SignalReceiveServerPort { get; set; }
        public int SignalSendRetryIntervalMs { get; set; }
        public int SignalSendRetryMaxCount { get; set; }
        public bool SignalScanSuccessUntilStopped { get; set; }
        public int BufferServerPort { get; set; }
        public int BufferOriginX { get; set; }
        public int BufferOriginY { get; set; }
        public int BufferOriginZ { get; set; }
        public int BufferSpacingX { get; set; }
        public int BufferDropOffsetMm { get; set; }

        public static ScannerSettingsData CreateDefault()
        {
            return new ScannerSettingsData
            {
                ReaderIp = "192.168.1.100",
                ReaderPort = 8000,
                DeviceIndex = 0,
                ScanIntervalMs = 500,
                AutoFocus = true,
                ExposureAuto = false,
                GainAuto = false,
                AutoReconnect = true,
                SaveRawImage = true,
                ImageSavePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scan_images"),
                LightMode = "频闪模式",
                ExposureTimeUs = 15000F,
                GainDb = 0F,
                AcquisitionFrameRate = 10F,
                UseAutoPacketSize = true,
                GevSCPSPacketSize = 0,
                GevHeartbeatTimeoutMs = 3000,
                JpegQuality = 90,
                ImageSaveFormat = "JPG",
                AutoFocusCommand = "FocusOnce",
                AutoFocusWaitMs = 800,
                AutoConfig = 0,
                FocusModeSelector = 0,
                FocusPositionIndex = 0,
                UseManualFocusPosition = true,
                FocusStep = 1,
                SignalServerIp = "192.168.0.200",
                SignalServerPort = 10000,
                SignalReceiveServerIp = "192.168.0.200",
                SignalReceiveServerPort = 15000,
                SignalSendRetryIntervalMs = 500,
                SignalSendRetryMaxCount = 5,
                SignalScanSuccessUntilStopped = true,
                BufferServerPort = 8056,
                BufferOriginX = 0,
                BufferOriginY = 0,
                BufferOriginZ = 0,
                BufferSpacingX = 200,
                BufferDropOffsetMm = 0
            };
        }

        public void Normalize()
        {
            if (string.IsNullOrWhiteSpace(ReaderIp))
            {
                ReaderIp = "192.168.1.100";
            }
            if (ReaderPort <= 0)
            {
                ReaderPort = 8000;
            }
            if (DeviceIndex < 0)
            {
                DeviceIndex = 0;
            }
            if (ScanIntervalMs < 100)
            {
                ScanIntervalMs = 500;
            }
            if (string.IsNullOrWhiteSpace(ImageSavePath))
            {
                ImageSavePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scan_images");
            }
            if (string.IsNullOrWhiteSpace(LightMode))
            {
                LightMode = "频闪模式";
            }
            if (ExposureTimeUs <= 0F)
            {
                ExposureTimeUs = 15000F;
            }
            if (GainDb < 0F)
            {
                GainDb = 0F;
            }
            if (AcquisitionFrameRate <= 0F)
            {
                AcquisitionFrameRate = 10F;
            }
            if (GevHeartbeatTimeoutMs <= 0)
            {
                GevHeartbeatTimeoutMs = 3000;
            }
            if (JpegQuality <= 0 || JpegQuality > 100)
            {
                JpegQuality = 90;
            }
            if (!string.Equals(ImageSaveFormat, "BMP", StringComparison.OrdinalIgnoreCase))
            {
                ImageSaveFormat = "JPG";
            }
            if (string.IsNullOrWhiteSpace(AutoFocusCommand))
            {
                AutoFocusCommand = "FocusOnce";
            }
            if (AutoFocusWaitMs < 0)
            {
                AutoFocusWaitMs = 0;
            }
            if (AutoConfig < 0 || AutoConfig > 2)
            {
                AutoConfig = 0;
            }
            if (FocusModeSelector < 0 || FocusModeSelector > 1)
            {
                FocusModeSelector = 0;
            }
            if (FocusPositionIndex < 0 || FocusPositionIndex > 7)
            {
                FocusPositionIndex = 0;
            }
            if (FocusStep <= 0)
            {
                FocusStep = 1;
            }
            if (string.IsNullOrWhiteSpace(SignalServerIp))
            {
                SignalServerIp = "192.168.0.200";
            }
            if (SignalServerPort <= 0)
            {
                SignalServerPort = 10000;
            }
            if (string.IsNullOrWhiteSpace(SignalReceiveServerIp))
            {
                SignalReceiveServerIp = SignalServerIp;
            }
            if (SignalReceiveServerPort <= 0)
            {
                SignalReceiveServerPort = 15000;
            }
            if (SignalSendRetryIntervalMs < 100)
            {
                SignalSendRetryIntervalMs = 500;
            }
            if (SignalSendRetryMaxCount < 0)
            {
                SignalSendRetryMaxCount = 0;
            }
            SignalScanSuccessUntilStopped = true;
            if (BufferServerPort <= 0)
            {
                BufferServerPort = 8056;
            }
            if (BufferSpacingX <= 0)
            {
                BufferSpacingX = 200;
            }
            if (BufferDropOffsetMm <= 0)
            {
                BufferDropOffsetMm = 0;
            }
        }
    }

    internal sealed class OrderMatchItem
    {
        public string Barcode { get; set; }
        public string Sku { get; set; }
        public int OrderQuantity { get; set; }
        public string Length { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string PackingSequences { get; set; }
    }

    public sealed class CommittedProductionRecord
    {
        public string Barcode { get; set; }
        public string Sku { get; set; }
        public int PackingSequence { get; set; }
        public string PackingBoxId { get; set; }
        public bool IsPackingLongShortSwapped { get; set; }
        public string PlacementMode { get; set; }
    }

    internal sealed class ScanRecord
    {
        public string Mode { get; set; }
        public string OrderNo { get; set; }
        public string BatchBoxCode { get; set; }
        public int Sequence { get; set; }
        public string Barcode { get; set; }
        public string Sku { get; set; }
        public string Length { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string Status { get; set; }
        public int ScanCount { get; set; }
        public int PackingSequence { get; set; }
        public string PackingBoxId { get; set; }
        public bool IsPackingLongShortSwapped { get; set; }
        public string PlacementMode { get; set; }
        public DateTime ScanTime { get; set; }
        public string ImagePath { get; set; }
    }

    internal sealed class MatchSummaryRow
    {
        public string Barcode { get; set; }
        public string Sku { get; set; }
        public int? OrderQuantity { get; set; }
        public int ActualQuantity { get; set; }
        public string Status { get; set; }
        public string PackingSequences { get; set; }
    }
}
