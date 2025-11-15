using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models;

namespace ManaxClient.ViewModels.Pages.Upload.Tab;

public partial class ManualUploadTabViewModel : TabViewModel
{
    [ObservableProperty] private string _processFolder = string.Empty;
}