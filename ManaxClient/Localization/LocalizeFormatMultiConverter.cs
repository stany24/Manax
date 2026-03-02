using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace ManaxClient.Localization;

public class LocalizeFormatMultiConverter : IMultiValueConverter
{
    public object Convert(IList<object?>? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null || values.Count == 0)
            return "Missing key";

        if (values[0] is not string key || string.IsNullOrEmpty(key))
            return "Missing key";

        try
        {
            string localizedText = Localizer.Localizer.Get(key);
            
            if (values.Count == 1)
                return localizedText;

            object?[] formatArgs = values.Skip(1).ToArray();
            
            return string.Format(CultureInfo.InvariantCulture, localizedText, formatArgs);
        }
        catch (Exception)
        {
            return Localizer.Localizer.Get(key);
        }
    }
}