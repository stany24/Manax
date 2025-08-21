using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Media;

namespace ManaxClient.Models;

public class Theme : INotifyPropertyChanged
{
    public string Name { get; set; }
    public SolidColorBrush PrimaryColor { get; set; }
    public SolidColorBrush SecondaryColor { get; set; }
    public SolidColorBrush TertiaryColor { get; set; }

    public event PropertyChangedEventHandler? PropertyChanged;
}

public static class ThemePresets
{
    public static List<Theme> GetPresets() =>
    [
        new()
        {
            Name = "Bleu par défaut",
            PrimaryColor = SolidColorBrush.Parse("#007ACC"),
            SecondaryColor = SolidColorBrush.Parse("#6C757D"),
            TertiaryColor = SolidColorBrush.Parse("#28A745")
        },

        new()
        {
            Name = "Violet",
            PrimaryColor = SolidColorBrush.Parse("#6F42C1"),
            SecondaryColor = SolidColorBrush.Parse("#6C757D"),
            TertiaryColor = SolidColorBrush.Parse("#FD7E14")
        },

        new()
        {
            Name = "Rouge",
            PrimaryColor = SolidColorBrush.Parse("#DC3545"),
            SecondaryColor = SolidColorBrush.Parse("#6C757D"),
            TertiaryColor = SolidColorBrush.Parse("#20C997")
        },

        new()
        {
            Name = "Vert",
            PrimaryColor = SolidColorBrush.Parse("#28A745"),
            SecondaryColor = SolidColorBrush.Parse("#6C757D"),
            TertiaryColor = SolidColorBrush.Parse("#FFC107")
        },

        new()
        {
            Name = "Sombre Bleu",
            PrimaryColor = SolidColorBrush.Parse("#0D6EFD"),
            SecondaryColor = SolidColorBrush.Parse("#ADB5BD"),
            TertiaryColor = SolidColorBrush.Parse("#198754")
        },

        new()
        {
            Name = "Sombre Violet",
            PrimaryColor = SolidColorBrush.Parse("#8B5CF6"),
            SecondaryColor = SolidColorBrush.Parse("#ADB5BD"),
            TertiaryColor = SolidColorBrush.Parse("#F59E0B")
        }
    ];
}
