using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models;

namespace ManaxClient.ViewModels.Pages.Upload.Tab;

public partial class AutoUploadTabViewModel:TabViewModel
{
    [ObservableProperty] private string _processFolder = string.Empty;
    
    public void Upload()
    {
    }
}