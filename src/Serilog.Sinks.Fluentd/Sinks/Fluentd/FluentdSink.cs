using System.Collections.Generic;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;

namespace Serilog.Sinks.Fluentd
{
    public class FluentdSink : PeriodicBatchingSink
    {
        private readonly FluentdSinkClient _fluentdClient;

        public FluentdSink(FluentdSinkOptions options) : base(options.BatchPostingLimit, options.Period)
        {
            _fluentdClient = new FluentdSinkClient(options);
        }

        protected override void EmitBatch(IEnumerable<LogEvent> events)
        {
            foreach (var logEvent in events)
                _fluentdClient.Send(logEvent);
        }
    }
}
