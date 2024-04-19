using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Serilog.Events;

namespace WinUI3SampleApp;

public class LogItemDataTemplateSelector : DataTemplateSelector
{
    public DataTemplate? VerboseTemplate { get; set; }

    public DataTemplate? DebugTemplate { get; set; }

    public DataTemplate? InformationTemplate { get; set; }

    public DataTemplate? WarningTemplate { get; set; }

    public DataTemplate? ErrorTemplate { get; set; }

    public DataTemplate? FatalTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        var template = (item as LogItem)?.Level switch
        {
            LogEventLevel.Verbose => VerboseTemplate,
            LogEventLevel.Debug => DebugTemplate,
            LogEventLevel.Information => InformationTemplate,
            LogEventLevel.Warning => WarningTemplate,
            LogEventLevel.Error => ErrorTemplate,
            LogEventLevel.Fatal => FatalTemplate,
            _ => base.SelectTemplateCore(item),
        };

        return template ?? base.SelectTemplateCore(item);
    }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        var template = (item as LogItem)?.Level switch
        {
            LogEventLevel.Verbose => VerboseTemplate,
            LogEventLevel.Debug => DebugTemplate,
            LogEventLevel.Information => InformationTemplate,
            LogEventLevel.Warning => WarningTemplate,
            LogEventLevel.Error => ErrorTemplate,
            LogEventLevel.Fatal => FatalTemplate,
            _ => base.SelectTemplateCore(item),
        };

        return template ?? base.SelectTemplateCore(item);
    }
}
