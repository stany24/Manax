using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models;
using ManaxClient.ViewModels.Pages.Upload.Tab;

namespace ManaxClient.ViewModels.Pages.Upload;

public partial class UploadPageViewModel:PageViewModel
{
    private List<TabViewModel> Tabs { get; } = [];
    [ObservableProperty] private TabViewModel _selectedTabViewModel;

    public UploadPageViewModel()
    {
        Tabs.Add(new ConfigureUploadTabViewModel());
        Tabs.Add(new FetchFromSourceTabViewModel());
        Tabs.Add(new AutoCleanupTabViewModel());
        Tabs.Add(new ManualCleanupTabViewModel());
        Tabs.Add(new AutoUploadTabViewModel());
        Tabs.Add(new ManualUploadTabViewModel());
        SelectedTabViewModel = Tabs[0];
        
        foreach (TabViewModel tabViewModel in Tabs)
        {
            tabViewModel.NextRequested += (_, _) =>
            {
                int currentIndex = Tabs.IndexOf(SelectedTabViewModel);
                if (currentIndex < Tabs.Count - 1)
                {
                    SelectedTabViewModel = Tabs[currentIndex + 1];
                }
            };
        }
    }
}