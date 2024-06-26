﻿namespace Serilog.Sinks.MemorySink;

public interface ILogSource<T>
{
    void Initialize();

    int GetLogsCount();

    Task<IEnumerable<T>> GetLogs(int start, int requiredCount = int.MaxValue, CancellationToken cancellationToken = default);

    Task ClearLogs(CancellationToken cancellationToken = default);
}
