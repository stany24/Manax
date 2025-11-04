using System;
using System.Text.Json;

namespace ManaxClient.Models.Upload;

public static class UploadSettings
{
    public static EventHandler? SettingsChanged;
    private static UploadSettingsData _settings = new();
    private static readonly string SavePath = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ManaxClient",
        "uploadsettings.json");

    private static readonly JsonSerializerOptions JsonSettings = new() { WriteIndented = true };

    static UploadSettings()
    {
        Load();
    }
    
    public static string ProcessingFolder => _settings.ProcessingFolder;
    public static System.Collections.ObjectModel.ObservableCollection<string> SourceFolders => _settings.SourceFolders;
    
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
        if (!System.IO.File.Exists(SavePath)) return;
        string json = System.IO.File.ReadAllText(SavePath);
        _settings = JsonSerializer.Deserialize<UploadSettingsData>(json) ?? new UploadSettingsData();
    }

    private static void Save()
    {
        string directory = System.IO.Path.GetDirectoryName(SavePath) ?? string.Empty;
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(_settings, JsonSettings);
        System.IO.File.WriteAllText(SavePath, json);
        SettingsChanged?.Invoke(null, EventArgs.Empty);
    }
}