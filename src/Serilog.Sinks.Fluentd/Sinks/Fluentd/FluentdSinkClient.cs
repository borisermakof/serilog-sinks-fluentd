using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using Serilog.Debugging;
using Serilog.Events;

namespace Serilog.Sinks.Fluentd
{
    public class FluentdSinkClient : IDisposable
    {
        private readonly FluentdSinkOptions _options;
        private TcpClient _tcpClient;
        private Stream _stream;
        private FluentdEmitter _emitter;

        public FluentdSinkClient(FluentdSinkOptions options)
        {
            _options = options;
        }

        protected void InitializeTcpClient()
        {
            Cleanup();

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

        protected async void EnsureConnected()
        {
            try
            {
                if (IsConnected()) return;

                InitializeTcpClient();

                await _tcpClient.ConnectAsync(_options.Host, _options.Port);
                _stream = _tcpClient.GetStream();
                _emitter = new FluentdEmitter(_stream);
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine($"[Serilog.Sinks.Fluentd] Connection exception {ex.Message}\n{ex.StackTrace}");
            }
        }

        protected void Cleanup()
        {
            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }
            if (_tcpClient != null)
            {
                _tcpClient.Client.Dispose();
                _tcpClient = null;
            }

            _emitter = null;
        }

        public void Send(LogEvent logEvent, int retryCount = 1)
        {

            var record = new Dictionary<string, object> {
                { "Level", logEvent.Level },
                { _options.MessageTemplateKey, logEvent.MessageTemplate.Text }
            };

            foreach (var log in logEvent.Properties)
            {
                record.Add(log.Key, log.Value.ToString());
            }

            if (logEvent.Exception != null)
            {
                var exception = logEvent.Exception;
                var errorFormatted = new Dictionary<string, object>
                {
                    {"ExceptionMessage", exception.Message},
                    {"ExceptionSource", exception.Source},
                    {"ExceptionStackTrace", exception.StackTrace}
                };
                record.Add("Exception", errorFormatted);
            }

            EnsureConnected();
            if (_emitter != null)
            {
                try
                {
                    _emitter.Emit(logEvent.Timestamp.UtcDateTime, _options.Tag, record);
                }
                catch (Exception ex)
                {
                    SelfLog.WriteLine($"[Serilog.Sinks.Fluentd] Send exception {ex.Message}\n{ex.StackTrace}");
                    RetrySend(logEvent, retryCount);
                }
            }
            else
            {
                RetrySend(logEvent, retryCount);
            }
        }

        private void RetrySend(LogEvent logEvent, int retryCount)
        {
            if (retryCount < _options.RetryCount)
            {
                Thread.Sleep(_options.RetryDelay);
                SelfLog.WriteLine($"[Serilog.Sinks.Fluentd] Retry send {retryCount + 1}");
                Send(logEvent, retryCount + 1);
            }
            else
            {
                SelfLog.WriteLine($"[Serilog.Sinks.Fluentd] Retry count has exceeded limit {_options.RetryCount}. Giving up. Data will be lost");
            }
        }

        public void Dispose()
        {
            Cleanup();
        }

        protected bool IsConnected()
        {
            if (_tcpClient == null || !_tcpClient.Connected)
                return false;

            if (!_tcpClient.Client.Poll(0, SelectMode.SelectWrite) || _tcpClient.Client.Poll(0, SelectMode.SelectError))
                return false;

            return true;
        }
    }
}
