using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxApp.ViewModels.Home;
using ManaxApp.ViewModels.Issue;
using ManaxApp.ViewModels.Library;
using ManaxApp.ViewModels.Login;
using ManaxApp.ViewModels.User;
using ManaxLibrary.ApiCaller;

namespace ManaxApp.ViewModels;

public class TaskItem : ObservableObject
{
    public string TaskName { get; set; } = string.Empty;
    public int Number { get; set; }
}

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private PageViewModel _currentPageViewModel;
    [ObservableProperty] private ObservableCollection<string> _infos = [];
    [ObservableProperty] private Control? _popup;
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private ObservableCollection<TaskItem> _runningTasks = [];

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
        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != nameof(CurrentPageViewModel)) return;
            CurrentPageViewModel.Admin = IsAdmin;
            CurrentPageViewModel.PageChangedRequested += (_, e) => { CurrentPageViewModel = e; };
            CurrentPageViewModel.PopupRequested += (_, e) => { Popup = e; };
            CurrentPageViewModel.InfoEmitted += (_, e) =>
            {
                Infos.Add(e);
            };
        };

        PropertyChanging += (_, args) =>
        {
            if (args.PropertyName != nameof(CurrentPageViewModel)) return;
            if (CurrentPageViewModel is not LoginPageViewModel login) return;
            Dispatcher.UIThread.Post(() =>
            {
                IsAdmin = login.IsAdmin();
                Popup = new Label {Content = "Test", Foreground = Brushes.White};
            });
            UpdateRunningTasks();

        };

        CurrentPageViewModel = new LoginPageViewModel();
    }

    public void ChangePageHome()
    {
        CurrentPageViewModel = new HomePageViewModel();
    }
    
    public void ChangePageLibraries()
    {
        CurrentPageViewModel = new LibrariesPageViewModel();
    }

    public void ChangePageIssues()
    {
        CurrentPageViewModel = new IssuesPageViewModel();
    }

    public void ChangePageUsers()
    {
        CurrentPageViewModel = new UsersPageViewModel();
    }
}