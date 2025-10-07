using Avalonia.Media;

namespace ManaxClient.Models.Theme;

public class Theme(string name, SolidColorBrush primary, SolidColorBrush secondary, SolidColorBrush tertiary)
{
    public string Name { get; set; } = name;
    public SolidColorBrush PrimaryColor { get; set; } = primary;
    public SolidColorBrush SecondaryColor { get; set; } = secondary;
    public SolidColorBrush TertiaryColor { get; set; } = tertiary;
}