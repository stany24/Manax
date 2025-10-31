using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxClient.ViewModels.Pages.Upload.Tab;

public partial class FetchFromSourceTabViewModel:PageViewModel
{
    public ObservableCollection<string> SourcesFolders = ["/path/to/source"];
    [ObservableProperty] private string _processingFolder = string.Empty;

    public void Fetch()
    {
    }
}