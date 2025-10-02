using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models.Collections;
using ManaxClient.ViewModels.Pages.Serie;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Search;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Pages.Home;

public partial class HomePageViewModel : PageViewModel
{
    [ObservableProperty] private bool _isFolderPickerOpen;
    public SortedObservableCollection<Models.Serie.Serie> Series { get; set; } =
        new([]) { SortingSelector = dto => dto.Title, Descending = false };

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

            LoadSeries(new Search());
        });
    }

    private void LoadSeries(Search search)
    {
        try
        {
            Optional<List<long>> searchResultResponse = ManaxApiSerieClient.GetSearchResult(search).Result;
            if (searchResultResponse.Failed)
            {
                InfoEmitted?.Invoke(this, searchResultResponse.Error);
                return;
            }

            foreach (long serieId in searchResultResponse.GetValue())
            {
                if (Series.FirstOrDefault(s => s.Id == serieId) != null) continue;

                Optional<SerieDto> serieInfoAsync = ManaxApiSerieClient.GetSerieInfoAsync(serieId).Result;
                if (serieInfoAsync.Failed)
                {
                    InfoEmitted?.Invoke(this, serieInfoAsync.Error);
                    continue;
                }

                Models.Serie.Serie serie = new(serieInfoAsync.GetValue());
                serie.ErrorEmitted += (_, info) => { InfoEmitted?.Invoke(this, info); };
                serie.LoadInfo();
                serie.LoadPoster();

                Dispatcher.UIThread.Post(() =>
                {
                    Series.Add(serie);
                });
            }
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to load series", e, Environment.StackTrace);
        }
    }
    
    public void MoveToSeriePage(Models.Serie.Serie serie)
    {
        SeriePageViewModel seriePageViewModel = new(serie.Id);
        PageChangedRequested?.Invoke(this, seriePageViewModel);
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

            Optional<bool> uploadSerieResponse = await ManaxApiUploadClient.UploadSerieAsync(folderPath);
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