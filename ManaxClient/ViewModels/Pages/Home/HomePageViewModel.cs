using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using DynamicData.Binding;
using Jeek.Avalonia.Localization;
using ManaxClient.Event;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Pages.Home;

public partial class HomePageViewModel : PageViewModel
{
    private readonly ReadOnlyObservableCollection<Models.Server.Data.Serie> _series;
    [ObservableProperty] private bool _isFolderPickerOpen;

    public HomePageViewModel()
    {
        SortExpressionComparer<Models.Server.Data.Serie> comparer =
            SortExpressionComparer<Models.Server.Data.Serie>.Descending(serie => serie.Title);
        MainWindowViewModel.Instance.SerieSource.Series
            .Connect()
            .SortAndBind(out _series, comparer)
            .Subscribe();
    }

    public ReadOnlyObservableCollection<Models.Server.Data.Serie> Series => _series;

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
                    Title = Localizer.Get("HomePage.SelectFolder"),
                    AllowMultiple = false
                });
            IsFolderPickerOpen = false;

            if (folders.Count == 0) return;
            string folderPath = folders[0].Path.LocalPath;
            if (string.IsNullOrEmpty(folderPath)) return;

            Optional<bool> uploadSerieResponse = await ManaxApiUploadClient.UploadSerieAsync(folderPath);
            if (uploadSerieResponse.Failed)
            {
                string format1 = string.Format(CultureInfo.InvariantCulture, Localizer.Get("HomePage.UploadFailure"),
                    Path.GetDirectoryName(folderPath));

                WeakReferenceMessenger.Default.Send(new NotificationMessage(format1));

                Logger.LogFailure("Failed to upload series: " + uploadSerieResponse.Error);
                return;
            }

            string format2 = string.Format(CultureInfo.InvariantCulture, Localizer.Get("HomePage.UploadSuccess"),
                Path.GetDirectoryName(folderPath));
            WeakReferenceMessenger.Default.Send(new NotificationMessage(format2));
            Logger.LogInfo("Serie upload successful");
        }
        catch (Exception e)
        {
            IsFolderPickerOpen = false;
            WeakReferenceMessenger.Default.Send(new NotificationMessage(Localizer.Get("HomePage.UploadError")));
            Logger.LogError("Error uploading series", e);
        }
    }
}