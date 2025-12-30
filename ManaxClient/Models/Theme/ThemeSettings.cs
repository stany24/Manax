using System;
using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Messaging;
using ManaxClient.Event;
using ManaxClient.Manager;
using Material.Styles.Themes;
using Material.Styles.Themes.Base;

namespace ManaxClient.Models.Theme;

public static class ThemeSettings
{
    private static ThemeSettingsData LoadFailBackup { get; } = new(new HslColor(1, 230, 0.5,0.5));
    public static ThemeSettingsData Current { get; private set; } = null!;


    public static void UpdateTheme(ThemeSettingsData themeSettingsData)
    {
        IBaseTheme mode = themeSettingsData.IsDark
            ? Material.Styles.Themes.Theme.Dark
            : Material.Styles.Themes.Theme.Light;
        
        double offset = themeSettingsData.IsDark ? - 0.2 : 0.2;
        
        HslColor secondaryHsl = new(themeSettingsData.AccentColor.A,
            themeSettingsData.AccentColor.H,
            themeSettingsData.AccentColor.S,
            Math.Clamp(themeSettingsData.AccentColor.L + offset, 0, 1));
        
        Material.Styles.Themes.Theme theme = Material.Styles.Themes.Theme.Create(mode,themeSettingsData.AccentColor.ToRgb(), secondaryHsl.ToRgb());
        MaterialThemeBase? themeBootstrap = Application.Current?.LocateMaterialTheme<MaterialThemeBase>();
        if (themeBootstrap == null) return;
        themeBootstrap.CurrentTheme = theme;
        StorageManager.Save(StorageManager.ThemeFile, themeSettingsData);
        Current = themeSettingsData;
        WeakReferenceMessenger.Default.Send(new ThemeMessage(themeSettingsData));
    }

    public static void Load()
    {
        ThemeSettingsData? themeSettingsData = StorageManager.Load<ThemeSettingsData>(StorageManager.ThemeFile);
        if (themeSettingsData == null)
        {
            UpdateTheme(LoadFailBackup);
            return;
        }
        UpdateTheme(themeSettingsData);
    }
}