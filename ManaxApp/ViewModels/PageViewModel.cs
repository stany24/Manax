using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxApp.ViewModels;

public abstract partial class PageViewModel : ObservableObject
{
    [ObservableProperty] private bool _admin;
    [ObservableProperty] private bool _controlBarVisible;
    public EventHandler<PageViewModel>? PageChangedRequested;
}