using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using Serilog.Events;

namespace Serilog.Sinks.Fluentd
{
    public class FluentdSinkClient : IDisposable
    {
        private readonly FluentdSinkOptions _options;
        private readonly TcpClient _tcpClient;
        private Stream _stream;
        private FluentdEmitter _emitter;

        public FluentdSinkClient(FluentdSinkOptions options)
        {
            _options = options;
            _tcpClient = new TcpClient();

            InitializeTcpClient();
        }

        protected void InitializeTcpClient()
        {
            _tcpClient.NoDelay = _options.NoDelay;
            _tcpClient.ReceiveBufferSize = _options.ReceiveBufferSize;
            _tcpClient.SendBufferSize = _options.SendBufferSize;
            _tcpClient.SendTimeout = _options.SendTimeout;
            _tcpClient.ReceiveTimeout = _options.SendTimeout;
            _tcpClient.LingerState = new LingerOption(_options.LingerEnabled, _options.LingerTime);
        }

        protected async void EnsureConnected()
        {
            try
            {
                if (!_tcpClient.Connected)
                {
                    await _tcpClient.ConnectAsync(_options.Host, _options.Port);
                    _stream = _tcpClient.GetStream();
                    _emitter = new FluentdEmitter(_stream);
                }
            }
            catch (Exception ex)
            {
                var e = ex;
            }
        }

        protected void Cleanup()
        {
            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }
            if (_stream != null)
            {
                _stream.Dispose();
                _stream = null;
            }
        }

        public void Send(LogEvent logEvent)
        {

            var record = new Dictionary<string, object> {
                { "Level", logEvent.Level }
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

            /*if (_options.EmitStackTraceWhenAvailable)
            {
                var transcodedFrames = new List<Dictionary<string, object>>();
                StackTrace stackTrace = logEvent.StackTrace;
                foreach (StackFrame frame in stackTrace.GetFrames())
                {
                    var transcodedFrame = new Dictionary<string, object>
                    {
                        { "filename", frame.GetFileName() },
                        { "line", frame.GetFileLineNumber() },
                        { "column", frame.GetFileColumnNumber() },
                        { "method", frame.GetMethod().ToString() },
                        { "il_offset", frame.GetILOffset() },
                        { "native_offset", frame.GetNativeOffset() },
                    };
                    transcodedFrames.Add(transcodedFrame);
                }
                record.Add("stacktrace", transcodedFrames);
            }*/
            EnsureConnected();
            if (_emitter != null)
            {
                try
                {
                    _emitter.Emit(logEvent.Timestamp.UtcDateTime, "Tag", record);
                }
                catch (Exception)
                {
                }
            }
        }

        public void Dispose()
        {
            Cleanup();
        }
    }
}
