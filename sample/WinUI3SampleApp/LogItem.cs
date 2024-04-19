using Serilog.Events;
using System;

namespace WinUI3SampleApp;

public record LogItem(DateTimeOffset Timestamp, LogEventLevel Level, string Message);
