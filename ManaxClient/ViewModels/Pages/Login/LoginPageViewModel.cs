using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Jeek.Avalonia.Localization;
using ManaxClient.Models;
using ManaxClient.ViewModels.Pages.Home;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.User;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Pages.Login;

public sealed partial class LoginPageViewModel : PageViewModel
{
    private readonly string _saveFile;
    [ObservableProperty] private bool _canLogin = true;
    [ObservableProperty] private string _emoji = "🔑";
    [ObservableProperty] private string _host = string.Empty;
    private bool _isAdmin;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private int? _port;
    [ObservableProperty] private string _username = string.Empty;
    
    [ObservableProperty] private List<LanguageItem> _availableLanguages = [];
    [ObservableProperty] private LanguageItem? _selectedLanguage;

    public LoginPageViewModel()
    {
        ManaxApiConfig.ResetToken();
        _saveFile = Path.Combine(Directory.GetCurrentDirectory(), "login.json");
        ControlBarVisible = false;
        
        InitializeLanguages();
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

    public void Login()
    {
        Block();
        Uri hostUri;
        try
        {
            hostUri = Port != null ? new Uri(Host + $":{Port}/") : new Uri(Host);
        }
        catch
        {
            Release(Localizer.Get("LoginPage.Invalid.Host.Port"));
            return;
        }
        Task.Run(async () =>
        {
            ManaxApiConfig.SetHost(hostUri);
            Optional<UserLoginResultDto> loginResponse = await ManaxApiUserClient.LoginAsync(Username, Password);
            if (loginResponse.Failed)
            {
                Release(loginResponse.Error);
                return;
            }

            CheckToken(loginResponse.GetValue());
        });
    }

    private void Block()
    {
        CanLogin = false;
        Emoji = "⌛";
    }

    private void Release(string errorMessage)
    {
        InfoEmitted?.Invoke(this,errorMessage);
        CanLogin = true;
        Emoji = "🔑";
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
            InfoEmitted?.Invoke(this,"Unknown error while checking token");
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