using System;
using System.Net.Sockets;
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

        public bool IsConnected => _client != null && _client.Connected;

        public event EventHandler<bool> ConnectionChanged;

        public async Task ConnectAsync(string host, int port, CancellationToken cancellationToken = default)
        {
            Disconnect();

            var client = new TcpClient();
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

        public void Dispose()
        {
            Disconnect();
        }
    }
}
