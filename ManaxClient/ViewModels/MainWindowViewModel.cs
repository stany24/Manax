using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Controls;
using ManaxClient.Models;
using ManaxClient.ViewModels.Home;
using ManaxClient.ViewModels.Issue;
using ManaxClient.ViewModels.Library;
using ManaxClient.ViewModels.Login;
using ManaxClient.ViewModels.User;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTOs.Library;

namespace ManaxClient.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly PageHistoryManager _history = new();
    [ObservableProperty] private ObservableCollection<string> _infos = [];
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private Popup? _popup;
    [ObservableProperty] private ObservableCollection<TaskItem> _runningTasks = [];
    [ObservableProperty] private ObservableCollection<LibraryDTO> _libraries = [];

    public MainWindowViewModel()
    {
        _history.PageChanged += _ =>
        {
            if (CurrentPageViewModel == null) return;
            CurrentPageViewModel.Admin = IsAdmin;
            CurrentPageViewModel.PageChangedRequested += (_, e) => { SetPage(e); };
            CurrentPageViewModel.PopupRequested += (_, e) => { SetPopup(e); };
            CurrentPageViewModel.InfoEmitted += (_, e) => { ShowInfo(e); };
            CurrentPageViewModel.PreviousRequested += (_, _) => GoBack();
            CurrentPageViewModel.NextRequested += (_, _) => GoForward();
        };

        _history.PageChanging += _ =>
        {
            if (CurrentPageViewModel is not LoginPageViewModel login) return;
            Dispatcher.UIThread.Post(() =>
            {
                IsAdmin = login.IsAdmin();
                ServerNotification.OnRunningTasks += OnRunningTasksHandler;
                LoadLibraries();
            });
        };
        
        SetPage(new LoginPageViewModel());
    }

    private void OnRunningTasksHandler(Dictionary<string, int> tasks)
    {
        if (tasks == null) return;

        Dispatcher.UIThread.Invoke(() =>
        {
            RunningTasks.Clear();

            foreach (KeyValuePair<string, int> task in tasks)
                RunningTasks.Add(new TaskItem { TaskName = task.Key, Number = task.Value });
        });
    }

    private void LoadLibraries()
    {
        Task.Run(async () =>
        {
            List<long>? ids = await ManaxApiLibraryClient.GetLibraryIdsAsync();
            if (ids == null) return;
            Dispatcher.UIThread.Invoke(() => { Libraries.Clear(); });
            foreach (long id in ids)
            {
                LibraryDTO? libraryAsync = await ManaxApiLibraryClient.GetLibraryAsync(id);
                if (libraryAsync == null) continue;
                Dispatcher.UIThread.Post(() => Libraries.Add(libraryAsync));
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
        Dispatcher.UIThread.Invoke(() =>
        {
            Infos.Add(info);
        });
        Task.Run(() =>
        {
            Thread.Sleep(10000);
            Dispatcher.UIThread.Invoke(() =>
            {
                Infos.Remove(info);
            });
        });
    }

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

    public void CreateLibrary()
    {
        LibraryCreatePopup popup = new();
        popup.CloseRequested += async void (_, _) =>
        {
            try
            {
                popup.Close();
                LibraryCreateDTO? library = popup.GetResult();
                if (library == null) return;
                long? id = await ManaxApiLibraryClient.PostLibraryAsync(library);
                if (id == null)
                {
                    Infos.Add("Library creation failed");
                    return;
                }

                LibraryDTO? createdLibrary = await ManaxApiLibraryClient.GetLibraryAsync((long)id);
                if (createdLibrary == null) return;
                Dispatcher.UIThread.Post(() => Libraries.Add(createdLibrary));
                ShowLibrary(createdLibrary.Id);
            }
            catch (Exception)
            {
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
}