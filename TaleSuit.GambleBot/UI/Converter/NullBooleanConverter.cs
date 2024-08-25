using System.Globalization;
using System.Windows.Data;

namespace TaleSuit.GambleBot.UI.Converter;

public class NullBooleanConverter : IValueConverter
{
    public bool Null { get; init; }
    public bool NotNull { get; init; }
    
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is null ? Null : NotNull;
    }

    public object ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}