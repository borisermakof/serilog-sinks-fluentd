using Serilog.Configuration;
using Serilog.Events;
using Serilog.Sinks.Fluentd;
using System;

namespace Serilog
{
    public static class LoggerConfigurationFluentdExtensions
    {
        private const string Host = "localhost";
        private const int Port = 24224;

        /// <summary>
        /// Configures logging to Fluentd.
        /// </summary>
        /// <param name="loggerSinkConfiguration"></param>
        /// <param name="option">If null, set to default of localhost and port 24224</param>
        /// <param name="restrictedToMinimumLevel"></param>
        /// <returns></returns>
        public static LoggerConfiguration Fluentd(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            FluentdSinkOptions option = null,
            LogEventLevel restrictedToMinimumLevel = LogEventLevel.Information)
        {
            if(option!=null)
            {
                if (string.IsNullOrWhiteSpace(option.Host))
                    throw new ArgumentException("Host value must be set", "option.Host");
                if (option.Port==0)
                    throw new ArgumentException("Port value must be set to positve integer", "option.Port");
            }

            var sink = new FluentdSink(option ?? new FluentdSinkOptions(Host, Port));
            return loggerSinkConfiguration.Sink(sink, restrictedToMinimumLevel);
        }

        /// <summary>
        ///  Configures logging to Fluentd.
        /// </summary>
        /// <param name="loggerSinkConfiguration"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="restrictedToMinimumLevel"></param>
        /// <returns></returns>
        public static LoggerConfiguration Fluentd(
           this LoggerSinkConfiguration loggerSinkConfiguration,
           string host,
           int port,
           LogEventLevel restrictedToMinimumLevel = LogEventLevel.Debug)
        {
            return Fluentd(
                loggerSinkConfiguration,
                new FluentdSinkOptions(host, port),
                restrictedToMinimumLevel);
        }
    }
}
