using FluentAssertions;
using Serilog.Events;

namespace Serilog.Sinks.MemorySink.Tests;

public class MemorySinkTests
{
    private readonly MemorySinkOptions<LogEvent> _options = new()
    {
        ProcessingInterval = TimeSpan.FromMilliseconds(10)
    };

    public record GetLogsTestItem(IEnumerable<LogEvent> Input, int Start, int Count, bool ClearLogsFirst, IEnumerable<LogEvent> ExpectedOutput);

    public static IEnumerable<LogEvent> TestLogEvents { get; } = LogEventHelper.LogEvents(10, LogEventLevel.Information);

    public static IEnumerable<object[]> GetLogsTestItems { get; } =
    [
        [new GetLogsTestItem(TestLogEvents.Take(1), Start: 0, Count: 0, ClearLogsFirst: false, TestLogEvents.Take(0))],
        [new GetLogsTestItem(TestLogEvents.Take(1), Start: 0, Count: 1, ClearLogsFirst: false, TestLogEvents.Take(1))],
        [new GetLogsTestItem(TestLogEvents.Take(1), Start: 0, Count: 2, ClearLogsFirst: false, TestLogEvents.Take(1))],
        [new GetLogsTestItem(TestLogEvents.Take(1), Start: 1, Count: 1, ClearLogsFirst: false, TestLogEvents.Take(0))],

        [new GetLogsTestItem(TestLogEvents.Take(2), Start: 0, Count: 1, ClearLogsFirst: false, TestLogEvents.Take(1))],
        [new GetLogsTestItem(TestLogEvents.Take(2), Start: 0, Count: 2, ClearLogsFirst: false, TestLogEvents.Take(2))],
        [new GetLogsTestItem(TestLogEvents.Take(2), Start: 0, Count: 3, ClearLogsFirst: false, TestLogEvents.Take(2))],
        [new GetLogsTestItem(TestLogEvents.Take(2), Start: 1, Count: 1, ClearLogsFirst: false, TestLogEvents.Skip(1).Take(1)) ],
        [new GetLogsTestItem(TestLogEvents.Take(2), Start: 1, Count: 2, ClearLogsFirst: false, TestLogEvents.Skip(1).Take(1))],

        [new GetLogsTestItem(TestLogEvents.Take(1), Start: 0, Count: 1, ClearLogsFirst: true, TestLogEvents.Take(0))],
        [new GetLogsTestItem(TestLogEvents.Take(2), Start: 0, Count: 1, ClearLogsFirst: true, TestLogEvents.Take(0))],
    ];

    [Fact]
    public async Task GetLogs_WhenLogsCountIsZero_ShouldReturnEmptyCollection()
    {
        // Arrange
        var sut = new MemorySink<LogEvent>(_options);
        sut.Initialize();
        await Task.Delay(_options.ProcessingInterval * 2);
        sut.GetLogsCount().Should().Be(0);

        // Act
        var logs = await sut.GetLogs();

        // Assert
        logs.Should().BeEmpty();
    }

    [Fact]
    public async Task GetLogs_WhenLogsAreAdded_ShouldReturnAddedLogs()
    {
        // Arrange
        var sut = new MemorySink<LogEvent>(_options);
        sut.Initialize();
        var logEvent1 = LogEventHelper.LogEvent(LogEventLevel.Information);
        sut.Emit(logEvent1);
        var logEvent2 = LogEventHelper.LogEvent(LogEventLevel.Information);
        sut.Emit(logEvent2);
        await Task.Delay(_options.ProcessingInterval * 2);
        sut.GetLogsCount().Should().Be(2);

        // Act
        var logs = await sut.GetLogs();

        // Assert
        logs.ElementAt(0).Should().BeEquivalentTo(logEvent1);
        logs.ElementAt(1).Should().BeEquivalentTo(logEvent2);
    }

    [Theory]
    [MemberData(nameof(GetLogsTestItems))]
    public async Task GetLogs_WhenGivenSpecificStartIndexAndCount_ShouldReturnSpecifiedLogs(GetLogsTestItem item)
    {
        // Arrange
        var sut = new MemorySink<LogEvent>(_options);
        sut.Initialize();

        foreach (var logEvent in item.Input)
        {
            sut.Emit(logEvent);
        }

        await Task.Delay(TimeSpan.FromMilliseconds(100));

        // Act
        if (item.ClearLogsFirst is true)
        {
            await sut.ClearLogs();
        }

        var logs = await sut.GetLogs(item.Start, item.Count);

        // Assert
        logs.Should().BeEquivalentTo(item.ExpectedOutput);
    }

    [Fact]
    public async Task ClearLogs_WhenLogsCountIsZero_ShouldClearLogs()
    {
        // Arrange
        var sut = new MemorySink<LogEvent>(_options);

        // Act
        await sut.ClearLogs();

        // Assert
        sut.GetLogsCount().Should().Be(0);
    }

    [Fact]
    public async Task ClearLogs_WhenLogsCountIsGreaterThanZero_ShouldClearLogs()
    {
        // Arrange
        var sut = new MemorySink<LogEvent>(_options);
        sut.Initialize();
        sut.Emit(LogEventHelper.LogEvent(LogEventLevel.Information));
        await Task.Delay(_options.ProcessingInterval * 2);
        sut.GetLogsCount().Should().Be(1);

        // Act
        await sut.ClearLogs();

        // Assert
        sut.GetLogsCount().Should().Be(0);
    }
}
