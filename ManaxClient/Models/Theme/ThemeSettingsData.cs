using System.Text.Json.Serialization;
using Avalonia.Media;

namespace ManaxClient.Models.Theme;

public class ThemeSettingsData
{
    public string Name { get; set; } = string.Empty;
    
    // Serializable color strings
    public string PrimaryColorHex { get; set; } = "#0000FF";
    public string SecondaryColorHex { get; set; } = "#808080";
    
    // Non-serializable brushes
    [JsonIgnore]
    public SolidColorBrush PrimaryColor 
    { 
        get => new(Color.Parse(PrimaryColorHex)); 
        set => PrimaryColorHex = value.Color.ToString(); 
    }
    
    [JsonIgnore]
    public SolidColorBrush SecondaryColor 
    { 
        get => new(Color.Parse(SecondaryColorHex)); 
        set => SecondaryColorHex = value.Color.ToString(); 
    }
    
    public bool IsDark { get; set; }
    
    public ThemeSettingsData()
    {
    }
    
    public ThemeSettingsData(string name, Color primary, Color secondary, bool isDark = false)
    {
        Name = name;
        PrimaryColorHex = primary.ToString();
        SecondaryColorHex = secondary.ToString();
        IsDark = isDark;
    }
}