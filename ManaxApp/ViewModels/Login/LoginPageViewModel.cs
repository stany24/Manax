using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxApp.ViewModels.Login;

public partial class LoginPageViewModel: PageViewModel
{
    [ObservableProperty] private string _username = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private string _host = "http://127.0.0.1";
    [ObservableProperty] private int _port = 5246;
    [ObservableProperty] private string _loginError = string.Empty;

    public void Login()
    {
        Task.Run(async () =>
        {
            Dispatcher.UIThread.Post(() => { LoginError = string.Empty; });
            ManaxApiCaller.ManaxApiCaller.SetHost(new Uri(Host + $":{Port}/"));
            string? token = await ManaxApiCaller.ManaxApiCaller.LoginAsync(Username, Password);
            if (token == null)
            {
                Dispatcher.UIThread.Post(() => { LoginError = "Invalid username or password"; });
                return;
            }
            ManaxApiCaller.ManaxApiCaller.SetToken(token);
            PageChangedRequested?.Invoke(this,new LoginPageViewModel());
        });
    }
}