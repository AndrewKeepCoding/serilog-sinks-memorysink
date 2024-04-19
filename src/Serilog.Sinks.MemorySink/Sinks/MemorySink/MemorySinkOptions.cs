using Serilog.Events;

namespace Serilog.Sinks.MemorySink;

public sealed class MemorySinkOptions<T>
{
    public int MaxLogsCount { get; set; } = int.MaxValue;

    public TimeSpan ProcessingInterval { get; set; } = TimeSpan.FromMilliseconds(100);

    public int MaxBatchSize { get; set; } = 100;

    public Func<LogEvent, T>? LogEventConverter { get; set; }

    public Action<Exception>? OnException { get; set; }
}
