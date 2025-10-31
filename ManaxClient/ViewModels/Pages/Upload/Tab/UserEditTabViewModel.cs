using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxClient.ViewModels.Pages.Upload.Tab;

public partial class UserEditTabViewModel:PageViewModel
{
    [ObservableProperty] private string _processFolder = string.Empty;
}