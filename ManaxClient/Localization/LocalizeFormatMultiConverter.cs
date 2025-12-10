using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Jeek.Avalonia.Localization;

namespace ManaxClient.Localization;

public class LocalizeFormatMultiConverter(string key) : IMultiValueConverter
{
    public object Convert(IList<object?>? values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (string.IsNullOrEmpty(key))
            return "Missing key";

        try
        {
            string localizedText = Localizer.Get(key);
            object?[] formatArgs = new object[LocalizeFormatExtension.NbParameters];
            if (values == null) return string.Format(CultureInfo.InvariantCulture, localizedText, formatArgs);

            for (int i = 0; i < LocalizeFormatExtension.NbParameters; i++)
                if (i < values.Count)
                    formatArgs[i] = values[i] ?? string.Empty;
                else
                    formatArgs[i] = string.Empty;

            return string.Format(CultureInfo.InvariantCulture, localizedText, formatArgs);
        }
        catch (Exception)
        {
            return Localizer.Get(key);
        }
    }
}