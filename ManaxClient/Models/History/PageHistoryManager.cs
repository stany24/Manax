using System;
using System.Collections.Generic;
using ManaxClient.ViewModels.Pages;
using ManaxClient.ViewModels.Pages.Login;

namespace ManaxClient.Models.History;

public class PageHistoryManager
{
    private readonly Stack<PageViewModel> _backStack = new();
    private readonly Stack<PageViewModel> _forwardStack = new();
    private bool _navigatingHistory;
    public PageViewModel? CurrentPage { get; private set; }

    public bool CanGoBack => _backStack.Count > 0;
    public bool CanGoForward => _forwardStack.Count > 0;

    public event Action<PageViewModel>? OnPageChanged;
    public event Action<PageViewModel>? OnPageChanging;

    private void SetCurrent(PageViewModel pageViewModel)
    {
        CurrentPage?.OnPageClosed();
        OnPageChanging?.Invoke(pageViewModel);
        CurrentPage = pageViewModel;
        OnPageChanged?.Invoke(pageViewModel);
    }

    public void SetPage(PageViewModel pageViewModel)
    {
        if (!_navigatingHistory && !ReferenceEquals(CurrentPage, pageViewModel))
        {
            if (CurrentPage is LoginPageViewModel)
                _backStack.Clear();
            else if (CurrentPage != null)
                _backStack.Push(CurrentPage);
            _forwardStack.Clear();
        }

        if (ReferenceEquals(CurrentPage, pageViewModel)) return;
        SetCurrent(pageViewModel);
    }

    public void GoBack()
    {
        if (!CanGoBack) return;
        _navigatingHistory = true;
        _forwardStack.Push(CurrentPage!);
        PageViewModel previous = _backStack.Pop();
        SetCurrent(previous);
        _navigatingHistory = false;
    }

    public void GoForward()
    {
        if (!CanGoForward) return;
        _navigatingHistory = true;
        _backStack.Push(CurrentPage!);
        PageViewModel next = _forwardStack.Pop();
        SetCurrent(next);
        _navigatingHistory = false;
    }
}