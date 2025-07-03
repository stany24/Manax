using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxApp.ViewModels.Home;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTOs.User;

namespace ManaxApp.ViewModels.Login;

public partial class LoginPageViewModel : PageViewModel
{
    private bool _isAdmin;
    [ObservableProperty] private string _host = "http://127.0.0.1";
    [ObservableProperty] private string _loginError = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private int _port = 5246;
    [ObservableProperty] private string _username = string.Empty;

    public void Login()
    {
        LoginError = string.Empty;
        Task.Run(async () =>
        {
            ManaxApiConfig.SetHost(new Uri(Host + $":{Port}/"));
            string? token = await ManaxApiUserClient.LoginAsync(Username, Password);
            CheckToken(token, "Invalid username or password");
        });
    }

    public void Claim()
    {
        LoginError = string.Empty;
        Task.Run(async () =>
        {
            ManaxApiConfig.SetHost(new Uri(Host + $":{Port}/"));
            string? token = await ManaxApiUserClient.ClaimAsync(Username, Password);
            CheckToken(token, "Could not claim server");
        });
    }

    private async void CheckToken(string? token, string errorMessage)
    {
        try
        {
            if (token == null)
            {
                Dispatcher.UIThread.Post(() => { LoginError = errorMessage; });
                return;
            }

            ManaxApiConfig.SetToken(token);

            UserDTO? self = await ManaxApiUserClient.GetSelf();

            if (self == null)
            {
                ManaxApiConfig.SetToken("");
                Dispatcher.UIThread.Post(() => { LoginError = "Failed to retrieve user information"; });
                return;
            }

            _isAdmin = self.Role is UserRole.Admin or UserRole.Owner;
            
            InfoEmitted?.Invoke(this, $"Logged in as {self.Username} ({self.Role})");
            PageChangedRequested?.Invoke(this, new HomePageViewModel());
        }
        catch (Exception)
        {
            Dispatcher.UIThread.Post(() => { LoginError = "Unknown error"; });
        }
    }

    public bool IsAdmin()
    {
        return _isAdmin;
    }
}