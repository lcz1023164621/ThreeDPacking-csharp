using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using MvCodeReaderSDKNet;

namespace WindowsFormsApp1
{
    internal sealed class IdmvsBarcodeCamera : IBarcodeCamera
    {
        private readonly ScannerSettingsData _settings;
        private readonly MvCodeReader _device = new MvCodeReader();
        private MvCodeReader.cbOutputEx2delegate _callback;
        private bool _handleCreated;
        private bool _opened;
        private bool _grabbing;
        private bool _autoFocusAvailable = true;

        public event Action<BarcodeCapture> BarcodeCaptured;

        public IdmvsBarcodeCamera(ScannerSettingsData settings)
        {
            _settings = settings ?? ScannerSettingsData.CreateDefault();
            _settings.Normalize();
        }

        public void Start()
        {
            EnsureOpened();

            if (_grabbing)
            {
                return;
            }

            int ret = _device.MV_CODEREADER_StartGrabbing_NET();
            ThrowIfFailed(ret, "Start IDMVS grabbing");
            _grabbing = true;
        }

        public void TriggerOnce()
        {
            if (!_grabbing)
            {
                Start();
            }

            if (_settings.AutoFocus && _autoFocusAvailable)
            {
                RunAutoFocus();
            }

            int ret = _device.MV_CODEREADER_SetCommandValue_NET("TriggerSoftware");
            ThrowIfFailed(ret, "Trigger IDMVS software acquisition");
        }

        public void Stop()
        {
            if (_grabbing)
            {
                _device.MV_CODEREADER_StopGrabbing_NET();
                _grabbing = false;
            }
        }

        public void Dispose()
        {
            Stop();
            if (_opened)
            {
                _device.MV_CODEREADER_CloseDevice_NET();
                _opened = false;
            }
            if (_handleCreated)
            {
                _device.MV_CODEREADER_DestroyHandle_NET();
                _handleCreated = false;
            }
        }

        private void EnsureOpened()
        {
            if (_opened)
            {
                return;
            }

            var deviceList = new MvCodeReader.MV_CODEREADER_DEVICE_INFO_LIST();
            int ret = MvCodeReader.MV_CODEREADER_EnumDevices_NET(ref deviceList, MvCodeReader.MV_CODEREADER_GIGE_DEVICE);
            ThrowIfFailed(ret, "Enum IDMVS devices");
            if (deviceList.nDeviceNum == 0)
            {
                throw new InvalidOperationException("No IDMVS code reader device found.");
            }

            int deviceIndex = ResolveDeviceIndex(deviceList);
            var deviceInfo = (MvCodeReader.MV_CODEREADER_DEVICE_INFO)Marshal.PtrToStructure(
                deviceList.pDeviceInfo[deviceIndex],
                typeof(MvCodeReader.MV_CODEREADER_DEVICE_INFO));

            ret = _device.MV_CODEREADER_CreateHandle_NET(ref deviceInfo);
            ThrowIfFailed(ret, "Create IDMVS handle");
            _handleCreated = true;

            ret = _device.MV_CODEREADER_OpenDevice_NET();
            ThrowIfFailed(ret, "Open IDMVS device");
            _opened = true;

            ApplyNetworkSettings(deviceInfo);
            ApplyCaptureSettings();

            ret = _device.MV_CODEREADER_SetEnumValue_NET("TriggerMode", (uint)MvCodeReader.MV_CODEREADER_TRIGGER_MODE.MV_CODEREADER_TRIGGER_MODE_ON);
            ThrowIfFailed(ret, "Set IDMVS TriggerMode on");

            ret = _device.MV_CODEREADER_SetEnumValue_NET("TriggerSource", (uint)MvCodeReader.MV_CODEREADER_TRIGGER_SOURCE.MV_CODEREADER_TRIGGER_SOURCE_SOFTWARE);
            ThrowIfFailed(ret, "Set IDMVS TriggerSource software");

            _callback = OnImageCallback;
            ret = _device.MV_CODEREADER_RegisterImageCallBackEx2_NET(_callback, IntPtr.Zero);
            ThrowIfFailed(ret, "Register IDMVS image callback");
        }

