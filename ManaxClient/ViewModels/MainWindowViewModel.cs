using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using ManaxClient.Models;
using ManaxClient.Models.Collections;
using ManaxClient.Models.History;
using ManaxClient.Models.Issue;
using ManaxClient.Models.Sources;
using ManaxClient.ViewModels.Pages;
using ManaxClient.ViewModels.Pages.Home;
using ManaxClient.ViewModels.Pages.Issue;
using ManaxClient.ViewModels.Pages.Library;
using ManaxClient.ViewModels.Pages.Login;
using ManaxClient.ViewModels.Pages.Rank;
using ManaxClient.ViewModels.Pages.Settings;
using ManaxClient.ViewModels.Pages.Stats;
using ManaxClient.ViewModels.Pages.Tag;
using ManaxClient.ViewModels.Pages.User;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.Notifications;

namespace ManaxClient.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly PageHistoryManager _history = new();
    [ObservableProperty] private ObservableCollection<string> _infos = [];
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private Thickness _pageMargin = new(0, 0, 0, 0);
    [ObservableProperty] private Controls.Popups.Popup? _popup;
    [ObservableProperty] private SortedObservableCollection<TaskItem> _runningTasks = new([]);
    
    private readonly ReadOnlyObservableCollection<Library> _libraries;
    public ReadOnlyObservableCollection<Library> Libraries => _libraries;
    
    private readonly IDisposable _librariesSubscription;

    public MainWindowViewModel()
    {
        SortExpressionComparer<Library> comparer = SortExpressionComparer<Library>.Descending(library => library.Name);
        _librariesSubscription = LibrarySource.Libraries
            .Connect()
            .SortAndBind(out _libraries, comparer)
            .Subscribe();
        
        RunningTasks.SortingSelector = t => t.TaskName;
        _history.OnPageChanged += _ =>
        {
            if (CurrentPageViewModel == null) return;
            CurrentPageViewModel.Admin = IsAdmin;
            CurrentPageViewModel.PageChangedRequested += (_, e) => { SetPage(e); };
            CurrentPageViewModel.PopupRequested += (_, e) => { SetPopup(e); };
            CurrentPageViewModel.InfoEmitted += (_, e) => { ShowInfo(e); };
            CurrentPageViewModel.PreviousRequested += (_, _) => GoBack();
            CurrentPageViewModel.NextRequested += (_, _) => GoForward();
            PageMargin = CurrentPageViewModel.HasMargin ? new Thickness(20) : new Thickness(0);
        };

        _history.OnPageChanging += _ =>
        {
            if (CurrentPageViewModel is not LoginPageViewModel login) return;
            Library.ErrorEmitted += (_,e) => ShowInfo(e);
            Serie.ErrorEmitted += (_,e) => ShowInfo(e);
            Chapter.ErrorEmitted += (_,e) => ShowInfo(e);
            RankSource.ErrorEmitted += (_,e) => ShowInfo(e);
            TagSource.ErrorEmitted += (_,e) => ShowInfo(e);
            UserSource.ErrorEmitted += (_,e) => ShowInfo(e);
            IssueSource.ErrorEmitted += (_,e) => ShowInfo(e);
            IsAdmin = login.IsAdmin();
            ServerNotification.OnRunningTasks += OnRunningTasks;
            ServerNotification.OnPermissionModified += OnPermissionModified;
            Task.Run(LoadPermissions);
            LibrarySource.LoadLibraries();
        };

        SetPage(new LoginPageViewModel());
    }

    public bool CanGoBack => _history.CanGoBack;
    public bool CanGoForward => _history.CanGoForward;
    public PageViewModel? CurrentPageViewModel => _history.CurrentPage;

    ~MainWindowViewModel()
    {
        ServerNotification.OnRunningTasks -= OnRunningTasks;
        ServerNotification.OnPermissionModified -= OnPermissionModified;
        _librariesSubscription.Dispose();
    }

    private void OnRunningTasks(Dictionary<string, int> tasks)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            RunningTasks.Clear();

            foreach (KeyValuePair<string, int> task in tasks)
                RunningTasks.Add(new TaskItem { TaskName = task.Key, Number = task.Value });
        });
    }

    public async void Logout()
    {
        ManaxLibrary.Optional<bool> logoutAsync = await ManaxApiUserClient.LogoutAsync();
        if (logoutAsync.Failed) ShowInfo(logoutAsync.Error);
        SetPage(new LoginPageViewModel());
    }

    private void SetPopup(Controls.Popups.Popup? popup)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            Popup = popup;
            if (Popup is not null) Popup.Closed += (_, _) => Popup = null;
        });
    }

    private void ShowInfo(string info)
    {
        Dispatcher.UIThread.Invoke(() => { Infos.Add(info); });
        Task.Run(() =>
        {
            Thread.Sleep(10000);
            Dispatcher.UIThread.Invoke(() => { Infos.Remove(info); });
        });
    }

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

    private void SetPage(PageViewModel pageViewModel)
    {
        _history.SetPage(pageViewModel);
        OnPropertyChanged(nameof(CurrentPageViewModel));
    }

    public void ShowLibrary(Library library)
    {
        SetPage(new LibraryPageViewModel(library));
    }

    public void ChangePageHome()
    {
        SetPage(new HomePageViewModel());
    }

    public void ChangePageIssues()
    {
        SetPage(new IssuesPageViewModel());
    }

    public void ChangePageUsers()
    {
        SetPage(new UsersPageViewModel());
    }

    public void ChangePageRanks()
    {
        SetPage(new RankPageViewModel());
    }

    public void ChangePageTags()
    {
        SetPage(new TagPageViewModel());
    }

    public void ChangePageSettings()
    {
        SetPage(new SettingsServerPageViewModel());
    }

    public void ChangePageAppSettings()
    {
        SetPage(new SettingsAppViewModel());
    }

    public void ChangePageUserStats()
    {
        SetPage(new UserStatsPageViewModel());
    }

    public void ChangePageServerStats()
    {
        SetPage(new ServerStatsPageViewModel());
    }
}