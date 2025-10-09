using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.ViewModels.Pages.Serie;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;

namespace ManaxClient.ViewModels.Pages.Library;

public partial class LibraryPageViewModel : PageViewModel
{
    [ObservableProperty] private Models.Library _library;
    
    [ObservableProperty] private string _seriesCountText = string.Empty;
    [ObservableProperty] private string _deleteText = string.Empty;
    [ObservableProperty] private string _emptyLibraryTitle = string.Empty;
    [ObservableProperty] private string _emptyLibraryDescription = string.Empty;
    [ObservableProperty] private string _librarySeriesTitle = string.Empty;

    public LibraryPageViewModel(Models.Library library)
    {
        Library = library;
        BindLocalizedStrings();
    }

    private void BindLocalizedStrings()
    {
        Localize(() => SeriesCountText, "LibraryPage.SeriesCount", () => Library.Series.Count);
        Localize(() => DeleteText, "LibraryPage.Delete");
        Localize(() => EmptyLibraryTitle, "LibraryPage.EmptyLibrary.Title");
        Localize(() => EmptyLibraryDescription, "LibraryPage.EmptyLibrary.Description");
        Localize(() => LibrarySeriesTitle, "LibraryPage.LibrarySeries.Title");
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