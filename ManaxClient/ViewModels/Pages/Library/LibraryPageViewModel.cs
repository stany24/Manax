using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.ViewModels.Pages.Serie;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.Notifications;

namespace ManaxClient.ViewModels.Pages.Library;

public partial class LibraryPageViewModel:PageViewModel
{
    [ObservableProperty] private Models.Library? _library;

    public LibraryPageViewModel(long libraryId)
    {
        ServerNotification.OnLibraryDeleted += OnLibraryDeleted;
        Library = new Models.Library(libraryId);
        Library.LoadInfo();
    }

    private void OnLibraryDeleted(long id)
    {
        if (Library?.Id != id) return;
        PageChangedRequested?.Invoke(this, new Home.HomePageViewModel());
    }

    public void MoveToSeriePage(Models.Serie serie)
    {
        SeriePageViewModel seriePageViewModel = new(serie.Id);
        PageChangedRequested?.Invoke(this, seriePageViewModel);
    }

    public void DeleteLibrary()
    {
        if (Library == null) return;
        Task.Run(async () =>
        {
            Optional<bool> deleteLibraryResponse = await ManaxApiLibraryClient.DeleteLibraryAsync(Library.Id);
            if (deleteLibraryResponse.Failed)
                InfoEmitted?.Invoke(this, "Failed to delete Library '" + Library.Name + "'");
        });
    }
}