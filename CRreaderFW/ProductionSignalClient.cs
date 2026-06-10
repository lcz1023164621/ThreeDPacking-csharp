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
        public const int SignalBatchComplete = 3;

        private TcpClient _client;
        private NetworkStream _stream;
        private CancellationTokenSource _cts;
        private Task _workerTask;
        private readonly object _sendLock = new object();
        private readonly List<byte> _receiveBuffer = new List<byte>();
        private string _host;
        private int _port;
        private bool _autoReconnect;
        private int? _lastSentValue;

        public const int SignalFrameSize = 4;

        public event Action<int> SignalReceived;
        public event Action<bool> ConnectionChanged;
        public event Action<bool, int, string> DataTransmitted;

        public bool IsConnected
        {
            get { return _client != null && _client.Connected; }
        }

        public string EndpointDescription
        {
            get { return _host + ":" + _port; }
        }

        public void Start(string host, int port, bool autoReconnect)
        {
            Stop();
            _host = host ?? string.Empty;
            _port = port;
            _autoReconnect = autoReconnect;
            _receiveBuffer.Clear();
            _lastSentValue = null;
            _cts = new CancellationTokenSource();
            _workerTask = Task.Run(() => RunLoopAsync(_cts.Token));
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
            CloseConnection();
        }

        public bool Send(int value)
        {
            if (!IsConnected || _stream == null)
            {
                return false;
            }

            if (!IsValidSignal(value))
            {
                return false;
            }

            if (_lastSentValue == value && value != SignalScanSuccess)
            {
                return true;
            }

            byte[] payload = BuildSignalFrame(value);
            lock (_sendLock)
            {
                try
                {
                    _stream.Write(payload, 0, payload.Length);
                    _stream.Flush();
                    _lastSentValue = value;
                    NotifyTransmitted(true, value, BuildSignalDetail(value));
                    return true;
                }
                catch (IOException)
                {
                    CloseConnection();
                    return false;
                }
                catch (SocketException)
                {
                    CloseConnection();
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
            return new byte[] { (byte)('0' + value) };
        }

        private static string BuildSignalDetail(int value)
        {
            char ascii = (char)('0' + value);
            return "Int=" + value + " (ASCII=\"" + ascii + "\" Hex=" + ((byte)ascii).ToString("X2") + ")";
        }

        private async Task RunLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (!IsConnected)
                    {
                        await ConnectOnceAsync(token).ConfigureAwait(false);
                    }

                    await ReadSignalsAsync(token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception)
                {
                    CloseConnection();
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

        private async Task ConnectOnceAsync(CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(_host) || _port <= 0)
            {
                throw new InvalidOperationException("信号服务器地址或端口无效。");
            }

            var client = new TcpClient { NoDelay = true };
            await client.ConnectAsync(_host, _port).ConfigureAwait(false);
            _client = client;
            _stream = client.GetStream();
            _receiveBuffer.Clear();
            ConnectionChanged?.Invoke(true);
        }

        private async Task ReadSignalsAsync(CancellationToken token)
        {
            byte[] buffer = new byte[256];
            while (!token.IsCancellationRequested && IsConnected)
            {
                int read = await _stream.ReadAsync(buffer, 0, buffer.Length, token).ConfigureAwait(false);
                if (read <= 0)
                {
                    throw new IOException("信号连接已断开。");
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
                        DispatchSignal(value);
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
                    if (!hasMoreData || nextIsDelimiter)
                    {
                        if (TryConsumeAsciiMessage(0, digitLength))
                        {
                            continue;
                        }
                    }
                }

                if (_receiveBuffer.Count >= 4 && digitLength == 0)
                {
                    int value = BitConverter.ToInt32(_receiveBuffer.ToArray(), 0);
                    _receiveBuffer.RemoveRange(0, 4);
                    DispatchSignal(value);
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

            string text = Encoding.ASCII.GetString(_receiveBuffer.ToArray(), start, length).Trim();
            _receiveBuffer.RemoveRange(start, length);

            int value;
            if (!int.TryParse(text, out value))
            {
                if (text.Length > 0)
                {
                    NotifyTransmitted(false, -1, "无法解析文本=\"" + text + "\"");
                }
                return text.Length > 0;
            }

            DispatchSignal(value);
            return true;
        }

        private static bool IsValidSignal(int value)
        {
            return value >= SignalRobotReady && value <= SignalBatchComplete;
        }

        private void DispatchSignal(int value)
        {
            if (!IsValidSignal(value))
            {
                NotifyTransmitted(false, value, "忽略无效整型信号 " + value);
                return;
            }

            NotifyTransmitted(false, value, BuildSignalDetail(value));
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

        private void CloseConnection()
        {
            bool wasConnected = IsConnected;

            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }

            if (_client != null)
            {
                _client.Close();
                _client.Dispose();
                _client = null;
            }

            _receiveBuffer.Clear();

            if (wasConnected)
            {
                ConnectionChanged?.Invoke(false);
            }
        }
    }
}
