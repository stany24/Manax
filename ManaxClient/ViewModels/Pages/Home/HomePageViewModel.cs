using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using Jeek.Avalonia.Localization;
using ManaxClient.Models.Sources;
using ManaxClient.ViewModels.Pages.Serie;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Pages.Home;

public partial class HomePageViewModel : PageViewModel
{
    private readonly ReadOnlyObservableCollection<Models.Serie> _series;
    [ObservableProperty] private bool _isFolderPickerOpen;
    
    [ObservableProperty] private string _welcomeTitle = string.Empty;
    [ObservableProperty] private string _subtitle = string.Empty;
    [ObservableProperty] private string _addSeriesText = string.Empty;
    [ObservableProperty] private string _noSeriesText = string.Empty;
    [ObservableProperty] private string _noSeriesDescription = string.Empty;
    [ObservableProperty] private string _addFirstSeriesText = string.Empty;
    [ObservableProperty] private string _recentSeriesText = string.Empty;
    [ObservableProperty] private string _seriesCountText = string.Empty;
    [ObservableProperty] private string _selectFolderText = string.Empty;

    public HomePageViewModel()
    {
        SortExpressionComparer<Models.Serie> comparer =
            SortExpressionComparer<Models.Serie>.Descending(serie => serie.Title);
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
        
        BindLocalizedStrings();
    }

    private void BindLocalizedStrings()
    {
        Localize(() => WelcomeTitle, "HomePage.Welcome");
        Localize(() => Subtitle, "HomePage.Subtitle");
        Localize(() => AddSeriesText, "HomePage.AddSeries");
        Localize(() => NoSeriesText, "HomePage.NoSeries");
        Localize(() => NoSeriesDescription, "HomePage.NoSeries.Description");
        Localize(() => AddFirstSeriesText, "HomePage.AddFirstSeries");
        Localize(() => RecentSeriesText, "HomePage.RecentSeries");
        Localize(() => SelectFolderText, "HomePage.SelectFolder");
        Localize(() => SeriesCountText, "HomePage.SeriesCount", () => Series.Count);
    }

    public ReadOnlyObservableCollection<Models.Serie> Series => _series;

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
                    Title = SelectFolderText,
                    AllowMultiple = false
                });
            IsFolderPickerOpen = false;

            if (folders.Count == 0) return;
            string folderPath = folders[0].Path.LocalPath;
            if (string.IsNullOrEmpty(folderPath)) return;

            Optional<bool> uploadSerieResponse = await ManaxApiUploadClient.UploadSerieAsync(folderPath);
            if (uploadSerieResponse.Failed)
            {
                InfoEmitted?.Invoke(this, 
                    string.Format(Localizer.Get("HomePage.UploadFailure"),Path.GetDirectoryName(folderPath)));
                Logger.LogFailure("Failed to upload series: " + uploadSerieResponse.Error);
                return;
            }
            InfoEmitted?.Invoke(this, 
                string.Format(Localizer.Get("HomePage.UploadSuccess"), Path.GetDirectoryName(folderPath)));
            Logger.LogInfo("Serie upload successful");
        }
        catch (Exception e)
        {
            IsFolderPickerOpen = false;
            InfoEmitted?.Invoke(this, Localizer.Get("HomePage.UploadError"));
            Logger.LogError("Error uploading series", e);
        }
    }
}