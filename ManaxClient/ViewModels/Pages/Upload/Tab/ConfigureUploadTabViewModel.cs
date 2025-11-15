using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using ManaxClient.Models.Upload;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Pages.Upload.Tab;

public partial class ConfigureUploadTabViewModel : TabViewModel
{
    [ObservableProperty] private bool _canFetch = true;
    [ObservableProperty] private string _processingFolder = string.Empty;

    public ConfigureUploadTabViewModel()
    {
        LoadSettings();
        UploadSettings.SettingsChanged += (_, _) => LoadSettings();
    }

    public ObservableCollection<Source> SourceFolders { get; set; } = [];

    private void LoadSettings()
    {
        IEnumerable<Source> sources = UploadSettings.SourceFolders.Select(s => new Source { Path = s });
        SourceFolders.Clear();
        SourceFolders.AddRange(sources);
        ProcessingFolder = UploadSettings.ProcessingFolder;
    }

    public async void AddSource()
    {
        try
        {
            Window? window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;
            if (window?.StorageProvider == null) return;

            IReadOnlyList<IStorageFolder> folders = await window.StorageProvider.OpenFolderPickerAsync(
                new FolderPickerOpenOptions
                {
                    Title = "Select Source Folder",
                    AllowMultiple = true
                });

            if (folders.Count == 0) return;

            foreach (IStorageFolder folder in folders)
            {
                string folderPath = folder.Path.LocalPath;
                if (string.IsNullOrEmpty(folderPath)) continue;

                if (SourceFolders.Any(s => s.Path == folderPath)) continue;

                UploadSettings.AddSourceFolder(folderPath);
            }
        }
        catch (Exception e)
        {
            Logger.LogError("Error adding source folder", e);
        }
    }

    public void RemoveSource(Source source)
    {
        UploadSettings.RemoveSourceFolder(source.Path);
    }

    public void Next()
    {
        NextRequested?.Invoke(this, new FetchFromSourceTabViewModel());
    }

    public async void UpdateProcessingFolder()
    {
        try
        {
            Window? window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;
            if (window?.StorageProvider == null) return;

            IReadOnlyList<IStorageFolder> folders = await window.StorageProvider.OpenFolderPickerAsync(
                new FolderPickerOpenOptions
                {
                    Title = "Select Processing Folder",
                    AllowMultiple = false
                });

            if (folders.Count == 0) return;

            string folderPath = folders[0].Path.LocalPath;
            if (!string.IsNullOrEmpty(folderPath)) UploadSettings.SetProcessingFolder(folderPath);
        }
        catch (Exception e)
        {
            Logger.LogError("Error updating processing folder", e);
        }
    }
}