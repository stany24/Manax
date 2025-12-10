using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ManaxClient.Converters;

public class ColorToSolidBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            null => new SolidColorBrush(Colors.Transparent),
            Color avaloniaColor => new SolidColorBrush(avaloniaColor),
            System.Drawing.Color systemColor => new SolidColorBrush(Color.FromArgb(systemColor.A, systemColor.R, systemColor.G, systemColor.B)),
            _ => new SolidColorBrush(Colors.Transparent)
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}