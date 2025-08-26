using Avalonia;
using Avalonia.Media;

namespace ManaxClient.Models;

public class ThemeService
{
    public static void UpdateTheme(Theme theme, bool isDark)
    {
        Application? app = Application.Current;
        if (app == null) return;

        app.Resources["PrimaryBrush"] = theme.PrimaryColor;
        app.Resources["SecondaryColor"] = theme.SecondaryColor;
        app.Resources["TertiaryBrush"] = theme.TertiaryColor;

        app.Resources["Background1"] = isDark
            ? new SolidColorBrush(Color.Parse("#212529"))
            : new SolidColorBrush(Color.Parse("#F8F9FA"));
        app.Resources["Background2"] = isDark
            ? new SolidColorBrush(Color.Parse("#343A40"))
            : new SolidColorBrush(Color.Parse("#F8F9FA"));
        app.Resources["Background3"] = isDark
            ? new SolidColorBrush(Color.Parse("#495057"))
            : new SolidColorBrush(Color.Parse("#F8F9FA"));

        app.Resources["TextColor1"] = !isDark
            ? new SolidColorBrush(Color.Parse("#212529"))
            : new SolidColorBrush(Color.Parse("#F8F9FA"));
        app.Resources["TextColor2"] = !isDark
            ? new SolidColorBrush(Color.Parse("#343A40"))
            : new SolidColorBrush(Color.Parse("#F8F9FA"));
        app.Resources["TextColor3"] = !isDark
            ? new SolidColorBrush(Color.Parse("#495057"))
            : new SolidColorBrush(Color.Parse("#F8F9FA"));
    }
}