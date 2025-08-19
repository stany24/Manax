using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Search;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Home;

public partial class HomePageViewModel : BaseSeriesViewModel
{
    [ObservableProperty] private bool _isFolderPickerOpen;

    public HomePageViewModel()
    {
        Task.Run(async () =>
        {
            Optional<List<long>> libraryIdsAsync = await ManaxApiLibraryClient.GetLibraryIdsAsync();
            if (libraryIdsAsync.Failed)
            {
                InfoEmitted?.Invoke(this, libraryIdsAsync.Error);
                return;
            }
            LoadSeries(new Search { IncludedLibraries = libraryIdsAsync.GetValue() });
        });
    }

    protected override void OnSerieCreated(SerieDto serie)
    {
        Logger.LogInfo("A new Serie has been created");
        AddSerieToCollection(serie);
    }

    public async void UploadSerie()
    {
        try
        {
            if (IsFolderPickerOpen) return;
            IsFolderPickerOpen = true;

            Window? window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;
            if (window?.StorageProvider == null) return;
            IReadOnlyList<IStorageFolder> folders = await window.StorageProvider.OpenFolderPickerAsync(
                new FolderPickerOpenOptions
                {
                    Title = "Sélectionnez un dossier à uploader",
                    AllowMultiple = false
                });
            IsFolderPickerOpen = false;

            if (folders.Count == 0) return;
            string folderPath = folders[0].Path.LocalPath;
            if (string.IsNullOrEmpty(folderPath)) return;

            Optional<bool> uploadSerieResponse = await UploadApiUploadClient.UploadSerieAsync(folderPath);
            if (uploadSerieResponse.Failed)
            {
                InfoEmitted?.Invoke(this, uploadSerieResponse.Error);
            }
            else
            {
                InfoEmitted?.Invoke(this, "Serie upload successful");
                Logger.LogInfo("Serie upload successful");
            }
        }
        catch (Exception e)
        {
            Logger.LogError("Error uploading series", e, Environment.StackTrace);
        }
    }
}