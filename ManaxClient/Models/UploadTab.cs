using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.ViewModels.Pages;

namespace ManaxClient.Models;

public partial class UploadTab:ObservableObject
{
    [ObservableProperty] private string _title;
    [ObservableProperty] private PageViewModel _content;
    
    public UploadTab(string title,PageViewModel content)
    {
        _title = title;
        _content = content;
    }
}