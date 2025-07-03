using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxApp.Models;
using ManaxApp.ViewModels.Serie;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTOs.Library;
using ManaxLibrary.DTOs.Serie;

namespace ManaxApp.ViewModels.Library;

public partial class LibraryPageViewModel : PageViewModel
{
    [ObservableProperty] private LibraryDTO? _library;
    [ObservableProperty] private ObservableCollection<ClientSerie> _series = [];

    public LibraryPageViewModel(long libraryId)
    {
        ControlBarVisible = true;
        Task.Run(async () =>
        {
            LibraryDTO? libraryAsync = await ManaxApiLibraryClient.GetLibraryAsync(libraryId);
            if (libraryAsync == null) return;
            Dispatcher.UIThread.Post(() => Library = libraryAsync);
        });
        Task.Run(async () =>
        {
            List<long>? seriesIds = await ManaxApiLibraryClient.GetLibrarySeriesAsync(libraryId);

            if (seriesIds == null) return;
            foreach (long serieId in seriesIds)
            {
                SerieDTO? info = await ManaxApiSerieClient.GetSerieInfoAsync(serieId);
                if (info == null) continue;
                byte[]? posterBytes = await ManaxApiSerieClient.GetSeriePosterAsync(serieId);
                ClientSerie serie = new() { Info = info };
                if (posterBytes != null)
                    try
                    {
                        serie.Poster = new Bitmap(new MemoryStream(posterBytes));
                    }
                    catch
                    {
                        serie.Poster = null;
                    }
                else
                    serie.Poster = null;

                Dispatcher.UIThread.Post(() => Series.Add(serie));
            }
        });
    }

    public void MoveToSeriePage(ClientSerie serie)
    {
        SeriePageViewModel seriePageViewModel = new(serie.Info.Id);
        PageChangedRequested?.Invoke(this, seriePageViewModel);
    }

    public async void UploadDefault()
    {
        try
        {
            if (Library == null) return;

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

            IStorageFolder? folder = folders.FirstOrDefault();
            if (folder == null) return;
            string folderPath = folder.Path.LocalPath;
            if (string.IsNullOrEmpty(folderPath)) return;

            bool success = await UploadApiUploadClient.UploadSerieAsync(folderPath, Library.Id);
            if (!success) InfoEmitted?.Invoke(this, "Serie upload failed");
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error uploading series: {e.Message}");
        }
    }
}