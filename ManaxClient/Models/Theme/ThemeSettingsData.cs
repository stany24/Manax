using System.Text.Json.Serialization;
using Avalonia.Media;

namespace ManaxClient.Models.Theme;

public class ThemeSettingsData
{
    public ThemeSettingsData()
    {
    }

    public ThemeSettingsData(HslColor accent, bool isDark = false)
    {
        AccentColorString = accent.ToRgb().ToUInt32();
        IsDark = isDark;
    }

    public bool IsDark { get; set; }

    [JsonIgnore]
    public HslColor AccentColor
    {
        get => Color.FromUInt32(AccentColorString).ToHsl();
        init => AccentColorString = value.ToRgb().ToUInt32();
    }

    [JsonInclude] private uint AccentColorString { get; set; }
}