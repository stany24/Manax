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
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Library;
using ManaxLibrary.DTO.Search;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.ViewModels.Library;

public partial class LibraryPageViewModel : PageViewModel
{
    private readonly object _serieLock = new();
    [ObservableProperty] private bool _isFolderPickerOpen;
    [ObservableProperty] private LibraryDto? _library;
    [ObservableProperty] private SortedObservableCollection<ClientSerie> _series;

    public LibraryPageViewModel(long libraryId)
    {
        Series = new SortedObservableCollection<ClientSerie>([])
            { SortingSelector = serie => serie.Info.Title, Descending = false };
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

    private void OnSerieCreated(SerieDto serie)
    {
        Logger.LogInfo("A new Serie has been created in " + Library?.Name);
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
                if (existingSerie != null) Series.Remove(existingSerie);
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
            Optional<byte[]> seriePosterResponse = await ManaxApiSerieClient.GetSeriePosterAsync(serieId);
            if (seriePosterResponse.Failed)
            {
                InfoEmitted?.Invoke(this, seriePosterResponse.Error);
                return;
            }

            try
            {
                Bitmap poster = new(new MemoryStream(seriePosterResponse.GetValue()));
                Dispatcher.UIThread.Post(() => serie.Poster = poster);
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to load the new poster for serie with ID: " + serieId, e, Environment.StackTrace);
            }
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to receive the new poster for serie with ID: " + serieId, e, Environment.StackTrace);
        }
    }

    private async void LoadLibrary(long libraryId)
    {
        try
        {
            Optional<LibraryDto> libraryResponse = await ManaxApiLibraryClient.GetLibraryAsync(libraryId);
            if (libraryResponse.Failed)
            {
                InfoEmitted?.Invoke(this, libraryResponse.Error);
                return;
            }

            Dispatcher.UIThread.Post(() => Library = libraryResponse.GetValue());
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to load the library with ID: " + libraryId, e, Environment.StackTrace);
        }
    }

    public void DeleteLibrary()
    {
        if (Library == null) return;
        Task.Run(async () =>
        {
            Optional<bool> deleteLibraryResponse = await ManaxApiLibraryClient.DeleteLibraryAsync(Library.Id);
            if (deleteLibraryResponse.Failed)
                PageChangedRequested?.Invoke(this, new HomePageViewModel());
            else
                InfoEmitted?.Invoke(this, "Library '" + Library.Name + "' was correctly deleted");
        });
    }

    private async void LoadSeries(long libraryId)
    {
        try
        {
            Optional<List<long>> searchResultResponse =
                await ManaxApiSerieClient.GetSearchResult(new Search { IncludedLibraries = [libraryId] });
            if (searchResultResponse.Failed)
            {
                InfoEmitted?.Invoke(this, searchResultResponse.Error);
                return;
            }

            foreach (long serieId in searchResultResponse.GetValue())
            {
                lock (_serieLock)
                {
                    if (Series.FirstOrDefault(s => s.Info.Id == serieId) != null) continue;
                }

                Optional<SerieDto> serieInfoAsync = await ManaxApiSerieClient.GetSerieInfoAsync(serieId);
                if (serieInfoAsync.Failed)
                {
                    InfoEmitted?.Invoke(this, serieInfoAsync.Error);
                    continue;
                }

                ClientSerie serie = new() { Info = serieInfoAsync.GetValue() };

                Optional<byte[]> seriePosterAsync = await ManaxApiSerieClient.GetSeriePosterAsync(serieId);
                if (seriePosterAsync.Failed)
                    InfoEmitted?.Invoke(this, seriePosterAsync.Error);
                else
                    try
                    {
                        serie.Poster = new Bitmap(new MemoryStream(seriePosterAsync.GetValue()));
                    }
                    catch (Exception e)
                    {
                        Logger.LogError("Failed to convert byte[] to image", e, Environment.StackTrace);
                        InfoEmitted?.Invoke(this, "Invalid image received for serie " + serieId);
                        serie.Poster = null;
                    }

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
            IsFolderPickerOpen = false;

            if (folders.Count == 0) return;
            string folderPath = folders[0].Path.LocalPath;
            if (string.IsNullOrEmpty(folderPath)) return;

            Optional<bool> uploadSerieResponse = await UploadApiUploadClient.UploadSerieAsync(folderPath, Library.Id);
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