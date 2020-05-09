using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Serilog.Sinks.Fluentd.Sinks.Fluentd.Endpoints
{
    internal class TcpEndpoint : IEndpoint
    {
        private readonly FluentdSinkOptions _options;
        private TcpClient _tcpClient;

        public TcpEndpoint(FluentdSinkOptions options)
        {
            _options = options;

            _tcpClient = new TcpClient
            {
                NoDelay = _options.NoDelay,
                ReceiveBufferSize = _options.ReceiveBufferSize,
                SendBufferSize = _options.SendBufferSize,
                SendTimeout = _options.SendTimeout,
                ReceiveTimeout = _options.SendTimeout,
                LingerState = new LingerOption(_options.LingerEnabled, _options.LingerTime)
            };
        }

        public async Task ConnectAsync()
        {
            await _tcpClient.ConnectAsync(_options.Host, _options.Port);
        }

        public Stream GetStream()
        {
            return _tcpClient.GetStream();
        }

        public bool IsConnected()
        {
            if (_tcpClient == null || !_tcpClient.Connected)
                return false;

            if (!_tcpClient.Client.Poll(0, SelectMode.SelectWrite) || _tcpClient.Client.Poll(0, SelectMode.SelectError))
                return false;

            return true;
        }

        public void Dispose()
        {
            _tcpClient.Client.Dispose();
            ((IDisposable)_tcpClient).Dispose();
            _tcpClient = null;
        }
    }
}
