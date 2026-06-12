using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    internal sealed class ProductionSignalClient : IDisposable
    {
        public const int SignalRobotReady = 0;
        public const int SignalScanSuccess = 1;
        public const int SignalScanFailed = 2;
        public const int SignalRobotAcknowledged = 3;
        public const int SignalBatchComplete = 4;
        public const int SignalScanFailedAcknowledged = 5;
        public const int SignalStraightPlacement = 6;
        public const int SignalLongShortSwapped = 7;

        private TcpClient _sendClient;
        private NetworkStream _sendStream;
        private TcpClient _receiveClient;
        private NetworkStream _receiveStream;
        private CancellationTokenSource _cts;
        private Task _sendWorkerTask;
        private Task _receiveWorkerTask;
        private readonly object _sendLock = new object();
        private readonly List<byte> _receiveBuffer = new List<byte>();
        private string _sendHost;
        private int _sendPort;
        private string _receiveHost;
        private int _receivePort;
        private bool _autoReconnect;
        private int? _lastSentValue;

        public const int SignalFrameSize = 4;
        public const byte SignalFrameDelimiter = (byte)'\n';
        private const int ConnectTimeoutMs = 5000;

        public event Action<int> SignalReceived;
        public event Action<bool, bool> ConnectionChanged;
        public event Action<bool, int, string> DataTransmitted;

        public bool IsConnected
        {
            get { return IsSendConnected && IsReceiveConnected; }
        }

        public bool IsSendConnected
        {
            get { return _sendClient != null && _sendClient.Connected; }
        }

        public bool IsReceiveConnected
        {
            get { return _receiveClient != null && _receiveClient.Connected; }
        }

        public string EndpointDescription
        {
            get { return "发送 " + SendEndpointDescription + "，接收 " + ReceiveEndpointDescription; }
        }

        public string SendEndpointDescription
        {
            get { return _sendHost + ":" + _sendPort; }
        }

        public string ReceiveEndpointDescription
        {
            get { return _receiveHost + ":" + _receivePort; }
        }

        public void Start(string host, int port, bool autoReconnect)
        {
            Start(host, port, host, 15000, autoReconnect);
        }

        public void Start(string sendHost, int sendPort, string receiveHost, int receivePort, bool autoReconnect)
        {
            Stop();
            _sendHost = sendHost ?? string.Empty;
            _sendPort = sendPort;
            _receiveHost = string.IsNullOrWhiteSpace(receiveHost) ? _sendHost : receiveHost;
            _receivePort = receivePort;
            _autoReconnect = autoReconnect;
            _receiveBuffer.Clear();
            _lastSentValue = null;
            _cts = new CancellationTokenSource();
            _sendWorkerTask = Task.Run(() => RunSendLoopAsync(_cts.Token));
            _receiveWorkerTask = Task.Run(() => RunReceiveLoopAsync(_cts.Token));
        }

        public void Stop()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }

            _receiveBuffer.Clear();
            CloseSendConnection();
            CloseReceiveConnection();
        }

        public bool Send(int value)
        {
            return Send(value, false);
        }

        public bool Send(int value, bool allowRepeat)
        {
            if (!IsSendConnected || _sendStream == null)
            {
                return false;
            }

            if (!IsValidSignal(value))
            {
                return false;
            }

            if (!allowRepeat && _lastSentValue == value && value != SignalScanSuccess && value != SignalScanFailed)
            {
                return true;
            }

            byte[] payload = BuildSignalFrame(value);
            lock (_sendLock)
            {
                try
                {
                    _sendStream.Write(payload, 0, payload.Length);
                    _sendStream.Flush();
                    _lastSentValue = value;
                    NotifyTransmitted(true, value, BuildSignalDetail(value));
                    return true;
                }
                catch (IOException)
                {
                    CloseSendConnection();
                    return false;
                }
                catch (SocketException)
                {
                    CloseSendConnection();
                    return false;
                }
            }
        }

        public bool SendString(string text)
        {
            if (!IsSendConnected || _sendStream == null || string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            string value = text.Trim();
            byte[] payload = Encoding.ASCII.GetBytes(value + "\n");
            lock (_sendLock)
            {
                try
                {
                    _sendStream.Write(payload, 0, payload.Length);
                    _sendStream.Flush();
                    NotifyTransmitted(true, -1, "String=\"" + EscapeControlChars(value) + "\" Frame=\"" + EscapeControlChars(value + "\n") + "\"");
                    return true;
                }
                catch (IOException)
                {
                    CloseSendConnection();
                    return false;
                }
                catch (SocketException)
                {
                    CloseSendConnection();
                    return false;
                }
            }
        }

        public void ResetSendState()
        {
            _lastSentValue = null;
        }

        public void Dispose()
        {
            Stop();
        }

        public static byte[] BuildSignalFrame(int value)
        {
            // 每条信号以换行结尾，避免 TCP 粘包把多个数字拼成 222... 导致对端整型溢出。
            return new byte[] { (byte)('0' + value), SignalFrameDelimiter };
        }

        private static string BuildSignalDetail(int value)
        {
            char ascii = (char)('0' + value);
            return "Int=" + value + " (ASCII=\"" + ascii + "\" Hex=" + ((byte)ascii).ToString("X2")
                + " Frame=\"" + ascii + "\\n\")";
        }

        private async Task RunSendLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!IsSendConnected)
                    {
                        await ConnectSendOnceAsync(token).ConfigureAwait(false);
                    }

                    while (!token.IsCancellationRequested && IsSendConnected)
                    {
                        await Task.Delay(1000, token).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    CloseSendConnection();
                    if (!token.IsCancellationRequested && _autoReconnect)
                    {
                        await Task.Delay(1000, token).ConfigureAwait(false);
                    }
                    else if (!_autoReconnect)
                    {
                        break;
                    }
                }
            }
        }

        private async Task RunReceiveLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!IsReceiveConnected)
                    {
                        await ConnectReceiveOnceAsync(token).ConfigureAwait(false);
                    }

                    await ReadSignalsAsync(token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    CloseReceiveConnection();
                    if (!token.IsCancellationRequested && _autoReconnect)
                    {
                        await Task.Delay(1000, token).ConfigureAwait(false);
                    }
                    else if (!_autoReconnect)
                    {
                        break;
                    }
                }
            }
        }

        private async Task ConnectSendOnceAsync(CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(_sendHost) || _sendPort <= 0)
            {
                throw new InvalidOperationException("信号发送服务器地址或端口无效。");
            }

            token.ThrowIfCancellationRequested();
            var client = new TcpClient { NoDelay = true };
            await ConnectWithTimeoutAsync(client, _sendHost, _sendPort, token).ConfigureAwait(false);
            _sendClient = client;
            _sendStream = client.GetStream();
            ConnectionChanged?.Invoke(IsSendConnected, IsReceiveConnected);
        }

        private async Task ConnectReceiveOnceAsync(CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(_receiveHost) || _receivePort <= 0)
            {
                throw new InvalidOperationException("信号接收服务器地址或端口无效。");
            }

            token.ThrowIfCancellationRequested();
            var client = new TcpClient { NoDelay = true };
            await ConnectWithTimeoutAsync(client, _receiveHost, _receivePort, token).ConfigureAwait(false);
            _receiveClient = client;
            _receiveStream = client.GetStream();
            _receiveBuffer.Clear();
            ConnectionChanged?.Invoke(IsSendConnected, IsReceiveConnected);
        }

        private async Task ReadSignalsAsync(CancellationToken token)
        {
            byte[] buffer = new byte[256];
            while (!token.IsCancellationRequested && IsReceiveConnected)
            {
                int read = await _receiveStream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
                if (read <= 0)
                {
                    throw new IOException("信号接收连接已断开。");
                }

                for (int i = 0; i < read; i++)
                {
                    _receiveBuffer.Add(buffer[i]);
                }

                ProcessReceiveBuffer();
            }
        }

        private void ProcessReceiveBuffer()
        {
            while (_receiveBuffer.Count > 0)
            {
                TrimLeadingWhitespace();

                if (_receiveBuffer.Count >= SignalFrameSize && IsFixedWidthAsciiFrame(_receiveBuffer, 0))
                {
                    string frame = Encoding.ASCII.GetString(_receiveBuffer.ToArray(), 0, SignalFrameSize);
                    _receiveBuffer.RemoveRange(0, SignalFrameSize);
                    int value;
                    if (int.TryParse(frame, out value))
                    {
                        DispatchSignal(value, "Raw=\"" + EscapeControlChars(frame) + "\" Parsed=" + value);
                        continue;
                    }
                }

                int newlineIndex = _receiveBuffer.IndexOf((byte)'\n');
                if (newlineIndex >= 0)
                {
                    if (TryConsumeAsciiMessage(0, newlineIndex))
                    {
                        TrimLeadingWhitespace();
                        continue;
                    }
                }

                int digitLength = CountLeadingAsciiDigits(_receiveBuffer, 0);
                if (digitLength > 0)
                {
                    bool hasMoreData = digitLength < _receiveBuffer.Count;
                    bool nextIsDelimiter = hasMoreData && IsDelimiterByte(_receiveBuffer[digitLength]);

                    if (digitLength > 1 && !nextIsDelimiter)
                    {
                        if (TryConsumeAsciiMessage(0, 1))
                        {
                            continue;
                        }
                    }

                    if (!hasMoreData || nextIsDelimiter)
                    {
                        if (TryConsumeAsciiMessage(0, digitLength))
                        {
                            if (nextIsDelimiter)
                            {
                                _receiveBuffer.RemoveAt(0);
                            }
                            continue;
                        }
                    }
                }

                if (_receiveBuffer.Count >= 4 && digitLength == 0)
                {
                    byte[] rawBytes = _receiveBuffer.GetRange(0, 4).ToArray();
                    int value = BitConverter.ToInt32(_receiveBuffer.ToArray(), 0);
                    _receiveBuffer.RemoveRange(0, 4);
                    DispatchSignal(value, "RawHex=" + FormatBytes(rawBytes) + " Parsed=" + value);
                    continue;
                }

                break;
            }
        }

        private bool TryConsumeAsciiMessage(int start, int length)
        {
            if (length <= 0)
            {
                return false;
            }

            string rawText = Encoding.ASCII.GetString(_receiveBuffer.ToArray(), start, length);
            string text = rawText.Trim();
            _receiveBuffer.RemoveRange(start, length);

            int value;
            if (!int.TryParse(text, out value))
            {
                if (text.Length > 0)
                {
                    NotifyTransmitted(false, -1, "无法解析字符串=\"" + text + "\"");
                }
                return text.Length > 0;
            }

            DispatchSignal(value, "Raw=\"" + EscapeControlChars(rawText) + "\" Parsed=" + value);
            return true;
        }

        private static bool IsValidSignal(int value)
        {
            return value >= SignalRobotReady && value <= SignalLongShortSwapped;
        }

        private void DispatchSignal(int value)
        {
            DispatchSignal(value, BuildSignalDetail(value));
        }

        private void DispatchSignal(int value, string detail)
        {
            if (!IsValidSignal(value))
            {
                NotifyTransmitted(false, value, "忽略无效整型信号 " + value + " (" + detail + ")");
                return;
            }

            NotifyTransmitted(false, value, detail);
            SignalReceived?.Invoke(value);
        }

        private void TrimLeadingWhitespace()
        {
            while (_receiveBuffer.Count > 0 && IsDelimiterByte(_receiveBuffer[0]))
            {
                _receiveBuffer.RemoveAt(0);
            }
        }

        private static int CountLeadingAsciiDigits(List<byte> buffer, int start)
        {
            int count = 0;
            for (int i = start; i < buffer.Count; i++)
            {
                if (IsAsciiDigitByte(buffer[i]))
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            return count;
        }

        private static bool IsAsciiDigitByte(byte value)
        {
            return value >= (byte)'0' && value <= (byte)'9';
        }

        private static bool IsDelimiterByte(byte value)
        {
            return value == (byte)'\r' || value == (byte)'\n' || value == (byte)' ' || value == (byte)'\t';
        }

        private static bool IsFixedWidthAsciiFrame(List<byte> buffer, int start)
        {
            if (buffer.Count - start < SignalFrameSize)
            {
                return false;
            }

            for (int i = 0; i < SignalFrameSize; i++)
            {
                if (!IsAsciiDigitByte(buffer[start + i]))
                {
                    return false;
                }
            }

            return true;
        }

        private void NotifyTransmitted(bool sent, int value, string detail)
        {
            Action<bool, int, string> handler = DataTransmitted;
            if (handler != null)
            {
                handler(sent, value, detail);
            }
        }

        private static string FormatBytes(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder(bytes.Length * 3);
            for (int i = 0; i < bytes.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(' ');
                }
                builder.Append(bytes[i].ToString("X2"));
            }
            return builder.ToString();
        }

        private static async Task ConnectWithTimeoutAsync(TcpClient client, string host, int port, CancellationToken token)
        {
            Task connectTask = client.ConnectAsync(host, port);
            Task delayTask = Task.Delay(ConnectTimeoutMs, token);
            Task completed = await Task.WhenAny(connectTask, delayTask).ConfigureAwait(false);
            if (completed != connectTask)
            {
                try
                {
                    client.Close();
                }
                catch
                {
                }

                throw new TimeoutException("连接 " + host + ":" + port + " 超时。");
            }

            await connectTask.ConfigureAwait(false);
        }

        private static string EscapeControlChars(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value
                .Replace("\\", "\\\\")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\t", "\\t");
        }

        private void CloseSendConnection()
        {
            bool wasConnected = IsSendConnected;

            if (_sendStream != null)
            {
                _sendStream.Dispose();
                _sendStream = null;
            }

            if (_sendClient != null)
            {
                _sendClient.Close();
                _sendClient.Dispose();
                _sendClient = null;
            }

            if (wasConnected)
            {
                ConnectionChanged?.Invoke(IsSendConnected, IsReceiveConnected);
            }
        }

        private void CloseReceiveConnection()
        {
            bool wasConnected = IsReceiveConnected;

            if (_receiveStream != null)
            {
                _receiveStream.Dispose();
                _receiveStream = null;
            }

            if (_receiveClient != null)
            {
                _receiveClient.Close();
                _receiveClient.Dispose();
                _receiveClient = null;
            }

            _receiveBuffer.Clear();

            if (wasConnected)
            {
                ConnectionChanged?.Invoke(IsSendConnected, IsReceiveConnected);
            }
        }
    }
}
