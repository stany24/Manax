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

    public static SettingsDataDto DataDto { get; private set; } = new();
    private static string SavePath => Path.Combine(AppContext.BaseDirectory, "settings.json");
    private static string BackupPath => Path.Combine(AppContext.BaseDirectory, "settings_backup.json");

    private static void Load()
    {
        if (!File.Exists(SavePath)) File.WriteAllText(SavePath, JsonSerializer.Serialize(DataDto));

        SettingsDataDto? settingsData = JsonSerializer.Deserialize<SettingsDataDto>(File.ReadAllText(SavePath));
        if (settingsData == null)
        {
            File.Move(SavePath, BackupPath, true);
            File.WriteAllText(SavePath, "{}");
            settingsData = new SettingsDataDto();
        }

        DataDto = settingsData;
    }

    private static void Save()
    {
        string json = JsonSerializer.Serialize(DataDto, JsonOptions);
        File.WriteAllText(SavePath, json);
    }

    public static void OverwriteSettings(SettingsDataDto newDataDto)
    {
        if (newDataDto.Validate() is { } err)
            throw new InvalidOperationException(err);
        DataDto = newDataDto;
        Save();
    }
}