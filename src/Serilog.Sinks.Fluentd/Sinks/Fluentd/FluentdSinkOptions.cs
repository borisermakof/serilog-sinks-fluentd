using System;
using System.Globalization;

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
        public string MessageKey { get; set; }
        public IFormatProvider FormatProvider { get; set; }
        public bool UseUnixDomainSocketEndpoit { get; set; }
        public string UdsSocketFilePath { get; set; }

        /// <summary>
        /// In case of network related problems, try that amount of times to send message
        /// </summary>
        public int RetryCount { get; set; }
        /// <summary>
        /// In case of network related problems, this is a delay between attempts
        /// </summary>
        public TimeSpan RetryDelay { get; set; }

        protected FluentdSinkOptions()
        {
            Host = String.Empty;
            Port = 0;
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
            MessageKey = "m";
            FormatProvider = CultureInfo.InvariantCulture;
            UseUnixDomainSocketEndpoit = false;
            UdsSocketFilePath = String.Empty;

            RetryCount = 10;
            RetryDelay = TimeSpan.FromSeconds(1);
        }

        public FluentdSinkOptions(string host, int port, string tag = "") : this()
        {
            Host = host;
            Port = port;
            Tag = tag;
        }

        public FluentdSinkOptions(string udsSocketFilePath) : this()
        {
            UseUnixDomainSocketEndpoit = true;
            UdsSocketFilePath = udsSocketFilePath;
        }
    }
}
