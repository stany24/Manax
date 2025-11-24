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
using Chapter = ManaxClient.Models.Server.Data.Chapter;
using Library = ManaxClient.Models.Server.Data.Library;
using Serie = ManaxClient.Models.Server.Data.Serie;

namespace ManaxClient.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly PageHistoryManager _history = new();

    private readonly ReadOnlyObservableCollection<Library> _libraries;

    private readonly IDisposable _librariesSubscription;
    [ObservableProperty] private ObservableCollection<string> _infos = [];
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private Thickness _pageMargin = new(0, 0, 0, 0);
    [ObservableProperty] private Controls.Popups.Popup? _popup;
    [ObservableProperty] private ObservableCollection<TaskItem> _runningTasks = new([]);

    public MainWindowViewModel()
    {
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
            CurrentPageViewModel.PopupRequested += (_, e) => { SetPopup(e); };
            CurrentPageViewModel.InfoEmitted += (_, e) => { ShowInfo(e); };
            CurrentPageViewModel.PreviousRequested += (_, _) => GoBack();
            CurrentPageViewModel.NextRequested += (_, _) => GoForward();
            PageMargin = CurrentPageViewModel.HasMargin ? new Thickness(20) : new Thickness(0);
        };

        LoginPageViewModel loginPage = new();
        loginPage.PageChangedRequested += (_, _) =>
        {
            Library.ErrorEmitted += (_, e) => ShowInfo(e);
            Serie.ErrorEmitted += (_, e) => ShowInfo(e);
            Chapter.ErrorEmitted += (_, e) => ShowInfo(e);
            RankSource.ErrorEmitted += (_, e) => ShowInfo(e);
            TagSource.ErrorEmitted += (_, e) => ShowInfo(e);
            UserSource.ErrorEmitted += (_, e) => ShowInfo(e);
            IssueSource.ErrorEmitted += (_, e) => ShowInfo(e);
            IsAdmin = loginPage.IsAdmin();
            ServerNotification.OnRunningTasks += OnRunningTasks;
            ServerNotification.OnPermissionModified += OnPermissionModified;
            ServerNotification.OnFeatureModified += OnFeatureModified;
            ServerNotification.OnChapterUploadFailed += OnChapterUploadFailed;
            Task.Run(LoadPermissions);
            Task.Run(LoadFeatures);

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
        ServerNotification.OnRunningTasks -= OnRunningTasks;
        ServerNotification.OnPermissionModified -= OnPermissionModified;
        ServerNotification.OnFeatureModified -= OnFeatureModified;
        ServerNotification.OnChapterUploadFailed -= OnChapterUploadFailed;
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

            SetPage(new LoginPageViewModel());
        }
        catch (Exception e)
        {
            const string error = "Failed to logout from server";
            Logger.LogError(error, e);
            ShowInfo(error);
        }
    }

    private void SetPopup(Controls.Popups.Popup? popup)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            Popup = popup;
            Popup?.Closed += (_, _) => Popup = null;
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
        SetPage(new SettingsFeaturesViewModel());
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