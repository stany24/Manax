using System.Collections.Generic;
using Avalonia.Media;

namespace ManaxClient.Models;

public class Theme(string name, SolidColorBrush primary, SolidColorBrush secondary, SolidColorBrush tertiary)
{
    public string Name { get; set; } = name;
    public SolidColorBrush PrimaryColor { get; set; } = primary;
    public SolidColorBrush SecondaryColor { get; set; } = secondary;
    public SolidColorBrush TertiaryColor { get; set; } = tertiary;
}

public static class ThemePresets
{
    public static List<Theme> GetPresets()
    {
        return
        [
            new Theme("Bleu", SolidColorBrush.Parse("#007ACC"), SolidColorBrush.Parse("#6C757D"),
                SolidColorBrush.Parse("#28A745")),
            new Theme("Violet", SolidColorBrush.Parse("#6F42C1"), SolidColorBrush.Parse("#6C757D"),
                SolidColorBrush.Parse("#FD7E14")),
            new Theme("Rouge", SolidColorBrush.Parse("#DC3545"), SolidColorBrush.Parse("#6C757D"),
                SolidColorBrush.Parse("#20C997")),
            new Theme("Vert", SolidColorBrush.Parse("#28A745"), SolidColorBrush.Parse("#6C757D"),
                SolidColorBrush.Parse("#FFC107")),
            new Theme("Sombre Bleu", SolidColorBrush.Parse("#0D6EFD"), SolidColorBrush.Parse("#ADB5BD"),
                SolidColorBrush.Parse("#198754")),
            new Theme("Sombre Violet", SolidColorBrush.Parse("#8B5CF6"), SolidColorBrush.Parse("#ADB5BD"),
                SolidColorBrush.Parse("#F59E0B"))
        ];
    }
}