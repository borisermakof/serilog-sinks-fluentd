using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Sinks.Fluentd.Sinks.Fluentd.Endpoints;

namespace Serilog.Sinks.Fluentd
{
    public class FluentdSinkClient : IDisposable
    {
        private readonly FluentdSinkOptions _options;
        private IEndpoint _endpoint;
        private Stream _stream;
        private FluentdEmitter _emitter;

        public FluentdSinkClient(FluentdSinkOptions options)
        {
            _options = options;
        }

        protected void InitializeEndpoint()
        {
            Cleanup();

            if(_options.UseUnixDomainSocketEndpoit)
            {
                _endpoint = new UdsEndpoint(_options);
            }
            else
            {
                _endpoint = new TcpEndpoint(_options);
            }

        }

        protected async Task EnsureConnectedAsync()
        {
            try
            {
                bool endpointInitialzied = _endpoint?.IsConnected() ?? false;

                if (endpointInitialzied) {
                    return;
                }

                InitializeEndpoint();

                await _endpoint.ConnectAsync();

                _stream = _endpoint.GetStream();
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
            if (_endpoint != null)
            {
                _endpoint.Dispose();
                _endpoint = null;
            }

            _emitter = null;
        }

        public async Task SendAsync(LogEvent logEvent, int retryCount = 1)
        {
            var record = new Dictionary<string, object>
            {
                {"Level", logEvent.Level},
                {_options.MessageTemplateKey, logEvent.MessageTemplate.Text}
            };

            foreach (var log in logEvent.Properties)
            {
                if (log.Value is SequenceValue sequenceValue)
                {
                    record.Add(log.Key, sequenceValue.Elements.Select(RenderSequenceValue).ToArray());
                }
                else
                {
                    record.Add(log.Key, log.Value.ToString());
                }
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

            await EnsureConnectedAsync();

            if (_emitter != null)
            {
                try
                {
                    _emitter.Emit(logEvent.Timestamp.UtcDateTime, _options.Tag, record);
                }
                catch (Exception ex)
                {
                    SelfLog.WriteLine($"[Serilog.Sinks.Fluentd] Send exception {ex.Message}\n{ex.StackTrace}");
                    await RetrySendAsync(logEvent, retryCount);
                }
            }
            else
            {
                await RetrySendAsync(logEvent, retryCount);
            }
        }

        private static object RenderSequenceValue(LogEventPropertyValue x) => (x as ScalarValue)?.Value ?? x.ToString();

        private async Task RetrySendAsync(LogEvent logEvent, int retryCount)
        {
            if (retryCount < _options.RetryCount)
            {
                Thread.Sleep(_options.RetryDelay);
                SelfLog.WriteLine($"[Serilog.Sinks.Fluentd] Retry send {retryCount + 1}");
                await SendAsync(logEvent, retryCount + 1);
            }
            else
            {
                SelfLog.WriteLine(
                    $"[Serilog.Sinks.Fluentd] Retry count has exceeded limit {_options.RetryCount}. Giving up. Data will be lost");
            }
        }

        public void Dispose()
        {
            Cleanup();
        }
    }
}