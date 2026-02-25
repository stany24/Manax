using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ManaxClient.Event;

namespace ManaxClient.ViewModels.Pages;

public abstract partial class PageViewModel : ObservableObject
{
    [ObservableProperty] private bool _controlBarVisible = true;

    public EventHandler? PageClosed { get; set; }

    public void Previous()
    {
        WeakReferenceMessenger.Default.Send(new PreviousPageMessage());
    }

    public void Next()
    {
        WeakReferenceMessenger.Default.Send(new NextPageMessage());
    }

    public virtual void OnPageClosed()
    {
        PageClosed?.Invoke(this, EventArgs.Empty);
    }
}