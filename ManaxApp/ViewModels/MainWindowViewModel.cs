using CommunityToolkit.Mvvm.ComponentModel;
using ManaxApp.ViewModels.Home;
using ManaxApp.ViewModels.Issue;
using ManaxApp.ViewModels.Login;
using ManaxApp.ViewModels.User;

namespace ManaxApp.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private PageViewModel _currentPageViewModel;
    [ObservableProperty] private bool _isAdmin;

    public MainWindowViewModel()
    {
        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != nameof(CurrentPageViewModel)) {return;}
            CurrentPageViewModel.Admin = IsAdmin;
            CurrentPageViewModel.PageChangedRequested += (_, e) =>
            {
                CurrentPageViewModel = e;
            };
        };
        
        PropertyChanging += (_, args) =>
        {
            if (args.PropertyName != nameof(CurrentPageViewModel)) {return;}
            if (CurrentPageViewModel is LoginPageViewModel login)
            {
                IsAdmin = login.IsAdmin();
            }
        };
        
        CurrentPageViewModel = new LoginPageViewModel();
    }
    
    public void ChangePageHome()
    {
        CurrentPageViewModel = new HomePageViewModel();
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