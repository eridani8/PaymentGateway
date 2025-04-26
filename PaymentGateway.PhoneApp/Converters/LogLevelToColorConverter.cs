using System.Globalization;
using Serilog.Events;

namespace PaymentGateway.PhoneApp.Converters;

public class LogLevelToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is LogEventLevel level)
        {
            return level switch
            {
                LogEventLevel.Error or LogEventLevel.Fatal => Application.Current?.Resources["Error"],
                LogEventLevel.Warning => Application.Current?.Resources["Warning"],
                LogEventLevel.Information => Application.Current?.Resources["Gray300"],
                LogEventLevel.Debug => Application.Current?.Resources["Gray500"],
                LogEventLevel.Verbose => Application.Current?.Resources["Gray600"],
                _ => Application.Current?.Resources["LightOnDarkBackground"]
            };
        }

        return Application.Current?.Resources["Gray300"];
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 