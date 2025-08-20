using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.ViewModels.Home;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Library;
using ManaxLibrary.DTO.Search;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.ViewModels.Library;

public partial class LibraryPageViewModel : BaseSeriesViewModel
{
    [ObservableProperty] private LibraryDto? _library;

    public LibraryPageViewModel(long libraryId)
    {
        Task.Run(() => { LoadLibrary(libraryId); });
        Task.Run(() => { LoadSeries(new Search { IncludedLibraries = [libraryId] }); });
        ServerNotification.OnLibraryDeleted += OnLibraryDeleted;
    }

    private void OnLibraryDeleted(long libraryId)
    {
        if (Library == null || Library.Id != libraryId) return;
        PageChangedRequested?.Invoke(this, new HomePageViewModel());
        InfoEmitted?.Invoke(this, "Library \'" + Library.Name + "\' was deleted");
    }

    protected override void OnSerieCreated(SerieDto serie)
    {
        Logger.LogInfo("A new Serie has been created in " + Library?.Name);
        if (serie.LibraryId != Library?.Id) return;
        AddSerieToCollection(serie);
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
}