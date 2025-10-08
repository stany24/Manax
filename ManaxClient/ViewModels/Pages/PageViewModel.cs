using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ManaxClient.ViewModels.Pages;

public abstract partial class PageViewModel : LocalizedViewModel
{
    [ObservableProperty] private bool _admin;
    [ObservableProperty] private bool _controlBarVisible = true;
    [ObservableProperty] private bool _hasMargin = true;

    public EventHandler<string>? InfoEmitted { get; set; }
    public EventHandler? NextRequested { get; set; }
    public EventHandler<PageViewModel>? PageChangedRequested { get; set; }
    public EventHandler<Controls.Popups.Popup>? PopupRequested { get; set; }
    public EventHandler? PreviousRequested { get; set; }

    public ICommand InfoEmittedCommand => new RelayCommand<string>(info =>
    {
        if (info != null) InfoEmitted?.Invoke(this, info);
    });

    public ICommand PopupRequestedCommand => new RelayCommand<Controls.Popups.Popup>(popup =>
    {
        if (popup != null) PopupRequested?.Invoke(this, popup);
    });

    public void Previous()
    {
        PreviousRequested?.Invoke(this, EventArgs.Empty);
    }

    public void Next()
    {
        NextRequested?.Invoke(this, EventArgs.Empty);
    }

    public virtual void OnPageClosed()
    {
    }
}