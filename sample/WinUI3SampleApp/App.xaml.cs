using Microsoft.UI.Xaml;
using Serilog;
using Serilog.Core;
using Serilog.Sinks.MemorySink;

namespace WinUI3SampleApp;

public partial class App : Application
{
    private Window? _window;

    public App()
    {
        this.InitializeComponent();

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(LoggingLevelSwitch)
            .WriteTo.MemorySink(
                out ILogSource<LogItem> logSource,
                options =>
                {
                    options.LogEventConverter = logEvent =>
                    {
                        return new LogItem(logEvent.Timestamp, logEvent.Level, logEvent.MessageTemplate.Text);
                    };
                    options.MaxLogsCount = 100_000;
                    options.OnException = ex =>
                    {
                        // Handle exception
                    };
                })
                .CreateLogger();

        LogSource = logSource;
    }

    //public static ILogSource<LogEvent> LogSource { get; set; }
    public static ILogSource<LogItem> LogSource { get; set; } = new NullLogSource<LogItem>();

    public static LoggingLevelSwitch LoggingLevelSwitch { get; } = new();

    protected override void OnLaunched(LaunchActivatedEventArgs _)
    {
        _window = new MainWindow();
        _window.Activate();
    }
}
