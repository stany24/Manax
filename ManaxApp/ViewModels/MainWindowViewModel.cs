using CommunityToolkit.Mvvm.ComponentModel;
using ManaxApp.ViewModels.Login;

namespace ManaxApp.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private PageViewModel _currentPageViewModel;

    public MainWindowViewModel()
    {
        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(CurrentPageViewModel))
            {
                CurrentPageViewModel.PageChangedRequested += (_, e) =>
                {
                    CurrentPageViewModel = e;
                };
            }
        };
        CurrentPageViewModel = new LoginPageViewModel();
    }
}