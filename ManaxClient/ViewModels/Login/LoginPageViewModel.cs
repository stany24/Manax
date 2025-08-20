using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.ViewModels.Home;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.User;
using Logger = ManaxLibrary.Logging.Logger;

namespace ManaxClient.ViewModels.Login;

public partial class LoginPageViewModel : PageViewModel
{
    [ObservableProperty] private string _host = "http://127.0.0.1";
    private bool _isAdmin;
    private bool _isOwner;
    [ObservableProperty] private string _loginError = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private int _port = 5246;
    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private bool _canLogin = true;
    [ObservableProperty] private string _emoji = "🔑";

    public LoginPageViewModel()
    {
        ControlBarVisible = false;
    }

    public void Login()
    {
        LoginError = string.Empty;
        Task.Run(async () =>
        {
            CanLogin = false;
            Emoji = "⌛";
            ManaxApiConfig.SetHost(new Uri(Host + $":{Port}/"));
            Optional<UserLoginResultDto> loginResponse = await ManaxApiUserClient.LoginAsync(Username, Password);
            if (loginResponse.Failed)
            {
                InfoEmitted?.Invoke(this, loginResponse.Error);
                CanLogin = true;
                Emoji = "🔑";
                return;
            }

            CheckToken(loginResponse.GetValue());
        });
    }

    public void Claim()
    {
        LoginError = string.Empty;
        Task.Run(async () =>
        {
            ManaxApiConfig.SetHost(new Uri(Host + $":{Port}/"));
            Optional<UserLoginResultDto> claimResponse = await ManaxApiUserClient.ClaimAsync(Username, Password);
            if (claimResponse.Failed)
            {
                InfoEmitted?.Invoke(this, claimResponse.Error);
                return;
            }

            CheckToken(claimResponse.GetValue());
        });
    }

    private void CheckToken(UserLoginResultDto result)
    {
        try
        {
            ManaxApiConfig.SetToken(result.Token);

            UserDto self = result.User;
            _isAdmin = self.Role is UserRole.Admin or UserRole.Owner;
            _isOwner = self.Role is UserRole.Owner;
            InfoEmitted?.Invoke(this, $"Logged in as {self.Username} ({self.Role})");
            Logger.LogInfo($"Logged in as {self.Username} ({self.Role})");
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

    public bool IsOwner()
    {
        return _isOwner;
    }
}