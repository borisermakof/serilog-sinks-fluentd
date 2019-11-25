using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.Fluentd;

namespace Serilog
{
    public static class LoggerConfigurationFluentdExtensions
    {
        private const string Host = "localhost";
        private const int Port = 24224;
        private const string Tag = "Tag";

        public static LoggerConfiguration Fluentd(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            FluentdSinkOptions option = null,
            LogEventLevel restrictedToMinimumLevel = LogEventLevel.Information)
        {
            var sink = new FluentdSink(option ?? new FluentdSinkOptions(Host, Port, Tag));

            return loggerSinkConfiguration.Sink(sink, restrictedToMinimumLevel);
        }

        public static LoggerConfiguration Fluentd(
           this LoggerSinkConfiguration loggerSinkConfiguration,
           string host,
           int port,
           string tag = Tag,
           LogEventLevel restrictedToMinimumLevel = LogEventLevel.Information)
        {
            var sink = new FluentdSink(new FluentdSinkOptions(host, port, tag));

            return loggerSinkConfiguration.Sink(sink, restrictedToMinimumLevel);
        }

        public static LoggerConfiguration Fluentd(
           this LoggerSinkConfiguration loggerSinkConfiguration,
           string udsSocketFilePath,
           LogEventLevel restrictedToMinimumLevel = LogEventLevel.Information)
        {
            var sink = new FluentdSink(new FluentdSinkOptions(udsSocketFilePath));

            return loggerSinkConfiguration.Sink(sink, restrictedToMinimumLevel);
        }
    }
}
