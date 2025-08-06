using System;
using System.Collections.Generic;
using ManaxClient.ViewModels;
using ManaxClient.ViewModels.Login;

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

    private void SetCurrent(PageViewModel page)
    {
        OnPageChanging?.Invoke(page);
        CurrentPage = page;
        OnPageChanged?.Invoke(page);
    }

    public void SetPage(PageViewModel page)
    {
        if (!_navigatingHistory && !ReferenceEquals(CurrentPage, page))
        {
            if (CurrentPage is LoginPageViewModel)
                _backStack.Clear();
            else if (CurrentPage != null)
                _backStack.Push(CurrentPage);
            _forwardStack.Clear();
        }

        if (ReferenceEquals(CurrentPage, page)) return;
        SetCurrent(page);
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