        private int ResolveDeviceIndex(MvCodeReader.MV_CODEREADER_DEVICE_INFO_LIST deviceList)
        {
            if (!string.IsNullOrWhiteSpace(_settings.ReaderIp))
            {
                IPAddress target;
                if (IPAddress.TryParse(_settings.ReaderIp.Trim(), out target))
                {
                    uint targetValue = ToDeviceIp(target);
                    for (int i = 0; i < deviceList.nDeviceNum; i++)
                    {
                        var deviceInfo = (MvCodeReader.MV_CODEREADER_DEVICE_INFO)Marshal.PtrToStructure(
                            deviceList.pDeviceInfo[i],
                            typeof(MvCodeReader.MV_CODEREADER_DEVICE_INFO));
                        if (deviceInfo.nTLayerType != MvCodeReader.MV_CODEREADER_GIGE_DEVICE)
                        {
                            continue;
                        }

                        IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(deviceInfo.SpecialInfo.stGigEInfo, 0);
                        var gigeInfo = (MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO)Marshal.PtrToStructure(
                            buffer,
                            typeof(MvCodeReader.MV_CODEREADER_GIGE_DEVICE_INFO));
                        if (gigeInfo.nCurrentIp == targetValue)
                        {
                            return i;
                        }
                    }
                }
            }

            if (_settings.DeviceIndex < 0 || _settings.DeviceIndex >= deviceList.nDeviceNum)
            {
                throw new ArgumentOutOfRangeException("DeviceIndex", "Device index out of range. Found " + deviceList.nDeviceNum + " device(s).");
            }

            return _settings.DeviceIndex;
        }

        private void ApplyNetworkSettings(MvCodeReader.MV_CODEREADER_DEVICE_INFO deviceInfo)
        {
            if (deviceInfo.nTLayerType != MvCodeReader.MV_CODEREADER_GIGE_DEVICE)
            {
                return;
            }

            if (_settings.UseAutoPacketSize)
            {
                int packetSize = _device.MV_CODEREADER_GetOptimalPacketSize_NET();
                if (packetSize > 0)
                {
                    TrySetIntValue("GevSCPSPacketSize", packetSize);
                }
            }
            else if (_settings.GevSCPSPacketSize > 0)
            {
                TrySetIntValue("GevSCPSPacketSize", _settings.GevSCPSPacketSize);
            }

            if (_settings.GevHeartbeatTimeoutMs > 0)
            {
                TrySetIntValue("GevHeartbeatTimeout", _settings.GevHeartbeatTimeoutMs);
            }
        }

        private void ApplyCaptureSettings()
        {
            TrySetFloatValue("ExposureTime", _settings.ExposureTimeUs);
            TrySetEnumValue("GainAuto", 0U);
            TrySetFloatValue("Gain", _settings.GainDb);
        }

        private void OnImageCallback(IntPtr data, IntPtr frameInfoPtr, IntPtr user)
        {
            if (frameInfoPtr == IntPtr.Zero)
            {
                return;
            }

            var frameInfo = (MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2)Marshal.PtrToStructure(
                frameInfoPtr,
                typeof(MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2));

            if (frameInfo.UnparsedBcrList.pstCodeListEx2 == IntPtr.Zero)
            {
                return;
            }

            var bcrResult = (MvCodeReader.MV_CODEREADER_RESULT_BCR_EX2)Marshal.PtrToStructure(
                frameInfo.UnparsedBcrList.pstCodeListEx2,
                typeof(MvCodeReader.MV_CODEREADER_RESULT_BCR_EX2));

            for (int i = 0; i < bcrResult.nCodeNum; i++)
            {
                string code = DecodeCode(bcrResult.stBcrInfoEx2[i].chCode);
                if (!string.IsNullOrWhiteSpace(code))
                {
                    byte[] imageBytes = TryEncodeFrame(data, frameInfo);
                    Action<BarcodeCapture> handler = BarcodeCaptured;
                    if (handler != null)
                    {
                        handler(new BarcodeCapture(code, imageBytes, ".jpg"));
                    }
                    return;
                }
            }
        }

