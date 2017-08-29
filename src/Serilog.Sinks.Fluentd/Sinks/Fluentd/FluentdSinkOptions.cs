using System;

namespace Serilog.Sinks.Fluentd
{
    public class FluentdSinkOptions
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public int ReceiveBufferSize { get; set; }
        public int SendBufferSize { get; set; }
        public bool NoDelay { get; set; }
        public int ReceiveTimeout { get; set; }
        public int SendTimeout { get; set; }
        public bool LingerEnabled { get; set; }
        public int LingerTime { get; set; }
        public bool EmitStackTraceWhenAvailable { get; set; }
        public int BatchPostingLimit { get; set; }
        public TimeSpan Period { get; set; }
        public string Tag { get; set; }
        public string MessageTemplateKey { get; set; }

        protected FluentdSinkOptions()
        {
            ReceiveBufferSize = 8192;
            SendBufferSize = 8192;
            ReceiveTimeout = 1000;
            SendTimeout = 1000;
            LingerEnabled = true;
            LingerTime = 1000;
            EmitStackTraceWhenAvailable = false;
            BatchPostingLimit = 50;
            Period = TimeSpan.FromSeconds(2);
            Tag = "Tag";
            MessageTemplateKey = "mt";
        }

        public FluentdSinkOptions(string host, int port) : this()
        {
            Host = host;
            Port = port;
        }
    }
}
