# serilog-sinks-fluentd

A Sink that sends structured log events to Fluentd using MessagePack (https://msgpack.org/) format

## Usage

```C#
var options = new FluentdSinkOptions(host, port);
options.Tag = $"app.log.{AppName}";
var logConfig = new LoggerConfiguration()
  .WriteTo.Fluentd(options);
```

## Config in fluentd

```
## built-in TCP input
## @see http://docs.fluentd.org/articles/in_forward
<source>
  @type forward
  @id input_forward
</source>
```