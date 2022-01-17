using System.Collections.Generic;
using MessagePack;

namespace Serilog.Sinks.Fluentd
{
    [MessagePackObject]
    public class FluentdMessage
    {
        [Key(0)]
        public string Tag { get; set; }

        [Key(1)]
        public ulong Timestamp { get; set; }

        [Key(2)]
        public IDictionary<string, object> Data { get; set; }
    }
}
