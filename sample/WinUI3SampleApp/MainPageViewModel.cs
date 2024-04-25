using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WinUI3SampleApp;

public partial class MainPageViewModel : ObservableObject
{
    [RelayCommand(IncludeCancelCommand = true)]
    private async Task LoggingDemo(CancellationToken cancellationToken)
    {
        var tasks = new List<Task>();

        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                return KeepLogging(TimeSpan.FromMilliseconds(100), cancellationToken);
            }, cancellationToken));
        }

        await Task.WhenAll(tasks);
    }

    private static async Task KeepLogging(TimeSpan interval, CancellationToken cancellationToken)
    {
        int threadId = Environment.CurrentManagedThreadId;

        try
        {
            using var periodicTimer = new PeriodicTimer(interval);

            while (await periodicTimer.WaitForNextTickAsync(cancellationToken) is true)
            {
                int value = Random.Shared.Next(1, 100);
                string message = $"This message was created by thread {threadId} with the random value {value}.";

                switch (value)
                {
                    case > 0 and <= 30:
                        Log.Verbose(message);
                        break;
                    case > 30 and <= 50:
                        Log.Debug(message);
                        break;
                    case > 50 and <= 90:
                        Log.Information(message);
                        break;
                    case > 90 and <= 95:
                        Log.Warning(message);
                        break;
                    case > 95 and <= 98:
                        Log.Error(message);
                        break;
                    default:
                        Log.Fatal(message);
                        break;
                }

                cancellationToken.ThrowIfCancellationRequested();
            }
        }
        catch (OperationCanceledException)
        {
            Log.Information($"Thread {threadId}: Logging demo task was cancelled.");
        }
    }
}
