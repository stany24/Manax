using System;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxApp.Controls;

namespace ManaxApp.ViewModels;

public abstract partial class PageViewModel : ObservableObject
{
    [ObservableProperty] private bool _admin;
    [ObservableProperty] private bool _controlBarVisible;
    public EventHandler<PageViewModel>? PageChangedRequested;
    public EventHandler? PreviousRequested;
    public EventHandler? NextRequested;
    public EventHandler<Popup>? PopupRequested;
    public EventHandler<string>? InfoEmitted;

    public void Previous()
    {
        PreviousRequested?.Invoke(this, EventArgs.Empty);
    }
    
    public void Next()
    {
        NextRequested?.Invoke(this, EventArgs.Empty);
    }
}