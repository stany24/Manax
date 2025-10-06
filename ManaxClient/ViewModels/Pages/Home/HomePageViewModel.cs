using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using ManaxClient.Models;
using ManaxClient.Models.Sources;
using ManaxClient.ViewModels.Pages.Serie;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Pages.Home;

public partial class HomePageViewModel : PageViewModel
{
    [ObservableProperty] private bool _isFolderPickerOpen;
    
    private readonly ReadOnlyObservableCollection<Models.Serie> _series;
    public ReadOnlyObservableCollection<Models.Serie> Series => _series;

    public HomePageViewModel()
    {
        SortExpressionComparer<Models.Serie> comparer = SortExpressionComparer<Models.Serie>.Descending(serie => serie.Title);
        SerieSource.Series
            .Connect()
            .SortAndBind(out _series, comparer)
            .Subscribe(changes =>
            {
                foreach (Change<Models.Serie, long> change in changes)
                {
                    if (change.Reason != ChangeReason.Add) continue;
                    change.Current.LoadInfo();
                    change.Current.LoadPoster();
                }
            });
    }
    
    public void MoveToSeriePage(Models.Serie serie)
    {
        SeriePageViewModel seriePageViewModel = new(serie);
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
            Logger.LogError("Error uploading series", e);
        }
    }
}