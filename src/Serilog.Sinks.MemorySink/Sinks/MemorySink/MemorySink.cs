using AsyncAwaitBestPractices;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using System.Diagnostics;
using System.Threading.Channels;

namespace Serilog.Sinks.MemorySink;

internal sealed class MemorySink<T>(MemorySinkOptions<T> options) : ILogEventSink, ILogSource<T>, IDisposable
{
    private readonly MemorySinkOptions<T> _options = options;

    private readonly Channel<LogEvent> _channel = Channel.CreateUnbounded<LogEvent>();

    private readonly SemaphoreSlim _semaphore = new(initialCount: 1);

    private Task UpdatingTask { get; set; } = Task.CompletedTask;

    private CancellationTokenSource CancellationTokenSource { get; set; } = new();

    private List<T> LogCollection { get; } = [];

    private Queue<T> LogQueue { get; } = new();

    private int LogCollectionFirstElementIndex { get; set; } = 0;

    public void Emit(LogEvent logEvent)
    {
        if (_channel.Writer.TryWrite(logEvent) is false)
        {
            SelfLog.WriteLine($"{nameof(MemorySink<T>)} failed to write to channel: {logEvent.RenderMessage()}");
        }
    }

    public void Initialize()
    {
        CancellationTokenSource = new CancellationTokenSource();
        CancellationToken cancellationToken = CancellationTokenSource.Token;
        UpdatingTask = ProcessLogs(cancellationToken);
        UpdatingTask.SafeFireAndForget(_options.OnException);
    }

    public int GetLogsCount()
    {
        return LogCollection.Count;
    }

    public async Task<IEnumerable<T>> GetLogs(int start = 0, int count = int.MaxValue, CancellationToken cancellationToken = default)
    {
        if (LogCollection.Count is 0)
        {
            return [];
        }

        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            int startIndex = start - LogCollectionFirstElementIndex;

            if (startIndex < 0)
            {
                return [];
            }

            return LogCollection
                .Skip(startIndex)
                .Take(count)
                .ToList();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async Task ClearLogs(CancellationToken cancellationToken = default)
    {
        if (LogCollection.Count > 0)
        {
            return;
        }

        try
        {
            await _semaphore.WaitAsync(cancellationToken);
            LogCollectionFirstElementIndex = 0;
            LogCollection.Clear();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private void Dispose(bool disposing)
    {
        if (disposing is true)
        {
            _channel.Writer.Complete();
            CancellationTokenSource.Cancel();
            CancellationTokenSource.Dispose();
        }
    }

    private async Task ProcessLogs(CancellationToken cancellationToken)
    {
        try
        {
            var logEventsBatch = new List<LogEvent>();
            var stopwatch = new Stopwatch();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                logEventsBatch.Clear();
                stopwatch.Restart();

                for (int i = 0; i < _options.MaxBatchSize; i++)
                {
                    if (_channel.Reader.TryRead(out LogEvent? logEvent) is false ||
                        logEvent is null)
                    {
                        break;
                    }

                    logEventsBatch.Add(logEvent);

                    if (cancellationToken.IsCancellationRequested is true ||
                        stopwatch.ElapsedMilliseconds > _options.ProcessingInterval.Milliseconds)
                    {
                        break;
                    }
                }

                if (logEventsBatch.Count is 0)
                {
                    TimeSpan waitingTime = _options.ProcessingInterval - stopwatch.Elapsed;

                    if (waitingTime > TimeSpan.Zero)
                    {
                        await Task.Delay(waitingTime, cancellationToken);
                    }

                    continue;
                }

                AddLogs(logEventsBatch, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            SelfLog.WriteLine($"{nameof(MemorySink<T>)} was canceled.");
        }
        catch (Exception exception)
        {
            SelfLog.WriteLine($"{nameof(MemorySink<T>)} failed to update collection: {exception.Message}");
            throw;
        }
    }

    private void AddLogs(IList<LogEvent> logEvents, CancellationToken cancellationToken)
    {
        try
        {
            if (logEvents.Any() is false)
            {
                return;
            }

            _semaphore.Wait(cancellationToken);

            IEnumerable<T> logs = _options.LogEventConverter is not null
                ? logEvents.Select(logEvent => _options.LogEventConverter(logEvent))
                : logEvents.Cast<T>();

            int removingCount = LogCollection.Count + logs.Count() - _options.MaxLogsCount;

            if (removingCount > 0)
            {
                LogCollectionFirstElementIndex += removingCount;
                LogCollection.RemoveRange(index: 0, removingCount);
            }

            LogCollection.AddRange(logs);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
