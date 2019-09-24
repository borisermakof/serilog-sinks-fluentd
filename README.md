# serilog-sinks-fluentd
A Sink that writes logs into Fluentd

## Installation
This sink is distributed via NuGet package. Install it by running
```
Install-Package Serilog.Sinks.Fluentd
``` 

## Usage
The sink lives in `Serilog.Sinks.Fluentd` namespace. Include it into your program and then simply add the `Fluentd` sink to your `LoggerConfiguration` object:
```
using Serilog;
using Serilog.Sinks.Fluentd;

var log = new LoggerConfiguration().WriteTo.Fluentd().CreateLogger();
log.Information($"That's it!");
```
When no arguments are provided to the sink it will emit only events with  Informational level and above via TCP protocol to the `localhost:24224`.


## Configuration
The sink provides three configuration options:

* `Fluentd(string host, int port, LogEventLevel restrictedToMinimumLevel = Debug)` will cause sink to emit events to the specified host and port via TCP. Optional `restrictedToMinimumLevel` parameter is used to override the default minimum level filter applied by the sink.

* `Fluentd(string udsSocketFilePath, LogEventLevel restrictedToMinimumLevel = Debug)` will cause sink to emit events into the specified UNIX domain socket file. Optional `restrictedToMinimumLevel` parameter is used to override the default minimum level filter applied by the sink.

* `Fluentd(FluentdSinkOptions option = null)` will cause sink to emit events to the endpoint (either TCP address or UNIX domain socket file) defined in the `FluentdSinkOptions` object `option`. If no options object is provided, events will be emitted to the `localhost:24224` via TCP. *Please note that when sink is configured like this, only events with level Informational and above will be emitted. This behaviour cannot be overriden.*

## Examples
### Creating a logger that will emit events via TCP
```
var log = new LoggerConfiguration().WriteTo.Fluentd("localhost", 9100).CreateLogger();
```

### Creating a logger that will emit events into a UNIX domain socket file
```
var log = new LoggerConfiguration().WriteTo.Fluentd("/var/run/fluentd/fluentd.sock").CreateLogger();
```

### Creating a logger that will emit Verbose-level events (and above) into a UNIX domain socket file
```
using Serilog.Events; // exposes LogEventLevel

var log = new LoggerConfiguration().MinimumLevel.Verbose().WriteTo.Fluentd("/var/run/fluentd/fluentd.sock", LogEventLevel.Verbose).CreateLogger();
```

### Creating a UNIX domain sockets file logger with sinkOptions object
```
FluentdSinkOptions sinkOptions = new FluentdSinkOptions("/var/run/fluentd/fluentd.sock");
var log = new LoggerConfiguration().WriteTo.Fluentd(sinkOptions).CreateLogger();
```