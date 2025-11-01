using Avalonia.Media;

namespace ManaxClient.Models.Theme;

public class ManaxTheme(string name, Color primary, Color secondary)
{
    public string Name { get; set; } = name;
    public SolidColorBrush PrimaryColor { get; set; } = new(primary);
    public SolidColorBrush SecondaryColor { get; set; } = new(secondary);
}