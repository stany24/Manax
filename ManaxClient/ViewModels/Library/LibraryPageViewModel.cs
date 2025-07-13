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
using ManaxClient.Models;
using ManaxClient.ViewModels.Home;
using ManaxClient.ViewModels.Serie;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTOs.Library;
using ManaxLibrary.DTOs.Serie;

namespace ManaxClient.ViewModels.Library;

public partial class LibraryPageViewModel : PageViewModel
{
    [ObservableProperty] private LibraryDTO? _library;
    [ObservableProperty] private ObservableCollection<ClientSerie> _series = [];
    [ObservableProperty] private bool _isFolderPickerOpen;

    public LibraryPageViewModel(long libraryId)
    {
        ControlBarVisible = true;
        Task.Run(() => { LoadLibrary(libraryId); });
        Task.Run(() => { LoadSeries(libraryId); });
        ServerNotification.OnSerieCreated += serieInfo =>
        {
            if (serieInfo.LibraryId != libraryId) return;
            Dispatcher.UIThread.Post(() =>
            {
                if (serieInfo == null) return;
                ClientSerie? existingSerie = Series.FirstOrDefault(s => s.Info.Id == serieInfo.Id);
                if (existingSerie != null) return;
                Series.Add(new ClientSerie { Info = serieInfo, Poster = null });
            });
        };
    }
    
    private async void LoadLibrary(long libraryId)
    {
        try
        {
            LibraryDTO? libraryAsync = await ManaxApiLibraryClient.GetLibraryAsync(libraryId);
            if (libraryAsync == null) return;
            Dispatcher.UIThread.Post(() => Library = libraryAsync);
        }
        catch (Exception)
        {
            InfoEmitted?.Invoke(this, "Failed to load library informations");
        }
    }
    
    public void DeleteLibrary()
    {
        if(Library == null){return;}
        Task.Run(async () =>
        {
            if (await ManaxApiLibraryClient.DeleteLibraryAsync(Library.Id))
            {
                PageChangedRequested?.Invoke(this, new HomePageViewModel());
            }
            else
            {
                InfoEmitted?.Invoke(this, "Failed to delete library");
            }
        });
    }

    private async void LoadSeries(long libraryId)
    {
        try
        {
            List<long>? seriesIds = await ManaxApiLibraryClient.GetLibrarySeriesAsync(libraryId);

            if (seriesIds == null) return;
            foreach (long serieId in seriesIds)
            {
                if(Series.FirstOrDefault(s => s.Info.Id == serieId) != null)
                    continue;
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
        }
        catch (Exception)
        {
            InfoEmitted?.Invoke(this,"Failed to load series");
        }
    }

    public void MoveToSeriePage(ClientSerie serie)
    {
        SeriePageViewModel seriePageViewModel = new(serie.Info.Id);
        PageChangedRequested?.Invoke(this, seriePageViewModel);
    }

    public async void UploadSerie()
    {
        try
        {
            if (IsFolderPickerOpen) return;
            IsFolderPickerOpen = true;
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
            InfoEmitted?.Invoke(this, !success ? "Serie upload failed" : "Serie upload successful");
        }
        catch (Exception e)
        {
            Debug.WriteLine($"Error uploading series: {e.Message}");
        }
        finally
        {
            IsFolderPickerOpen = false;
        }
    }
}