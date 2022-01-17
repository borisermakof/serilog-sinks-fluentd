using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                {"Level", logEvent?.Level.ToString()},
                {_options.MessageTemplateKey, logEvent.MessageTemplate.Text},
                {_options.MessageKey, logEvent.MessageTemplate.Render(logEvent.Properties, _options.FormatProvider)}
            };

            foreach (var log in logEvent.Properties)
            {
                record.Add(log.Key, GetRenderedProperty(log.Value));
            }

            if (logEvent.Exception != null)
            {
                var exception = logEvent.Exception;
                var errorFormatted = new Dictionary<string, object>
                {
                    {"Type", exception.GetType().FullName},
                    {"Message", exception.Message},
                    {"Source", exception.Source},
                    {"StackTrace", exception.StackTrace},
                    {"Details", exception.ToString()}
                };
                record.Add("Exception", errorFormatted);
            }

            await EnsureConnectedAsync();

            if (_emitter != null)
            {
                try
                {
                    await _emitter.EmitAsync(logEvent.Timestamp.UtcDateTime, _options.Tag, record);
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

        private object GetRenderedProperty(LogEventPropertyValue value)
        {
            switch (value)
            {
                case SequenceValue sequenceValue:
                    return sequenceValue.Elements.Select(GetRenderedSequenceValue).ToArray();
                case StructureValue structureValue:
                    return structureValue.Properties
                        .ToDictionary(x => x.Name, x => GetRenderedProperty(x.Value));
                case ScalarValue scalarValue:
                    switch (scalarValue.Value)
                    {
                        case null:
                            return null;
                        case string str:
                            return str;
                        case DateTime dt:
                            return dt.ToString("o");
                        case DateTimeOffset dt:
                            return dt.ToString("o");
                        case var numericScalar when numericScalar.GetType().IsNumericType():
                            return numericScalar;
                        case var unknownScalar:
                            return string.Format(_options.FormatProvider, "{0}", unknownScalar);
                    }
                default:
                    return value.ToString(null, _options.FormatProvider);
            }
        }

        private object GetRenderedSequenceValue(LogEventPropertyValue value) => 
            (value as ScalarValue)?.Value ?? value.ToString(null, _options.FormatProvider);

        private async Task RetrySendAsync(LogEvent logEvent, int retryCount)
        {
            if (retryCount < _options.RetryCount)
            {
                await Task.Delay(_options.RetryDelay);
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