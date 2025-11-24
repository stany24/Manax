using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;

namespace ManaxClient.Models.Upload;

public static class UploadSettings
{
    public static EventHandler? SettingsChanged { get; set; }
    private static UploadSettingsData _settings = new();

    private static readonly string SavePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ManaxClient",
        "uploadsettings.json");

    private static readonly JsonSerializerOptions JsonSettings = new() { WriteIndented = true };

    static UploadSettings()
    {
        Load();
    }

    public static string ProcessingFolder => _settings.ProcessingFolder;
    public static ObservableCollection<string> SourceFolders => _settings.SourceFolders;

    public static void SetProcessingFolder(string folder)
    {
        _settings.ProcessingFolder = folder;
        Save();
        SettingsChanged?.Invoke(null, EventArgs.Empty);
    }

    public static void AddSourceFolder(string folder)
    {
        if (_settings.SourceFolders.Contains(folder)) return;
        _settings.SourceFolders.Add(folder);
        Save();
        SettingsChanged?.Invoke(null, EventArgs.Empty);
    }

    public static void RemoveSourceFolder(string folder)
    {
        if (!_settings.SourceFolders.Contains(folder)) return;
        _settings.SourceFolders.Remove(folder);
        Save();
        SettingsChanged?.Invoke(null, EventArgs.Empty);
    }

    private static void Load()
    {
        if (!File.Exists(SavePath)) return;
        string json = File.ReadAllText(SavePath);
        _settings = JsonSerializer.Deserialize<UploadSettingsData>(json) ?? new UploadSettingsData();
    }

    private static void Save()
    {
        string directory = Path.GetDirectoryName(SavePath) ?? string.Empty;
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        string json = JsonSerializer.Serialize(_settings, JsonSettings);
        File.WriteAllText(SavePath, json);
        SettingsChanged?.Invoke(null, EventArgs.Empty);
    }
}