using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Jeek.Avalonia.Localization;
using ManaxClient.Models;
using ManaxClient.ViewModels.Pages.Home;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.User;
using Logger = ManaxLibrary.Logging.Logger;

namespace ManaxClient.ViewModels.Pages.Login;

public sealed partial class LoginPageViewModel : PageViewModel
{
    private readonly string _saveFile;
    [ObservableProperty] private bool _canLogin = true;
    [ObservableProperty] private string _emoji = "🔑";
    [ObservableProperty] private string _host = string.Empty;
    private bool _isAdmin;
    [ObservableProperty] private string _loginError = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private int? _port;
    [ObservableProperty] private string _username = string.Empty;

    [ObservableProperty] private string _titleText = string.Empty;
    [ObservableProperty] private string _subtitleText = string.Empty;
    [ObservableProperty] private string _usernameLabel = string.Empty;
    [ObservableProperty] private string _usernamePlaceholder = string.Empty;
    [ObservableProperty] private string _passwordLabel = string.Empty;
    [ObservableProperty] private string _passwordPlaceholder = string.Empty;
    [ObservableProperty] private string _serverLabel = string.Empty;
    [ObservableProperty] private string _serverPlaceholder = string.Empty;
    [ObservableProperty] private string _signInText = string.Empty;
    [ObservableProperty] private string _tipTitle = string.Empty;
    [ObservableProperty] private string _tipMessage = string.Empty;
    
    [ObservableProperty] private List<LanguageItem> _availableLanguages = [];
    [ObservableProperty] private LanguageItem? _selectedLanguage;

    public LoginPageViewModel()
    {
        ManaxApiConfig.ResetToken();
        _saveFile = Path.Combine(Directory.GetCurrentDirectory(), "login.json");
        ControlBarVisible = false;
        
        InitializeLanguages();
        BindLocalizedStrings();
        TryLoadSavedLogin();
    }

    private void InitializeLanguages()
    {
        AvailableLanguages = 
        [
            new LanguageItem { Code = "en", DisplayName = "English" },
            new LanguageItem { Code = "fr", DisplayName = "Français" }
        ];
        
        string currentLanguage = Localizer.Language;
        if (string.IsNullOrEmpty(currentLanguage)) {currentLanguage = "en";}
        SelectedLanguage = AvailableLanguages.FirstOrDefault(l => l.Code == currentLanguage);
    }

    partial void OnSelectedLanguageChanged(LanguageItem? value)
    {
        if (value != null && value.Code != Localizer.Language)
        {
            Localizer.Language = value.Code;
        }
    }

    private void BindLocalizedStrings()
    {
        Localize(() => TitleText, "LoginPage.Title");
        Localize(() => SubtitleText, "LoginPage.Subtitle");
        Localize(() => UsernameLabel, "LoginPage.Username");
        Localize(() => UsernamePlaceholder, "LoginPage.Username.Placeholder");
        Localize(() => PasswordLabel, "LoginPage.Password");
        Localize(() => PasswordPlaceholder, "LoginPage.Password.Placeholder");
        Localize(() => ServerLabel, "LoginPage.Server");
        Localize(() => ServerPlaceholder, "LoginPage.Server.Placeholder");
        Localize(() => SignInText, "LoginPage.SignIn");
        Localize(() => TipTitle, "LoginPage.Tip.Title");
        Localize(() => TipMessage, "LoginPage.Tip.Message");
    }

    public void Login()
    {
        LoginError = string.Empty;
        Task.Run(async () =>
        {
            CanLogin = false;
            Emoji = "⌛";
            Uri hostUri = Port != null ? new Uri(Host + $":{Port}/") : new Uri(Host);
            ManaxApiConfig.SetHost(hostUri);
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

    private void CheckToken(UserLoginResultDto result)
    {
        try
        {
            ManaxApiConfig.SetToken(result.Token);

            UserDto self = result.User;
            _isAdmin = self.Role is UserRole.Admin or UserRole.Owner;
            InfoEmitted?.Invoke(this, $"Logged in as {self.Username} ({self.Role})");
            Logger.LogInfo($"Logged in as {self.Username} ({self.Role})");
            SaveLoginValues();
            PageChangedRequested?.Invoke(this, new HomePageViewModel());
        }
        catch (Exception)
        {
            Dispatcher.UIThread.Post(() => { LoginError = "Unknown error"; });
        }
    }

    private void SaveLoginValues()
    {
        LoginValues loginValues = new()
        {
            Host = Host,
            Port = Port,
            Username = Username
        };
        try
        {
            if (File.Exists(_saveFile)) File.Delete(_saveFile);
            using FileStream fileStream = File.OpenWrite(_saveFile);
            using StreamWriter writer = new(fileStream);
            string serialize = JsonSerializer.Serialize(loginValues);
            writer.Write(serialize);
            writer.Flush();
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to save login values", e);
            throw;
        }
    }

    private void TryLoadSavedLogin()
    {
        if (!File.Exists(_saveFile)) return;
        try
        {
            using FileStream fileStream = File.OpenRead(_saveFile);
            using StreamReader reader = new(fileStream);
            string content = reader.ReadToEnd();
            LoginValues? loginValues = JsonSerializer.Deserialize<LoginValues>(content);
            if (loginValues == null) return;
            Host = loginValues.Host;
            Port = loginValues.Port;
            Username = loginValues.Username;
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to load login values", e);
        }
    }

    public bool IsAdmin()
    {
        return _isAdmin;
    }
}

public class LanguageItem
{
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
