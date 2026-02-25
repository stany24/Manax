using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ManaxClient.Converters;

public static class VisibilityConverters
{
    public static readonly IMultiValueConverter And = new AndConverter();
    public static readonly IMultiValueConverter IssuePageVisible = new IssuePageVisibleConverter();
}

public class AndConverter : IMultiValueConverter
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

public class IssuePageVisibleConverter : IMultiValueConverter
{
    public object Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count != 3)
            return false;

        if (values[0] is not bool automaticIssueEnabled)
            return false;
        if (values[1] is not bool reportedIssueEnabled)
            return false;
        if (values[2] is not bool canReadIssues)
            return false;

        return (automaticIssueEnabled || reportedIssueEnabled) && canReadIssues;
    }
}