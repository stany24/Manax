using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.ViewModels.Pages.Serie;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;

namespace ManaxClient.ViewModels.Pages.Library;

public partial class LibraryPageViewModel:PageViewModel
{
    [ObservableProperty] private Models.Library _library;

    public LibraryPageViewModel(Models.Library library)
    {
        Library = library;
    }

    public void MoveToSeriePage(Models.Serie serie)
    {
        SeriePageViewModel seriePageViewModel = new(serie);
        PageChangedRequested?.Invoke(this, seriePageViewModel);
    }

    public void DeleteLibrary()
    {
        Task.Run(async () =>
        {
            Optional<bool> deleteLibraryResponse = await ManaxApiLibraryClient.DeleteLibraryAsync(Library.Id);
            if (deleteLibraryResponse.Failed)
                InfoEmitted?.Invoke(this, "Failed to delete Library '" + Library.Name + "'");
        });
    }
}