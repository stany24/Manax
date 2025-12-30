using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using ManaxClient.Manager;

namespace ManaxClient.Models.Upload;

public static class UploadSettings
{
    private static readonly UploadSettingsData Settings;

    static UploadSettings()
    {
        Settings = StorageManager.Load<UploadSettingsData>(StorageManager.UploadFile) ?? new UploadSettingsData();
    }

    public static EventHandler? SettingsChanged { get; set; }

    public static string ProcessingFolder => Settings.ProcessingFolder;
    public static ObservableCollection<string> SourceFolders => Settings.SourceFolders;

    public static void SetProcessingFolder(string folder)
    {
        Settings.ProcessingFolder = folder;
        Save();
        SettingsChanged?.Invoke(null, EventArgs.Empty);
    }

    public static void AddSourceFolder(string folder)
    {
        if (Settings.SourceFolders.Contains(folder)) return;
        Settings.SourceFolders.Add(folder);
        Save();
        SettingsChanged?.Invoke(null, EventArgs.Empty);
    }

    public static void RemoveSourceFolder(string folder)
    {
        if (!Settings.SourceFolders.Contains(folder)) return;
        Settings.SourceFolders.Remove(folder);
        Save();
        SettingsChanged?.Invoke(null, EventArgs.Empty);
    }

    private static void Save()
    {
        StorageManager.Save(StorageManager.UploadFile,Settings);
        SettingsChanged?.Invoke(null, EventArgs.Empty);
    }
}