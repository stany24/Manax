using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.ViewModels.Pages;

namespace ManaxClient.Models;

public partial class UploadTab : ObservableObject
{
    [ObservableProperty] private PageViewModel _content;
    [ObservableProperty] private string _title;

    public UploadTab(string title, PageViewModel content)
    {
        _title = title;
        _content = content;
    }
}