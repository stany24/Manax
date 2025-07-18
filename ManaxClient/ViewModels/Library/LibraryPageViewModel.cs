using System;
using System.Collections.Generic;
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
using ManaxClient.Models.Collections;
using ManaxClient.ViewModels.Home;
using ManaxClient.ViewModels.Serie;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTOs.Library;
using ManaxLibrary.DTOs.Search;
using ManaxLibrary.DTOs.Serie;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Library;

public partial class LibraryPageViewModel : PageViewModel
{
    [ObservableProperty] private LibraryDTO? _library;
    [ObservableProperty] private SortedObservableCollection<ClientSerie> _series;
    [ObservableProperty] private bool _isFolderPickerOpen;
    private readonly object _serieLock = new();

    public LibraryPageViewModel(long libraryId)
    {
        Series = new SortedObservableCollection<ClientSerie>([]) {SortingSelector = serie => serie.Info.Title ,Descending = false };
        Task.Run(() => { LoadLibrary(libraryId); });
        Task.Run(() => { LoadSeries(libraryId); });
        ServerNotification.OnSerieCreated += OnSerieCreated;
        ServerNotification.OnPosterModified += OnPosterModified;
        ServerNotification.OnSerieDeleted += OnSerieDeleted;
    }

    ~LibraryPageViewModel()
    {
        ServerNotification.OnSerieCreated -= OnSerieCreated;
        ServerNotification.OnPosterModified -= OnPosterModified;
        ServerNotification.OnSerieDeleted -= OnSerieDeleted;
    }
    
    private void OnSerieCreated(SerieDTO serie)
    {
        Logger.LogInfo("A new Serie has been created in "+Library?.Name);
        if (serie.LibraryId != Library?.Id) return;
        Dispatcher.UIThread.Post(() =>
        {
            lock (_serieLock)
            {
                ClientSerie? existingSerie = Series.FirstOrDefault(s => s.Info.Id == serie.Id);
                if (existingSerie != null) return;
                Series.Add(new ClientSerie { Info = serie, Poster = null });
            }
        });
    }
    
    private void OnSerieDeleted(long serieId)
    {
        Dispatcher.UIThread.Post(() =>
        {
            lock (_serieLock)
            {
                ClientSerie? existingSerie = Series.FirstOrDefault(s => s.Info.Id == serieId);
                if (existingSerie != null)
                {
                    Series.Remove(existingSerie);
                }
            }
        });
    }
    
    private async void OnPosterModified(long serieId)
    {
        try
        {
            ClientSerie? serie;
            lock (_serieLock)
            {
                serie = Series.FirstOrDefault(s => s.Info.Id == serieId);
            }
            if (serie == null) return;
            byte[]? posterBytes = await ManaxApiSerieClient.GetSeriePosterAsync(serieId);
            if (posterBytes == null){return;}
            try
            {
                Bitmap poster = new(new MemoryStream(posterBytes));
                Dispatcher.UIThread.Post(() => serie.Poster = poster);
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to load the new poster for serie with ID: " + serieId,e, Environment.StackTrace);
            }
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to receive the new poster for serie with ID: " + serieId,e, Environment.StackTrace);
        }
    }
    
    private async void LoadLibrary(long libraryId)
    {
        try
        {
            LibraryDTO? libraryAsync = await ManaxApiLibraryClient.GetLibraryAsync(libraryId);
            if (libraryAsync == null) return;
            Dispatcher.UIThread.Post(() => Library = libraryAsync);
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to load the library with ID: " + libraryId,e, Environment.StackTrace);
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
                Logger.LogFailure("Failed to delete library with ID: " + Library.Id, Environment.StackTrace);
            }
        });
    }

    private async void LoadSeries(long libraryId)
    {
        try
        {
            List<long>? seriesIds = await ManaxApiSerieClient.GetSearchResult(new Search { IncludedLibraries = [libraryId] });
            if (seriesIds == null) return;
            foreach (long serieId in seriesIds)
            {
                lock (_serieLock)
                {
                    if(Series.FirstOrDefault(s => s.Info.Id == serieId) != null) {continue;}
                }
                
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

                Dispatcher.UIThread.Post(() =>
                {
                    lock (_serieLock)
                    {
                        Series.Add(serie);
                    }
                });
            }
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to load series for library with ID: " + libraryId, e, Environment.StackTrace);
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
            Logger.LogInfo(!success ? "Serie upload failed" : "Serie upload successful");
        }
        catch (Exception e)
        {
            Logger.LogError("Error uploading series", e, Environment.StackTrace);
        }
        finally
        {
            IsFolderPickerOpen = false;
        }
    }
}