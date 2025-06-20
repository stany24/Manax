using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxApi.Models.User;
using ManaxApiClient;
using ManaxApp.ViewModels.Home;

namespace ManaxApp.ViewModels.Login;

public partial class LoginPageViewModel: PageViewModel
{
    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _host = "http://127.0.0.1";
    [ObservableProperty] private int _port = 5246;
    [ObservableProperty] private string _loginError = string.Empty;

    private bool _isAdmin;

    public void Login()
    {
        Task.Run(async () =>
        {
            Dispatcher.UIThread.Post(() => { LoginError = string.Empty; });
            ManaxApiConfig.SetHost(new Uri(Host + $":{Port}/"));
            string? token = await ManaxApiUserClient.LoginAsync(Username, Password);
            if (token == null)
            {
                Dispatcher.UIThread.Post(() => { LoginError = "Invalid username or password"; });
                return;
            }
            ManaxApiConfig.SetToken(token);

            ManaxApi.Models.User.User? self = await ManaxApiUserClient.GetSelf();
            
            if (self == null)
            {
                ManaxApiConfig.SetToken("");
                Dispatcher.UIThread.Post(() => { LoginError = "Failed to retrieve user information"; });
                return;
            }
            
            _isAdmin = self.Role is UserRole.Admin or UserRole.Owner ;

            PageChangedRequested?.Invoke(this,new HomePageViewModel());
        });
    }

    public bool IsAdmin()
    {
        return _isAdmin;
    }
}