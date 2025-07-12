using System.Reflection;
using System.Text.Json;

namespace ManaxApi.Settings;

public static class Settings
{ 
    public static SettingsData Data { get; set; } = new();
    private static string SavePath => Path.Combine(Directory.GetCurrentDirectory(), "settings.json");
    private static string BackupPath => Path.Combine(Directory.GetCurrentDirectory(), "settings_backup.json");
    
    static Settings()
    {
        Load();   
    }
    
    public static void Load()
    {
        if (!File.Exists(SavePath)) { File.WriteAllText(SavePath, "{}"); }

        SettingsData? settingsData = JsonSerializer.Deserialize<SettingsData>(File.ReadAllText(SavePath));
        if (settingsData == null)
        {
            File.Move(SavePath,BackupPath, true);
            File.WriteAllText(SavePath, "{}");
            settingsData = new SettingsData();
        }
        Data = settingsData;
    }
    
    public static void Save()
    {
        string json = JsonSerializer.Serialize(Data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(SavePath, json);
    }
    
    public static bool SetProperty(string propertyName, object value)
    {
        PropertyInfo? property = Data.GetType().GetProperty(propertyName);
        if (property == null || !property.CanWrite) return false;

        try
        {
            property.SetValue(Data, value);
            Save();
            return true;
        }
        catch
        {
            return false;
        }
    }
}