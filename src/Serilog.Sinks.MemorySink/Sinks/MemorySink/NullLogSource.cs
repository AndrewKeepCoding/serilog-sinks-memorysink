namespace Serilog.Sinks.MemorySink;

public class NullLogSource<T> : ILogSource<T>
{
    public void Initialize() {}

    public int GetLogsCount() => 0;

    public Task<IEnumerable<T>> GetLogs(int start, int requiredCount = int.MaxValue, CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<T>>([]);

    public Task ClearLogs(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
