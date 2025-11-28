using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ManaxClient.Event;
using ManaxClient.ViewModels.Pages.Serie;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;

namespace ManaxClient.ViewModels.Pages.Library;

public partial class LibraryPageViewModel : PageViewModel
{
    [ObservableProperty] private Models.Server.Data.Library _library;

    public LibraryPageViewModel(Models.Server.Data.Library library)
    {
        Library = library;
    }

    public void MoveToSeriePage(Models.Server.Data.Serie serie)
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
                WeakReferenceMessenger.Default.Send(new NotificationMessage("Failed to delete Library '" + Library.Name + "'"));
        });
    }
}