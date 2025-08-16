using System;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Controls.Popups;

namespace ManaxClient.ViewModels;

public abstract partial class PageViewModel : ObservableObject
{
    [ObservableProperty] private bool _admin;
    [ObservableProperty] private bool _controlBarVisible = true;
    [ObservableProperty] private bool _owner;
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
    
    public virtual void OnPageClosed(){}
}