using System.Text.Json.Serialization;
using Avalonia.Media;

namespace ManaxClient.Models.Theme;

public class ThemeSettingsData
{
    public bool IsDark { get; set; }
    public string Name { get; set; } = string.Empty;
    [JsonIgnore] public HslColor PrimaryColor
    {
        get => Color.FromUInt32(PrimaryColorString).ToHsl();
        init => PrimaryColorString = value.ToRgb().ToUInt32();
    }
    [JsonInclude] private uint PrimaryColorString { get; set; }
    
    public ThemeSettingsData()
    {
    }

    public ThemeSettingsData(string name, HslColor primary, bool isDark = false)
    {
        Name = name;
        PrimaryColorString = primary.ToRgb().ToUInt32();
        IsDark = isDark;
    }
}