using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TrainerBot.Converter;

public class BoolToVisibilityConverter : IValueConverter
{
    public Visibility True { get; init; }
    public Visibility False { get; init; }
    
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            true => True,
            false => False,
            _ => throw new InvalidOperationException()
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}