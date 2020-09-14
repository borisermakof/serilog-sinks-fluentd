using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.Fluentd
{
    public class FluentdSink : PeriodicBatchingSink, IDisposable
    {
        private readonly FluentdSinkClient _fluentdClient;

        public FluentdSink(FluentdSinkOptions options) : base(options.BatchPostingLimit, options.Period)
        {
            _fluentdClient = new FluentdSinkClient(options);
        }

        public new void Dispose()
        {
            _fluentdClient?.Dispose();
            base.Dispose();
        }

        protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            foreach (var logEvent in events)
            {
                await _fluentdClient.SendAsync(logEvent);
            }
        }
    }
}
