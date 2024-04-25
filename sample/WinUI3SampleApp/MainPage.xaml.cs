using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WinUI3SampleApp;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        InitializeComponent();
        MinimumLevelComboBox.SelectedItem = LogEventLevel.Information;
    }

    public LogEventLevel[] LogEventLevels { get; } = Enum.GetValues<LogEventLevel>();

    private ObservableCollection<LogItem> LogEvents { get; } = [];

    private MainPageViewModel ViewModel { get; } = new();

    private CancellationTokenSource? LogViewerUpdateCancelllationTokenSource { get; set; }

    private async void UpdateLogViewerToggleSwitch_Toggled(object sender, RoutedEventArgs _)
    {
        if (sender is not ToggleSwitch toggleSwitch)
        {
            return;
        }

        if (toggleSwitch.IsOn is true)
        {
            LogViewerUpdateCancelllationTokenSource = new();
            await KeepFetchingLogs(LogViewerUpdateCancelllationTokenSource.Token);
        }
        else
        {
            LogViewerUpdateCancelllationTokenSource?.Cancel();
            LogViewerUpdateCancelllationTokenSource?.Dispose();
            LogViewerUpdateCancelllationTokenSource = null;
        }
    }

    private async Task KeepFetchingLogs(CancellationToken cancellationToken)
    {
        try
        {
            using var periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));

            while (await periodicTimer.WaitForNextTickAsync(cancellationToken) is true)
            {
                int startIndex = LogEvents.Count;
                var fetchedLogs = await App.LogSource.GetLogs(startIndex, 10_000, cancellationToken);

                if (fetchedLogs.Any() is false)
                {
                    continue;
                }

                foreach (var log in fetchedLogs)
                {
                    LogEvents.Add(log);
                }

                if (AutoScrollToggleSwitch.IsOn is true &&
                    LogEvents.LastOrDefault() is { } logEvent)
                {
                    LogEventsListView.ScrollIntoView(logEvent);
                }
            }
        }
        catch (OperationCanceledException)
        {
            Log.Information("Log viewer update task was cancelled.");
        }
        catch (Exception exception)
        {
            Log.Error(exception, "An error occurred while fetching logs.");
        }
    }

    private void MinimumLevelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs _)
    {
        if ((sender as ComboBox)?.SelectedItem is not LogEventLevel selectedLogLevel)
        {
            return;
        }

        App.LoggingLevelSwitch.MinimumLevel = selectedLogLevel;
    }

    private async void ClearLogsButton_Click(object sender, RoutedEventArgs e)
    {
        LogEvents.Clear();
        await App.LogSource.ClearLogs();
    }
}
