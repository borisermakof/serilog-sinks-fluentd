using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MessagePack;

namespace Serilog.Sinks.Fluentd
{
    internal class FluentdEmitter
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private readonly Stream _output;

        public FluentdEmitter(Stream stream)
        {
            this._output = stream;
        }

        public async Task EmitAsync(DateTime timestamp, string tag, IDictionary<string, object> data)
        {
            await MessagePackSerializer.SerializeAsync(
                this._output,
                new FluentdMessage
                {
                    Tag = tag,
                    Timestamp = (ulong)timestamp.ToUniversalTime().Subtract(UnixEpoch).Ticks / 10000000,
                    Data = data,
                });
        }
    }
}
