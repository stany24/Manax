using System.Reflection;
using System.Text.Json;
using ManaxLibrary.DTOs.Setting;

namespace ManaxServer.Settings;

public static class SettingsManager
{ 
    public static SettingsData Data { get; set; } = new();
    private static string SavePath => Path.Combine(AppContext.BaseDirectory, "settings.json");
    private static string BackupPath => Path.Combine(AppContext.BaseDirectory, "settings_backup.json");
    
    static SettingsManager()
    {
        Load();
    }

    private static void Load()
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

    private static void Save()
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
            Data.ForceFixIssues();
            Save();
            return true;
        }
        catch
        {
            return false;
        }
    }
}