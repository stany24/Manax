using System.Collections.Generic;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ManaxClient.Event;
using ManaxClient.ViewModels.Pages;
using ManaxClient.ViewModels.Pages.Login;

namespace ManaxClient.Models.History;

public partial class PageHistoryManager : ObservableObject
{
    private readonly Stack<PageViewModel> _backStack = new();
    private readonly Stack<PageViewModel> _forwardStack = new();
    private readonly Lock _lock = new();
    [ObservableProperty] private PageViewModel _currentPage;

    public PageHistoryManager(PageViewModel pageViewModel)
    {
        WeakReferenceMessenger.Default.Register<PageChangeMessage>(this, (_, m) => { SetPage(m.Value); });
        WeakReferenceMessenger.Default.Register<PreviousPageMessage>(this, (_, _) => { GoBack(); });
        WeakReferenceMessenger.Default.Register<NextPageMessage>(this, (_, _) => { GoForward(); });
        CurrentPage = pageViewModel;
    }

    public bool CanGoBack => _backStack.Count > 0;
    public bool CanGoForward => _forwardStack.Count > 0;

    private void SetCurrent(PageViewModel pageViewModel)
    {
        lock (_lock)
        {
            CurrentPage.OnPageClosed();
            CurrentPage = pageViewModel;
        }
    }

    private void SetPage(PageViewModel pageViewModel)
    {
        if (CurrentPage is not LoginPageViewModel)
            _backStack.Push(CurrentPage);
        _forwardStack.Clear();
        SetCurrent(pageViewModel);
    }

    private void GoBack()
    {
        if (!CanGoBack) return;
        _forwardStack.Push(CurrentPage);
        PageViewModel previous = _backStack.Pop();
        SetCurrent(previous);
    }

    private void GoForward()
    {
        if (!CanGoForward) return;
        _backStack.Push(CurrentPage);
        PageViewModel next = _forwardStack.Pop();
        SetCurrent(next);
    }
}