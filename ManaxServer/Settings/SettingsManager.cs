using System.Text.Json;
using ManaxLibrary.DTO.Setting;

namespace ManaxServer.Settings;

public static class SettingsManager
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    static SettingsManager()
    {
        Load();
    }

    public static SettingsData Data { get; private set; } = new();
    private static string SavePath => Path.Combine(AppContext.BaseDirectory, "settings.json");
    private static string BackupPath => Path.Combine(AppContext.BaseDirectory, "settings_backup.json");

    private static void Load()
    {
        if (!File.Exists(SavePath)) File.WriteAllText(SavePath, JsonSerializer.Serialize(Data));

        SettingsData? settingsData = JsonSerializer.Deserialize<SettingsData>(File.ReadAllText(SavePath));
        if (settingsData == null)
        {
            File.Move(SavePath, BackupPath, true);
            File.WriteAllText(SavePath, "{}");
            settingsData = new SettingsData();
        }

        Data = settingsData;
    }

    private static void Save()
    {
        string json = JsonSerializer.Serialize(Data, JsonOptions);
        File.WriteAllText(SavePath, json);
    }

    public static void OverwriteSettings(SettingsData newData)
    {
        if (!newData.IsValid)
            throw new InvalidOperationException(
                "New settings data contains issues that need to be resolved before saving.");
        Data = newData;
        Save();
    }
}