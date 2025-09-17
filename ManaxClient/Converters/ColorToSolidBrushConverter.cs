using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ManaxClient.Converters;

public class ColorToSolidBrushConverter:IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            null => new SolidColorBrush(Colors.Transparent),
            Color color => new SolidColorBrush(color),
            System.Drawing.Color color1 => new SolidColorBrush(Color.FromArgb(color1.A, color1.R, color1.G, color1.B)),
            _ => new SolidColorBrush(Colors.Transparent)
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}