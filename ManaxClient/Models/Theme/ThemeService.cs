using Avalonia;
using Material.Styles.Themes;
using Material.Styles.Themes.Base;

namespace ManaxClient.Models.Theme;

public static class ThemeService
{
    public static void UpdateTheme(ManaxTheme manaxTheme, bool isDark)
    {
        IBaseTheme mode = isDark ? Material.Styles.Themes.Theme.Dark : Material.Styles.Themes.Theme.Light;
    
        Material.Styles.Themes.Theme theme = Material.Styles.Themes.Theme.Create(mode, manaxTheme.PrimaryColor.Color, manaxTheme.SecondaryColor.Color);
        MaterialThemeBase? themeBootstrap = Application.Current?.LocateMaterialTheme<MaterialThemeBase>();
        if (themeBootstrap == null){return;}
        themeBootstrap.CurrentTheme = theme;
    }
}