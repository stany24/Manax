using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxClient.ViewModels.Pages.Upload.Tab;

public partial class AutoUploadTabViewModel:PageViewModel
{
    [ObservableProperty] private string _processFolder = string.Empty;
    
    public void Upload()
    {
    }
}