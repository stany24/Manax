using Avalonia.Data.Converters;

namespace ManaxClient.Converters.Visibility;

public static class VisibilityConverters
{
    public static readonly IMultiValueConverter And = new AndVisibilityConverter();
    public static readonly IMultiValueConverter IssuePageVisible = new IssuePageVisibilityConverter();
}