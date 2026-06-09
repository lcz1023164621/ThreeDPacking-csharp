using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ThreeDPacking.App.Communication
{
    /// <summary>
    /// 通过 TCP Socket 与机械臂控制器通信的客户端。
    /// </summary>
    public class RoboticArmClient : IDisposable
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private readonly SemaphoreSlim _sendLock = new SemaphoreSlim(1, 1);
        private const string MessageTerminator = "\n";

        public bool IsConnected => _client != null && _client.Connected;

        public event EventHandler<bool> ConnectionChanged;

        public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default)
        {
            Disconnect();

            var client = new TcpClient { NoDelay = true };
            try
            {
                var connectTask = client.ConnectAsync(host, port);
                if (cancellationToken.CanBeCanceled)
                {
                    var completed = await Task.WhenAny(connectTask, Task.Delay(Timeout.Infinite, cancellationToken))
                        .ConfigureAwait(false);
                    if (completed != connectTask)
                    {
                        client.Close();
                        throw new OperationCanceledException(cancellationToken);
                    }
                }
                await connectTask.ConfigureAwait(false);

                _client = client;
                _stream = client.GetStream();
                ConnectionChanged?.Invoke(this, true);
            }
            catch
            {
                client.Dispose();
                throw;
            }
        }

        public void Disconnect()
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

            if (wasConnected)
                ConnectionChanged?.Invoke(this, false);
        }

        /// <summary>
        /// 发送单条坐标消息（自动追加换行分隔符）。
        /// </summary>
        public async Task SendCoordinateAsync(string message, CancellationToken cancellationToken = default)
        {
            await _sendLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (!IsConnected || _stream == null)
                    throw new InvalidOperationException("未连接到机械臂");

                if (message == null)
                    throw new ArgumentNullException(nameof(message));

                string framedMessage = message + MessageTerminator;
                byte[] data = Encoding.UTF8.GetBytes(framedMessage);
                await _stream.WriteAsync(data, 0, data.Length, cancellationToken).ConfigureAwait(false);
                await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _sendLock.Release();
            }
        }

        public void Dispose()
        {
            _sendLock.Dispose();
            Disconnect();
        }
    }
}
