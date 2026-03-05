using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using DynamicData.Binding;
using ManaxClient.Event;
using ManaxClient.Localization.Localizer;
using ManaxClient.Manager;
using ManaxClient.Models;
using ManaxClient.Models.History;
using ManaxClient.Models.Server.Data;
using ManaxClient.Models.Server.Sources;
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

namespace ManaxClient.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly Lock _infoCancellationLock = new();
    private readonly Dictionary<Notification, CancellationTokenSource> _infoCancellationTokens = new();
    private readonly ReadOnlyObservableCollection<Library> _libraries;
    private readonly IDisposable _librariesSubscription;
    [ObservableProperty] private ChapterSource _chapterSource = new();
    [ObservableProperty] private FeatureManager _featureManager = new();
    [ObservableProperty] private PageHistoryManager _history;
    [ObservableProperty] private ObservableCollection<Notification> _infos = [];
    [ObservableProperty] private IssueSource _issueSource = new();
    [ObservableProperty] private LibrarySource _librarySource = new();
    [ObservableProperty] private PermissionManager _permissionManager = new();
    [ObservableProperty] private PersonSource _personSource = new();
    [ObservableProperty] private Controls.Popups.Popup? _popup;
    [ObservableProperty] private ProblemSource _problemSource = new();
    [ObservableProperty] private RankSource _rankSource = new();
    [ObservableProperty] private RoleSource _roleSource = new();
    [ObservableProperty] private ObservableCollection<TaskItem> _runningTasks = new([]);
    [ObservableProperty] private SerieSource _serieSource = new();
    [ObservableProperty] private bool _sideBarVisible = true;
    [ObservableProperty] private TagSource _tagSource = new();
    [ObservableProperty] private UserSource _userSource = new();

    public MainWindowViewModel()
    {
        Instance = this;
        WeakReferenceMessenger.Default.Register<NotificationMessage>(this, (_, m) => { ShowInfo(m.Value); });
        WeakReferenceMessenger.Default.Register<PopupChangeMessage>(this, (_, m) => { SetPopup(m.Value); });
        WeakReferenceMessenger.Default.Register<LoggedInMessage>(this, (_, _) => OnLogin());
        WeakReferenceMessenger.Default.Register<LoggedOutMessage>(this, (_, _) => OnLogout());
        SortExpressionComparer<Library> comparer = SortExpressionComparer<Library>.Descending(library => library.Name);
        _librariesSubscription = LibrarySource.Libraries
            .Connect()
            .SortAndBind(out _libraries, comparer)
            .Subscribe();

        History = new PageHistoryManager(new LoginPageViewModel());
    }

    public static MainWindowViewModel Instance { get; private set; } = null!;

    public ReadOnlyObservableCollection<Library> Libraries => _libraries;

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

    private void OnLogin()
    {
        NotificationReceiver.OnRunningTasks += OnRunningTasks;
        NotificationReceiver.OnChapterUploadFailed += OnChapterUploadFailed;
    }

    private void OnLogout()
    {
        NotificationReceiver.OnRunningTasks -= OnRunningTasks;
        NotificationReceiver.OnChapterUploadFailed -= OnChapterUploadFailed;
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
        const string key = "Mainwindow.Upload.Failed";
        ShowInfo(new Notification(key, [chapterPath]));
        Logger.LogWarning("Failed to upload chapter: " + chapterPath);
    }

    public async void Logout()
    {
        try
        {
            Optional<bool> logoutAsync = await ManaxApiUserClient.LogoutAsync();
            if (logoutAsync.Failed)
            {
                Logger.LogFailure(Localizer.Get(logoutAsync.Error));
                ShowInfo(new Notification(logoutAsync.Error));
                return;
            }

            ClearAllNotifications();
            WeakReferenceMessenger.Default.Send(new LoggedOutMessage());
            WeakReferenceMessenger.Default.Send(new PageChangeMessage(new LoginPageViewModel()));
        }
        catch (Exception e)
        {
            const string key = "Mainwindow.Logout.Failed";
            Logger.LogError("Unknown error while logging out", e);
            ShowInfo(new Notification(key));
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

    private void ShowInfo(Notification notification)
    {
        Dispatcher.UIThread.Post(() => { Infos.Add(notification); });
        notification.RemoveRequested += (_,_) => DismissNotification(notification);

        CancellationTokenSource cts = new();
        lock (_infoCancellationLock)
        {
            _infoCancellationTokens[notification] = cts;
        }

        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(10), cts.Token);

                Dispatcher.UIThread.Post(() =>
                {
                    Infos.Remove(notification);
                    lock (_infoCancellationLock)
                    {
                        _infoCancellationTokens.Remove(notification);
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

    private void DismissNotification(Notification notification)
    {
        lock (_infoCancellationLock)
        {
            if (_infoCancellationTokens.TryGetValue(notification, out CancellationTokenSource? cts))
            {
                cts.Cancel();
                cts.Dispose();
                _infoCancellationTokens.Remove(notification);
            }
        }

        Dispatcher.UIThread.Post(() => { Infos.Remove(notification); });
    }

    public void OpenSideBar()
    {
        SideBarVisible = true;
    }

    public void CloseSideBar()
    {
        SideBarVisible = false;
    }

    public void ShowLibrary(Library library)
    {
        WeakReferenceMessenger.Default.Send(new PageChangeMessage(new LibraryPageViewModel(library)));
    }

    public void ChangePageHome()
    {
        WeakReferenceMessenger.Default.Send(new PageChangeMessage(new HomePageViewModel()));
    }

    public void ChangePageIssues()
    {
        WeakReferenceMessenger.Default.Send(new PageChangeMessage(new IssuesPageViewModel()));
    }

    public void ChangePageUsers()
    {
        WeakReferenceMessenger.Default.Send(new PageChangeMessage(new UsersPageViewModel()));
    }

    public void ChangePageRanks()
    {
        WeakReferenceMessenger.Default.Send(new PageChangeMessage(new RankPageViewModel()));
    }

    public void ChangePageTags()
    {
        WeakReferenceMessenger.Default.Send(new PageChangeMessage(new TagPageViewModel()));
    }

    public void ChangePagePersons()
    {
        WeakReferenceMessenger.Default.Send(new PageChangeMessage(new PersonPageViewModel()));
    }

    public void ChangePageSettings()
    {
        WeakReferenceMessenger.Default.Send(new PageChangeMessage(new SettingsServerPageViewModel()));
    }

    public void ChangePageAppSettings()
    {
        WeakReferenceMessenger.Default.Send(new PageChangeMessage(new SettingsAppViewModel()));
    }

    public void ChangePageFeatures()
    {
        WeakReferenceMessenger.Default.Send(new PageChangeMessage(new SettingsFeaturesPageViewModel()));
    }

    public void ChangePageUserStats()
    {
        WeakReferenceMessenger.Default.Send(new PageChangeMessage(new UserStatsPageViewModel()));
    }

    public void ChangePageServerStats()
    {
        WeakReferenceMessenger.Default.Send(new PageChangeMessage(new ServerStatsPageViewModel()));
    }

    public void ChangeUploadPage()
    {
        WeakReferenceMessenger.Default.Send(new PageChangeMessage(new UploadPageViewModel()));
    }
}