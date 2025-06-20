using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxApp.ViewModels;

public abstract partial class PageViewModel:ObservableObject
{
    public EventHandler<PageViewModel>? PageChangedRequested;
    [ObservableProperty] private bool _controlBarVisible;
    [ObservableProperty] private bool _admin;
}