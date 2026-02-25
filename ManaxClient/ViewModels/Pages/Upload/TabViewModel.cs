using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxClient.ViewModels.Pages.Upload;

public class TabViewModel : ObservableObject
{
    public EventHandler<TabViewModel?>? NextRequested { get; set; }
}