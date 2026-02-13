using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using DynamicData.Binding;
using ManaxClient.Event;
using ManaxClient.Manager;
using ManaxClient.Models;
using ManaxClient.Models.History;
using ManaxClient.Models.Server.Sources;
using ManaxClient.ViewModels.Pages;
using ManaxClient.ViewModels.Pages.Home;
using ManaxClient.ViewModels.Pages.Issue;
using ManaxClient.ViewModels.Pages.Library;
using ManaxClient.ViewModels.Pages.Login;
using ManaxClient.ViewModels.Pages.Person;
using ManaxClient.ViewModels.Pages.Rank;
using ManaxClient.ViewModels.Pages.Settings;
using ManaxClient.ViewModels.Pages.Stats;
using ManaxClient.ViewModels.Pages.Tag;
using ManaxClient.ViewModels.Pages.Upload;
using ManaxClient.ViewModels.Pages.User;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;
using ManaxClient.Models.Server.Data;

namespace ManaxClient.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly PageHistoryManager _history = new();
    private readonly ReadOnlyObservableCollection<Library> _libraries;
    private readonly IDisposable _librariesSubscription;
    private readonly Dictionary<string, CancellationTokenSource> _infoCancellationTokens = new();
    private readonly Lock _infoCancellationLock = new();
    
    [ObservableProperty] private ObservableCollection<string> _infos = [];
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private Controls.Popups.Popup? _popup;
    [ObservableProperty] private ObservableCollection<TaskItem> _runningTasks = new([]);
    [ObservableProperty] private FeatureManager _featureManager = new();
    [ObservableProperty] private PermissionManager _permissionManager = new();
    [ObservableProperty] private ChapterSource _chapterSource = new();
    [ObservableProperty] private IssueSource _issueSource = new();
    [ObservableProperty] private LibrarySource _librarySource = new();
    [ObservableProperty] private PersonSource _personSource = new();
    [ObservableProperty] private ProblemSource _problemSource = new();
    [ObservableProperty] private RankSource _rankSource = new();
    [ObservableProperty] private RoleSource _roleSource = new();
    [ObservableProperty] private SerieSource _serieSource = new();
    [ObservableProperty] private TagSource _tagSource = new();
    [ObservableProperty] private UserSource _userSource = new();
    
    public static MainWindowViewModel Instance { get; private set; } = new();
    
    public MainWindowViewModel()
    {
        Instance = this;
        WeakReferenceMessenger.Default.Register<NotificationMessage>(this, (_, m) => { ShowInfo(m.Value); });
        WeakReferenceMessenger.Default.Register<PopupChangeMessage>(this, (_, m) => { SetPopup(m.Value); });
        WeakReferenceMessenger.Default.Register<PageChangeMessage>(this, (_, m) => { SetPage(m.Value);});
        SortExpressionComparer<Library> comparer = SortExpressionComparer<Library>.Descending(library => library.Name);
        _librariesSubscription = LibrarySource.Libraries
            .Connect()
            .SortAndBind(out _libraries, comparer)
            .Subscribe();

        _history.OnPageChanged += _ =>
        {
            if (CurrentPageViewModel == null) return;
            CurrentPageViewModel.Admin = IsAdmin;
            CurrentPageViewModel.PageChangedRequested += (_, e) => { SetPage(e); };
            CurrentPageViewModel.PreviousRequested += (_, _) => GoBack();
            CurrentPageViewModel.NextRequested += (_, _) => GoForward();
        };

        LoginPageViewModel loginPage = new();
        loginPage.PageChangedRequested += (_, _) =>
        {
            IsAdmin = loginPage.IsAdmin();
            NotificationReceiver.OnRunningTasks += OnRunningTasks;
            NotificationReceiver.OnChapterUploadFailed += OnChapterUploadFailed;

            RoleSource.LoadRoles();
            LibrarySource.LoadLibraries();
            PersonSource.LoadPersons();
            TagSource.LoadTags();
        };
        SetPage(loginPage);
    }

    public ReadOnlyObservableCollection<Library> Libraries => _libraries;

    public bool CanGoBack => _history.CanGoBack;
    public bool CanGoForward => _history.CanGoForward;
    public PageViewModel? CurrentPageViewModel => _history.CurrentPage;

    ~MainWindowViewModel()
    {
        NotificationReceiver.OnRunningTasks -= OnRunningTasks;
        NotificationReceiver.OnChapterUploadFailed -= OnChapterUploadFailed;
        _librariesSubscription.Dispose();
        
        lock (_infoCancellationLock)
        {
            foreach (CancellationTokenSource cts in _infoCancellationTokens.Values)
            {
                cts.Cancel();
                cts.Dispose();
            }
            _infoCancellationTokens.Clear();
        }
    }

    private void OnRunningTasks(Dictionary<string, int> tasks)
    {
        Dispatcher.UIThread.Post(() =>
        {
            RunningTasks.Clear();

            foreach (KeyValuePair<string, int> task in tasks)
                RunningTasks.Add(new TaskItem { TaskName = task.Key, Number = task.Value });
        });
    }

    private void OnChapterUploadFailed(string chapterPath)
    {
        ShowInfo($"Chapter upload failed: {chapterPath}");
    }

    public async void Logout()
    {
        try
        {
            Optional<bool> logoutAsync = await ManaxApiUserClient.LogoutAsync();
            if (logoutAsync.Failed)
            {
                Logger.LogFailure(logoutAsync.Error);
                ShowInfo(logoutAsync.Error);
                return;
            }

            ClearAllNotifications();
            SetPage(new LoginPageViewModel());
        }
        catch (Exception e)
        {
            const string error = "Failed to logout from server";
            Logger.LogError(error, e);
            ShowInfo(error);
        }
    }

    private void ClearAllNotifications()
    {
        lock (_infoCancellationLock)
        {
            foreach (CancellationTokenSource cts in _infoCancellationTokens.Values)
            {
                cts.Cancel();
                cts.Dispose();
            }
            _infoCancellationTokens.Clear();
        }
        
        Dispatcher.UIThread.Post(() => { Infos.Clear(); });
    }

    private void SetPopup(Controls.Popups.Popup? popup)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Popup = popup;
            Popup?.Closed += (_, _) => Popup = null;
        });
    }

    private void ShowInfo(string info)
    {
        Dispatcher.UIThread.Post(() => { Infos.Add(info); });
        
        CancellationTokenSource cts = new();
        lock (_infoCancellationLock)
        {
            _infoCancellationTokens[info] = cts;
        }
        
        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(10), cts.Token);
                
                Dispatcher.UIThread.Post(() => 
                { 
                    Infos.Remove(info);
                    lock (_infoCancellationLock)
                    {
                        _infoCancellationTokens.Remove(info);
                    }
                });
            }
            catch (OperationCanceledException)
            {
                // Ignored
            }
            finally
            {
                cts.Dispose();
            }
        }, cts.Token);
    }

    public void DismissNotification(string info)
    {
        lock (_infoCancellationLock)
        {
            if (_infoCancellationTokens.TryGetValue(info, out CancellationTokenSource? cts))
            {
                cts.Cancel();
                cts.Dispose();
                _infoCancellationTokens.Remove(info);
            }
        }
        
        Dispatcher.UIThread.Post(() => { Infos.Remove(info); });
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

    public void ChangePagePersons()
    {
        SetPage(new PersonPageViewModel());
    }

    public void ChangePageSettings()
    {
        SetPage(new SettingsServerPageViewModel());
    }

    public void ChangePageAppSettings()
    {
        SetPage(new SettingsAppViewModel());
    }

    public void ChangePageFeatures()
    {
        SetPage(new SettingsFeaturesPageViewModel());
    }

    public void ChangePageUserStats()
    {
        SetPage(new UserStatsPageViewModel());
    }

    public void ChangePageServerStats()
    {
        SetPage(new ServerStatsPageViewModel());
    }

    public void ChangeUploadPage()
    {
        SetPage(new UploadPageViewModel());
    }
}