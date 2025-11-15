using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models;
using ManaxClient.ViewModels.Pages.Upload.Tab;

namespace ManaxClient.ViewModels.Pages.Upload;

public partial class UploadPageViewModel : PageViewModel
{
    [ObservableProperty] private TabViewModel _selectedTabViewModel;

    public UploadPageViewModel()
    {
        SelectedTabViewModel = new ConfigureUploadTabViewModel();
        SelectedTabViewModel.NextRequested += NextTab;
    }

    private void NextTab(object? sender, TabViewModel? e)
    {
        if (e == null) return;
        SelectedTabViewModel.NextRequested -= NextTab;
        SelectedTabViewModel = e;
        SelectedTabViewModel.NextRequested += NextTab;
    }
}