using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Messaging;
using ManaxClient.Event;
using Material.Styles.Themes;
using Material.Styles.Themes.Base;

namespace ManaxClient.Models.Theme;

public static class ThemeSettings
{
    private static readonly string SavePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ManaxClient",
        "themesettings.json");

    private static readonly JsonSerializerOptions Settings = new() { WriteIndented = true };

    public static ThemeSettingsData Current { get; private set; } = GetPresets()[0];


    public static void UpdateTheme(ThemeSettingsData themeSettingsData)
    {
        IBaseTheme mode = themeSettingsData.IsDark
            ? Material.Styles.Themes.Theme.Dark
            : Material.Styles.Themes.Theme.Light;
        
        double offset = themeSettingsData.IsDark ? - 0.2 : 0.2;

        HslColor primaryHsl = new(themeSettingsData.PrimaryColor.A,
            themeSettingsData.PrimaryColor.H,
            themeSettingsData.PrimaryColor.S,
            Math.Clamp(themeSettingsData.PrimaryColor.L + offset, 0, 1));
        
        HslColor secondaryHsl = new(themeSettingsData.PrimaryColor.A,
            themeSettingsData.PrimaryColor.H,
            themeSettingsData.PrimaryColor.S,
            Math.Clamp(themeSettingsData.PrimaryColor.L + 2*offset, 0, 1));
        
        Material.Styles.Themes.Theme theme = Material.Styles.Themes.Theme.Create(mode,primaryHsl.ToRgb(), secondaryHsl.ToRgb());
        MaterialThemeBase? themeBootstrap = Application.Current?.LocateMaterialTheme<MaterialThemeBase>();
        if (themeBootstrap == null) return;
        themeBootstrap.CurrentTheme = theme;
        Save(themeSettingsData);
        Current = themeSettingsData;
        WeakReferenceMessenger.Default.Send(new ThemeMessage(themeSettingsData));
    }

    public static List<ThemeSettingsData> GetPresets()
    {
        return
        [
            new ThemeSettingsData("Bleu", new HslColor(1,240.0,1,0.5)),
            new ThemeSettingsData("Violet", new HslColor(1,270.0,1,0.5)),
            new ThemeSettingsData("Rouge", new HslColor(1,335.0,1,0.5)),
            new ThemeSettingsData("Vert", new HslColor(1,120.0,1,0.5))
        ];
    }

    public static void Load()
    {
        if (!File.Exists(SavePath))
        {
            UpdateTheme(GetPresets()[0]);
            return;
        }

        string json = File.ReadAllText(SavePath);
        ThemeSettingsData? themeSettingsData = JsonSerializer.Deserialize<ThemeSettingsData>(json);
        UpdateTheme(themeSettingsData ?? GetPresets()[0]);
    }

    private static void Save(ThemeSettingsData themeSettingsData)
    {
        string directory = Path.GetDirectoryName(SavePath) ?? string.Empty;
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        string json = JsonSerializer.Serialize(themeSettingsData, Settings);
        File.WriteAllText(SavePath, json);
    }
}