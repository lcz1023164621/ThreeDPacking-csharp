using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace WindowsFormsApp1
{
    public static class ScannerSettingsStore
    {
        private static string SettingsPath
        {
            get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scanner_settings.ini"); }
        }

        public static ScannerSettingsData Load()
        {
            var settings = ScannerSettingsData.CreateDefault();
            if (!File.Exists(SettingsPath))
            {
                return settings;
            }

            try
            {
                foreach (string rawLine in File.ReadAllLines(SettingsPath, Encoding.UTF8))
                {
                    string line = rawLine.Trim();
                    if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    int index = line.IndexOf('=');
                    if (index <= 0)
                    {
                        continue;
                    }

                    string key = line.Substring(0, index).Trim();
                    string value = line.Substring(index + 1).Trim();
                    Apply(settings, key, value);
                }
            }
            catch
            {
                return ScannerSettingsData.CreateDefault();
            }

            settings.Normalize();
            return settings;
        }

        public static void Save(ScannerSettingsData settings)
        {
            if (settings == null)
            {
                return;
            }

            settings.Normalize();
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath) ?? AppDomain.CurrentDomain.BaseDirectory);
            var builder = new StringBuilder();
            builder.AppendLine("# CRreader scanner settings");
            builder.AppendLine("ReaderIp=" + settings.ReaderIp);
            builder.AppendLine("ReaderPort=" + settings.ReaderPort.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("DeviceIndex=" + settings.DeviceIndex.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("ScanIntervalMs=" + settings.ScanIntervalMs.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("AutoFocus=" + settings.AutoFocus);
            builder.AppendLine("ExposureAuto=" + settings.ExposureAuto);
            builder.AppendLine("GainAuto=" + settings.GainAuto);
            builder.AppendLine("AutoReconnect=" + settings.AutoReconnect);
            builder.AppendLine("SaveRawImage=" + settings.SaveRawImage);
            builder.AppendLine("ImageSavePath=" + settings.ImageSavePath);
            builder.AppendLine("LightMode=" + settings.LightMode);
            builder.AppendLine("ExposureTimeUs=" + settings.ExposureTimeUs.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("GainDb=" + settings.GainDb.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("AcquisitionFrameRate=" + settings.AcquisitionFrameRate.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("UseAutoPacketSize=" + settings.UseAutoPacketSize);
            builder.AppendLine("GevSCPSPacketSize=" + settings.GevSCPSPacketSize.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("GevHeartbeatTimeoutMs=" + settings.GevHeartbeatTimeoutMs.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("JpegQuality=" + settings.JpegQuality.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("ImageSaveFormat=" + settings.ImageSaveFormat);
            builder.AppendLine("AutoFocusCommand=" + settings.AutoFocusCommand);
            builder.AppendLine("AutoFocusWaitMs=" + settings.AutoFocusWaitMs.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("AutoConfig=" + settings.AutoConfig.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("FocusModeSelector=" + settings.FocusModeSelector.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("FocusPositionIndex=" + settings.FocusPositionIndex.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("UseManualFocusPosition=" + settings.UseManualFocusPosition);
            builder.AppendLine("FocusStep=" + settings.FocusStep.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("EnabledBarcodeSymbologies=" + BarcodeSymbologyCatalog.FormatEnabledSet(settings.EnabledBarcodeSymbologies));
            builder.AppendLine("SignalServerIp=" + settings.SignalServerIp);
            builder.AppendLine("SignalServerPort=" + settings.SignalServerPort.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("SignalReceiveServerIp=" + settings.SignalReceiveServerIp);
            builder.AppendLine("SignalReceiveServerPort=" + settings.SignalReceiveServerPort.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("SignalSendRetryIntervalMs=" + settings.SignalSendRetryIntervalMs.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("SignalSendRetryMaxCount=" + settings.SignalSendRetryMaxCount.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("SignalScanSuccessUntilStopped=" + settings.SignalScanSuccessUntilStopped);
            builder.AppendLine("BufferServerPort=" + settings.BufferServerPort.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("BufferOriginX=" + settings.BufferOriginX.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("BufferOriginY=" + settings.BufferOriginY.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("BufferOriginZ=" + settings.BufferOriginZ.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("BufferSpacingX=" + settings.BufferSpacingX.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("BufferDropOffsetMm=" + settings.BufferDropOffsetMm.ToString(CultureInfo.InvariantCulture));
            File.WriteAllText(SettingsPath, builder.ToString(), new UTF8Encoding(true));
        }

        private static void Apply(ScannerSettingsData settings, string key, string value)
        {
            switch (key)
            {
                case "ReaderIp":
                    settings.ReaderIp = value;
                    break;
                case "ReaderPort":
                    int readerPort;
                    if (int.TryParse(value, out readerPort))
                    {
                        settings.ReaderPort = readerPort;
                    }
                    break;
                case "DeviceIndex":
                    int deviceIndex;
                    if (int.TryParse(value, out deviceIndex))
                    {
                        settings.DeviceIndex = deviceIndex;
                    }
                    break;
                case "ScanIntervalMs":
                    int scanIntervalMs;
                    if (int.TryParse(value, out scanIntervalMs))
                    {
                        settings.ScanIntervalMs = scanIntervalMs;
                    }
                    break;
                case "AutoFocus":
                    settings.AutoFocus = ParseBool(value);
                    break;
                case "ExposureAuto":
                    settings.ExposureAuto = ParseBool(value);
                    break;
                case "GainAuto":
                    settings.GainAuto = ParseBool(value);
                    break;
                case "AutoReconnect":
                    settings.AutoReconnect = ParseBool(value);
                    break;
                case "SaveRawImage":
                    settings.SaveRawImage = ParseBool(value);
                    break;
                case "ImageSavePath":
                    settings.ImageSavePath = value;
                    break;
                case "LightMode":
                    settings.LightMode = value;
                    break;
                case "ExposureTimeUs":
                    float exposureTimeUs;
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out exposureTimeUs))
                    {
                        settings.ExposureTimeUs = exposureTimeUs;
                    }
                    break;
                case "GainDb":
                    float gainDb;
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out gainDb))
                    {
                        settings.GainDb = gainDb;
                    }
                    break;
                case "AcquisitionFrameRate":
                    float acquisitionFrameRate;
                    if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out acquisitionFrameRate))
                    {
                        settings.AcquisitionFrameRate = acquisitionFrameRate;
                    }
                    break;
                case "UseAutoPacketSize":
                    settings.UseAutoPacketSize = ParseBool(value);
                    break;
                case "GevSCPSPacketSize":
                    int packetSize;
                    if (int.TryParse(value, out packetSize))
                    {
                        settings.GevSCPSPacketSize = packetSize;
                    }
                    break;
                case "GevHeartbeatTimeoutMs":
                    int heartbeatTimeoutMs;
                    if (int.TryParse(value, out heartbeatTimeoutMs))
                    {
                        settings.GevHeartbeatTimeoutMs = heartbeatTimeoutMs;
                    }
                    break;
                case "JpegQuality":
                    int jpegQuality;
                    if (int.TryParse(value, out jpegQuality))
                    {
                        settings.JpegQuality = jpegQuality;
                    }
                    break;
                case "ImageSaveFormat":
                    settings.ImageSaveFormat = value;
                    break;
                case "AutoFocusCommand":
                    settings.AutoFocusCommand = value;
                    break;
                case "AutoFocusWaitMs":
                    int autoFocusWaitMs;
                    if (int.TryParse(value, out autoFocusWaitMs))
                    {
                        settings.AutoFocusWaitMs = autoFocusWaitMs;
                    }
                    break;
                case "AutoConfig":
                    int autoConfig;
                    if (int.TryParse(value, out autoConfig))
                    {
                        settings.AutoConfig = autoConfig;
                    }
                    break;
                case "FocusModeSelector":
                    int focusModeSelector;
                    if (int.TryParse(value, out focusModeSelector))
                    {
                        settings.FocusModeSelector = focusModeSelector;
                    }
                    break;
                case "FocusPositionIndex":
                    int focusPositionIndex;
                    if (int.TryParse(value, out focusPositionIndex))
                    {
                        settings.FocusPositionIndex = focusPositionIndex;
                    }
                    break;
                case "UseManualFocusPosition":
                    settings.UseManualFocusPosition = ParseBool(value);
                    break;
                case "FocusStep":
                    int focusStep;
                    if (int.TryParse(value, out focusStep))
                    {
                        settings.FocusStep = focusStep;
                    }
                    break;
                case "EnabledBarcodeSymbologies":
                    settings.EnabledBarcodeSymbologies = BarcodeSymbologyCatalog.ParseEnabledSet(value);
                    break;
                case "SignalServerIp":
                    settings.SignalServerIp = value;
                    break;
                case "SignalServerPort":
                    int signalServerPort;
                    if (int.TryParse(value, out signalServerPort))
                    {
                        settings.SignalServerPort = signalServerPort;
                    }
                    break;
                case "SignalReceiveServerIp":
                    settings.SignalReceiveServerIp = value;
                    break;
                case "SignalReceiveServerPort":
                    int signalReceiveServerPort;
                    if (int.TryParse(value, out signalReceiveServerPort))
                    {
                        settings.SignalReceiveServerPort = signalReceiveServerPort;
                    }
                    break;
                case "SignalSendRetryIntervalMs":
                case "SignalFailRetryIntervalMs":
                    int signalSendRetryIntervalMs;
                    if (int.TryParse(value, out signalSendRetryIntervalMs))
                    {
                        settings.SignalSendRetryIntervalMs = signalSendRetryIntervalMs;
                    }
                    break;
                case "SignalSendRetryMaxCount":
                case "SignalFailRetryMaxCount":
                    int signalSendRetryMaxCount;
                    if (int.TryParse(value, out signalSendRetryMaxCount))
                    {
                        settings.SignalSendRetryMaxCount = signalSendRetryMaxCount;
                    }
                    break;
                case "SignalScanSuccessUntilStopped":
                    settings.SignalScanSuccessUntilStopped = ParseBool(value);
                    break;
                case "BufferServerPort":
                    int bufferServerPort;
                    if (int.TryParse(value, out bufferServerPort))
                    {
                        settings.BufferServerPort = bufferServerPort;
                    }
                    break;
                case "BufferOriginX":
                    int bufferOriginX;
                    if (int.TryParse(value, out bufferOriginX))
                    {
                        settings.BufferOriginX = bufferOriginX;
                    }
                    break;
                case "BufferOriginY":
                    int bufferOriginY;
                    if (int.TryParse(value, out bufferOriginY))
                    {
                        settings.BufferOriginY = bufferOriginY;
                    }
                    break;
                case "BufferOriginZ":
                    int bufferOriginZ;
                    if (int.TryParse(value, out bufferOriginZ))
                    {
                        settings.BufferOriginZ = bufferOriginZ;
                    }
                    break;
                case "BufferSpacingX":
                    int bufferSpacingX;
                    if (int.TryParse(value, out bufferSpacingX))
                    {
                        settings.BufferSpacingX = bufferSpacingX;
                    }
                    break;
                case "BufferDropOffsetMm":
                    int bufferDropOffsetMm;
                    if (int.TryParse(value, out bufferDropOffsetMm))
                    {
                        settings.BufferDropOffsetMm = bufferDropOffsetMm;
                    }
                    break;
            }
        }

        private static bool ParseBool(string value)
        {
            bool parsed;
            return bool.TryParse(value, out parsed) && parsed;
        }
    }
}
