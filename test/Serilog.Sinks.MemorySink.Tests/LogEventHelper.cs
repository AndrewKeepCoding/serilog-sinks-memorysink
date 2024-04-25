using Serilog.Events;

namespace Serilog.Sinks.MemorySink.Tests;

internal static class LogEventHelper
{
    public static LogEventLevel[] LogEventLevels { get; } = Enum.GetValues<LogEventLevel>();

    public static LogEventLevel RandomLogEventLevel => LogEventLevels[Random.Shared.Next(0, LogEventLevels.Length)];

    public static LogEvent LogEvent(LogEventLevel logEventLevel)
    {
        return new LogEvent(
            DateTimeOffset.Now,
            logEventLevel,
            exception: null,
            new MessageTemplate([]),
            properties: []);
    }

    public static IEnumerable<LogEvent> LogEvents(int count, LogEventLevel? level = null)
    {
        List<LogEvent> logEvents = [];

        for (int i = 0; i < count; i++)
        {
            LogEventLevel logEventLevel = level ?? RandomLogEventLevel;
            logEvents.Add(LogEvent(logEventLevel));
        }

        return logEvents;
    }
}
