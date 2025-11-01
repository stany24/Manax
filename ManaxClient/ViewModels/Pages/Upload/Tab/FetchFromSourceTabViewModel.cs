using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models.Upload;

namespace ManaxClient.ViewModels.Pages.Upload.Tab;

public partial class FetchFromSourceTabViewModel:PageViewModel
{
    public ObservableCollection<Source> SourcesFolders { get; set; }= [];
    [ObservableProperty] private string _processingFolder = string.Empty;
    [ObservableProperty] private bool _canFetch = true;
    
    public async void AddSource()
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
            
            // Check if already exists
            if (SourcesFolders.Any(s => s.Path == folderPath)) continue;
            
            SourcesFolders.Add(new Source { Path = folderPath });
        }
    }
    
    public async void BrowseProcessingFolder()
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
        if (!string.IsNullOrEmpty(folderPath))
        {
            ProcessingFolder = folderPath;
        }
    }

    public void Fetch()
    {
        CanFetch = false;
        Task.Run(() =>
        {
            foreach (Source source in SourcesFolders)
            {
                source.Fetch(ProcessingFolder);
            }
            CanFetch = true;
        });
    }
}