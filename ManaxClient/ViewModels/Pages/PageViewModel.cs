using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ManaxClient.ViewModels.Pages;

public abstract partial class PageViewModel : ObservableObject
{
    [ObservableProperty] private bool _admin;
    [ObservableProperty] private bool _controlBarVisible = true;
    [ObservableProperty] private bool _hasMargin = true;
    
    public EventHandler? NextRequested { get; set; }
    public EventHandler<PageViewModel>? PageChangedRequested { get; set; }
    public EventHandler? PreviousRequested { get; set; }

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