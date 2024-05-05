using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Serilog.Events;
using System;

namespace WinUI3SampleApp;

public class LogEventLevelToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return (LogEventLevel)value switch
        {
            LogEventLevel.Verbose => new SolidColorBrush(Colors.LightGray),
            LogEventLevel.Debug => new SolidColorBrush(Colors.Gray),
            LogEventLevel.Warning => new SolidColorBrush(Colors.Yellow),
            LogEventLevel.Error => new SolidColorBrush(Colors.HotPink),
            LogEventLevel.Fatal => new SolidColorBrush(Colors.Red),
            LogEventLevel.Information or _ => value,
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}