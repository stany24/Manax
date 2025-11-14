using System;
using System.Collections.Generic;
using System.Text.Json;
using Avalonia;
using Avalonia.Media;
using Material.Styles.Themes;
using Material.Styles.Themes.Base;

namespace ManaxClient.Models.Theme;

public static class ThemeSettings
{
    private static readonly string SavePath = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ManaxClient",
        "themesettings.json");

    private static readonly JsonSerializerOptions Settings = new() { WriteIndented = true };

    public static ThemeSettingsData Current { get; private set; } = new("Last default", Color.Parse("#007ACC"), Color.Parse("#6C757D"));
    
    public static void UpdateTheme(ThemeSettingsData themeSettingsData)
    {
        IBaseTheme mode = themeSettingsData.IsDark ? Material.Styles.Themes.Theme.Dark : Material.Styles.Themes.Theme.Light;
    
        Material.Styles.Themes.Theme theme = Material.Styles.Themes.Theme.Create(mode, themeSettingsData.PrimaryColor.Color, themeSettingsData.SecondaryColor.Color);
        MaterialThemeBase? themeBootstrap = Application.Current?.LocateMaterialTheme<MaterialThemeBase>();
        if (themeBootstrap == null){return;}
        themeBootstrap.CurrentTheme = theme;
        Save(themeSettingsData);
        Current = themeSettingsData;
    }
    
    public static List<ThemeSettingsData> GetPresets()
    {
        return
        [
            new ThemeSettingsData("Bleu", Color.Parse("#007ACC"), Color.Parse("#6C757D")),
            new ThemeSettingsData("Violet", Color.Parse("#6F42C1"), Color.Parse("#6C757D")),
            new ThemeSettingsData("Rouge", Color.Parse("#DC3545"), Color.Parse("#6C757D")),
            new ThemeSettingsData("Vert", Color.Parse("#28A745"), Color.Parse("#6C757D"))
        ];
    }
    
    public static void Load()
    {
        if (!System.IO.File.Exists(SavePath))
        {
            UpdateTheme(GetPresets()[0]);
            return;
        }
        string json = System.IO.File.ReadAllText(SavePath);
        UpdateTheme(JsonSerializer.Deserialize<ThemeSettingsData>(json) ?? GetPresets()[0]);
    }

    private static void Save(ThemeSettingsData themeSettingsData)
    {
        string directory = System.IO.Path.GetDirectoryName(SavePath) ?? string.Empty;
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(themeSettingsData, Settings);
        System.IO.File.WriteAllText(SavePath, json);
    }
}