        private byte[] TryEncodeFrame(IntPtr data, MvCodeReader.MV_CODEREADER_IMAGE_OUT_INFO_EX2 frameInfo)
        {
            if (!_settings.SaveRawImage || data == IntPtr.Zero || frameInfo.nFrameLen == 0 || frameInfo.nWidth == 0 || frameInfo.nHeight == 0)
            {
                return null;
            }

            IntPtr imageBuffer = IntPtr.Zero;
            try
            {
                uint bufferSize = Math.Max(frameInfo.nFrameLen + 2048, (uint)frameInfo.nWidth * frameInfo.nHeight * 4 + 2048);
                imageBuffer = Marshal.AllocHGlobal((int)bufferSize);
                var saveParam = new MvCodeReader.MV_CODEREADER_SAVE_IMAGE_PARAM_EX
                {
                    pData = data,
                    nDataLen = frameInfo.nFrameLen,
                    enPixelType = frameInfo.enPixelType,
                    nWidth = frameInfo.nWidth,
                    nHeight = frameInfo.nHeight,
                    pImageBuffer = imageBuffer,
                    nBufferSize = bufferSize,
                    enImageType = MvCodeReader.MV_CODEREADER_IAMGE_TYPE.MV_CODEREADER_Image_Jpeg,
                    nJpgQuality = (uint)Math.Max(1, Math.Min(100, _settings.JpegQuality)),
                    iMethodValue = 0
                };

                int ret = _device.MV_CODEREADER_SaveImage_NET(ref saveParam);
                if (ret != MvCodeReader.MV_CODEREADER_OK || saveParam.nImageLen == 0)
                {
                    return null;
                }

                byte[] bytes = new byte[saveParam.nImageLen];
                Marshal.Copy(saveParam.pImageBuffer, bytes, 0, bytes.Length);
                return bytes;
            }
            catch
            {
                return null;
            }
            finally
            {
                if (imageBuffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(imageBuffer);
                }
            }
        }

        private static string DecodeCode(byte[] bytes)
        {
            if (bytes == null)
            {
                return string.Empty;
            }

            int length = Array.IndexOf(bytes, (byte)0);
            if (length < 0)
            {
                length = bytes.Length;
            }

            byte[] payload = new byte[length];
            Array.Copy(bytes, payload, length);

            Encoding encoding = LooksLikeUtf8(payload) ? Encoding.UTF8 : Encoding.GetEncoding("GB2312");
            return encoding.GetString(payload).Trim();
        }

        private static bool LooksLikeUtf8(byte[] bytes)
        {
            bool hasNonAscii = false;
            int remaining = 0;

            foreach (byte value in bytes)
            {
                if ((value & 0x80) != 0)
                {
                    hasNonAscii = true;
                }

                if (remaining == 0)
                {
                    if ((value & 0x80) == 0)
                    {
                        continue;
                    }
                    if ((value & 0xE0) == 0xC0)
                    {
                        remaining = 1;
                    }
                    else if ((value & 0xF0) == 0xE0)
                    {
                        remaining = 2;
                    }
                    else if ((value & 0xF8) == 0xF0)
                    {
                        remaining = 3;
                    }
                    else
                    {
                        return false;
                    }
                }
                else if ((value & 0xC0) == 0x80)
                {
                    remaining--;
                }
                else
                {
                    return false;
                }
            }

            return hasNonAscii && remaining == 0;
        }

        private void RunAutoFocus()
        {
            int ret = _device.MV_CODEREADER_SetCommandValue_NET(_settings.AutoFocusCommand);
            if (ret != MvCodeReader.MV_CODEREADER_OK)
            {
                _autoFocusAvailable = false;
            }
        }

        private void TrySetIntValue(string key, int value)
        {
            _device.MV_CODEREADER_SetIntValue_NET(key, value);
        }

        private void TrySetFloatValue(string key, float value)
        {
            _device.MV_CODEREADER_SetFloatValue_NET(key, value);
        }

        private void TrySetEnumValue(string key, uint value)
        {
            _device.MV_CODEREADER_SetEnumValue_NET(key, value);
        }

        private static uint ToDeviceIp(IPAddress address)
        {
            byte[] bytes = address.GetAddressBytes();
            if (bytes.Length != 4)
            {
                return 0;
            }

            return ((uint)bytes[0] << 24) | ((uint)bytes[1] << 16) | ((uint)bytes[2] << 8) | bytes[3];
        }

        private static void ThrowIfFailed(int ret, string action)
        {
            if (ret != MvCodeReader.MV_CODEREADER_OK)
            {
                throw new InvalidOperationException(action + " failed: 0x" + ret.ToString("X8"));
            }
        }
    }
}
