using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ManaxClient.Converters.Visibility;

public class IssuePageVisibilityConverter : IMultiValueConverter
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