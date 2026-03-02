using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ManaxClient.Converters.Visibility;

public class AndVisibilityConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        foreach (object? value in values)
            switch (value)
            {
                case bool boolValue:
                {
                    if (!boolValue) return false;
                    break;
                }
                case int intValue:
                {
                    if (intValue == 0) return false;
                    break;
                }
                default:
                    return false;
            }

        return true;
    }
}