using System;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Controls;

namespace ManaxClient.ViewModels;

public abstract partial class PageViewModel : ObservableObject
{
    [ObservableProperty] private bool _admin;
    [ObservableProperty] private bool _controlBarVisible;
    public EventHandler<string>? InfoEmitted;
    public EventHandler? NextRequested;
    public EventHandler<PageViewModel>? PageChangedRequested;
    public EventHandler<Popup>? PopupRequested;
    public EventHandler? PreviousRequested;

    public void Previous()
    {
        PreviousRequested?.Invoke(this, EventArgs.Empty);
    }

    public void Next()
    {
        NextRequested?.Invoke(this, EventArgs.Empty);
    }
}