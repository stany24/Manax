using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxApp.Controls;
using ManaxApp.Models;
using ManaxApp.ViewModels.Home;
using ManaxApp.ViewModels.Issue;
using ManaxApp.ViewModels.Library;
using ManaxApp.ViewModels.Login;
using ManaxApp.ViewModels.User;
using ManaxLibrary.ApiCaller;

namespace ManaxApp.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private ObservableCollection<string> _infos = [];
    [ObservableProperty] private Popup? _popup;
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private ObservableCollection<TaskItem> _runningTasks = [];

    private readonly PageHistoryManager _history = new();

    public bool CanGoBack => _history.CanGoBack;
    public bool CanGoForward => _history.CanGoForward;
    public PageViewModel? CurrentPageViewModel => _history.CurrentPage;

    public void GoBack()
    {
        _history.GoBack();
        OnPropertyChanged(nameof(CurrentPageViewModel));
    }

    public void GoForward()
    {
        _history.GoForward();
        OnPropertyChanged(nameof(CurrentPageViewModel));
    }

    private void SetPage(PageViewModel page)
    {
        _history.SetPage(page);
        OnPropertyChanged(nameof(CurrentPageViewModel));
    }

    private void UpdateRunningTasks()
    {
        if(!IsAdmin){return;}
        Task.Run(() =>
        {
            while (true)
            {
                Thread.Sleep(1000);
                
                Dictionary<string, int>? tasks = ManaxApiScanClient.GetTasksAsync().Result;
                if (tasks == null) { continue; }

                Dispatcher.UIThread.Invoke(() =>
                {
                    RunningTasks.Clear();

                    foreach (KeyValuePair<string, int> task in tasks)
                    {
                        RunningTasks.Add(new TaskItem { TaskName = task.Key, Number = task.Value });
                    }
                });
            }
        });
    }
    
    public MainWindowViewModel()
    {
        _history.PageChanged += _ =>
        {
            if (CurrentPageViewModel == null) return;
            CurrentPageViewModel.Admin = IsAdmin;
            CurrentPageViewModel.PageChangedRequested += (_, e) => { SetPage(e); };
            CurrentPageViewModel.PopupRequested += (_, e) =>
            {
                Popup = e;
                if (Popup is not null)
                {
                    Popup.Closed += (_, _) => Popup = null;
                }
            };
            CurrentPageViewModel.InfoEmitted += (_, e) =>
            {
                Infos.Add(e);
            };
            CurrentPageViewModel.PreviousRequested += (_, _) => GoBack();
            CurrentPageViewModel.NextRequested += (_, _) => GoForward();
        };

        _history.PageChanging += _ =>
        {
            if (CurrentPageViewModel is not LoginPageViewModel login) return;
            Dispatcher.UIThread.Post(() =>
            {
                IsAdmin = login.IsAdmin();
                UpdateRunningTasks();
            });
        };
        
        SetPage(new LoginPageViewModel());
    }

    public void ChangePageHome() => SetPage(new HomePageViewModel());
    public void ChangePageLibraries() => SetPage(new LibrariesPageViewModel());
    public void ChangePageIssues() => SetPage(new IssuesPageViewModel());
    public void ChangePageUsers() => SetPage(new UsersPageViewModel());
}