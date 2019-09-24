using System;
using System.IO;
using System.Threading.Tasks;

namespace Serilog.Sinks.Fluentd.Sinks.Fluentd.Endpoints
{
    interface IEndpoint : IDisposable
    {
        Stream GetStream();
        Task ConnectAsync();
        bool IsConnected();
    }
}
