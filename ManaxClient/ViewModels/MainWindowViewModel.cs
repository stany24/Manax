using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Controls.Popups;
using ManaxClient.Controls.Popups.Library;
using ManaxClient.Controls.Popups.SavePoint;
using ManaxClient.Models;
using ManaxClient.Models.History;
using ManaxClient.ViewModels.Home;
using ManaxClient.ViewModels.Issue;
using ManaxClient.ViewModels.Library;
using ManaxClient.ViewModels.Login;
using ManaxClient.ViewModels.Rank;
using ManaxClient.ViewModels.Settings;
using ManaxClient.ViewModels.Stats;
using ManaxClient.ViewModels.User;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Library;
using ManaxLibrary.DTO.SavePoint;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly PageHistoryManager _history = new();
    [ObservableProperty] private ObservableCollection<string> _infos = [];
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private bool _isOwner;
    [ObservableProperty] private ObservableCollection<LibraryDto> _libraries = [];
    [ObservableProperty] private Popup? _popup;
    [ObservableProperty] private ObservableCollection<TaskItem> _runningTasks = [];
    [ObservableProperty] private Thickness _pageMargin = new(0, 0, 0, 0);

    public MainWindowViewModel()
    {
        _history.OnPageChanged += _ =>
        {
            if (CurrentPageViewModel == null) return;
            CurrentPageViewModel.Admin = IsAdmin;
            CurrentPageViewModel.Owner = IsOwner;
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
            Dispatcher.UIThread.Post(() =>
            {
                IsAdmin = login.IsAdmin();
                IsOwner = login.IsOwner();
                ServerNotification.OnRunningTasks += OnRunningTasksHandler;
                ServerNotification.OnLibraryCreated += OnLibraryCreatedHandler;
                ServerNotification.OnLibraryUpdated += OnLibraryUpdatedHandler;
                ServerNotification.OnLibraryDeleted += OnLibraryDeletedHandler;
                LoadLibraries();
            });
        };

        SetPage(new LoginPageViewModel());
    }

    public bool CanGoBack => _history.CanGoBack;
    public bool CanGoForward => _history.CanGoForward;
    public PageViewModel? CurrentPageViewModel => _history.CurrentPage;

    ~MainWindowViewModel()
    {
        ServerNotification.OnRunningTasks -= OnRunningTasksHandler;
        ServerNotification.OnLibraryCreated -= OnLibraryCreatedHandler;
        ServerNotification.OnLibraryUpdated -= OnLibraryUpdatedHandler;
        ServerNotification.OnLibraryDeleted -= OnLibraryDeletedHandler;
    }

    private void OnRunningTasksHandler(Dictionary<string, int> tasks)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            RunningTasks.Clear();

            foreach (KeyValuePair<string, int> task in tasks)
                RunningTasks.Add(new TaskItem { TaskName = task.Key, Number = task.Value });
        });
    }

    private void OnLibraryCreatedHandler(LibraryDto library)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            Libraries.Add(library);
            ShowInfo($"Library '{library.Name}' was created");
        });
    }

    private void OnLibraryUpdatedHandler(LibraryDto library)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            LibraryDto? old = Libraries.FirstOrDefault(l => l.Id == library.Id);
            if (old != null) Libraries.Remove(old);
            Libraries.Add(library);
            ShowInfo($"Library '{old?.Name ?? "null"}' was updated to '{library.Name}'");
        });
    }

    private void OnLibraryDeletedHandler(long libraryId)
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            LibraryDto? library = Libraries.FirstOrDefault(l => l.Id == libraryId);
            if (library == null) return;
            Libraries.Remove(library);
            ShowInfo($"Library '{library.Name}' was deleted");
        });
    }

    private void LoadLibraries()
    {
        Task.Run(async () =>
        {
            Optional<List<long>> libraryIdsResponse = await ManaxApiLibraryClient.GetLibraryIdsAsync();
            if (libraryIdsResponse.Failed)
            {
                ShowInfo(libraryIdsResponse.Error);
                return;
            }

            Dispatcher.UIThread.Invoke(() => { Libraries.Clear(); });
            List<long> ids = libraryIdsResponse.GetValue();
            foreach (long id in ids)
            {
                Optional<LibraryDto> libraryResponse = await ManaxApiLibraryClient.GetLibraryAsync(id);
                if (libraryResponse.Failed)
                {
                    ShowInfo(libraryResponse.Error);
                    continue;
                }

                Dispatcher.UIThread.Post(() => Libraries.Add(libraryResponse.GetValue()));
            }
        });
    }

    private void SetPopup(Popup? popup)
    {
        Popup = popup;
        if (Popup is not null) Popup.Closed += (_, _) => Popup = null;
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

    private void SetPage(PageViewModel page)
    {
        _history.SetPage(page);
        OnPropertyChanged(nameof(CurrentPageViewModel));
    }

    public void CreateLibrary()
    {
        LibraryCreatePopup popup = new();
        popup.CloseRequested += async void (_, _) =>
        {
            try
            {
                popup.Close();
                LibraryCreateDto? savePoint = popup.GetResult();
                if (savePoint == null) return;
                Optional<long> postLibraryResponse = await ManaxApiLibraryClient.PostLibraryAsync(savePoint);
                if (postLibraryResponse.Failed) ShowInfo(postLibraryResponse.Error);
            }
            catch (Exception e)
            {
                Logger.LogError("Error creating library", e, Environment.StackTrace);
                Infos.Add("Error creating library");
            }
        };
        SetPopup(popup);
    }

    public void ShowLibrary(long libraryId)
    {
        SetPage(new LibraryPageViewModel(libraryId));
    }

    public void ChangePageHome()
    {
        SetPage(new HomePageViewModel());
    }

    public void ChangePageUserIssues()
    {
        SetPage(new UserIssuesPageViewModel());
    }

    public void ChangePageAutomaticIssues()
    {
        SetPage(new AutomaticIssuesPageViewModel());
    }

    public void ChangePageUsers()
    {
        SetPage(new UsersPageViewModel());
    }

    public void ChangePageRanks()
    {
        SetPage(new RankPageViewModel());
    }

    public void ChangePageSettings()
    {
        SetPage(new SettingsPageViewModel());
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