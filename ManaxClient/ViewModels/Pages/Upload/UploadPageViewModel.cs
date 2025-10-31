using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models;
using ManaxClient.ViewModels.Pages.Upload.Tab;

namespace ManaxClient.ViewModels.Pages.Upload;

public partial class UploadPageViewModel:PageViewModel
{
    public ObservableCollection<UploadTab> Tabs { get; } = [];
    [ObservableProperty] private UploadTab _selectedTab;

    public UploadPageViewModel()
    {
        Tabs.Add(new UploadTab("Fetch from source", new FetchFromSourceTabViewModel()));
        Tabs.Add(new UploadTab("Auto cleanup", new AutoCleanupTabViewModel()));
        Tabs.Add(new UploadTab("User edit", new UserEditTabViewModel()));
        Tabs.Add(new UploadTab("Auto upload", new AutoUploadTabViewModel()));
        Tabs.Add(new UploadTab("Manual upload", new ManualUploadTabViewModel()));
        SelectedTab = Tabs[0];
    }
}