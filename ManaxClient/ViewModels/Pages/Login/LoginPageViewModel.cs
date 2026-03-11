using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using DynamicData.Binding;
using ManaxClient.Event;
using ManaxClient.Localization;
using ManaxClient.Localization.Localizer;
using ManaxClient.Manager;
using ManaxClient.Models;
using ManaxClient.ViewModels.Pages.Home;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.User;
using ManaxLibrary.Logging;
using Material.Icons;

namespace ManaxClient.ViewModels.Pages.Login;

public sealed partial class LoginPageViewModel : PageViewModel
{
    private readonly ReadOnlyObservableCollection<Language> _languages;

    [ObservableProperty] private bool _canLogin = true;
    [ObservableProperty] private MaterialIconKind _emoji = MaterialIconKind.KeyOutline;
    [ObservableProperty] private string _host = string.Empty;
    [ObservableProperty] private string _password = string.Empty;
    [ObservableProperty] private Language _selectedLanguage;
    [ObservableProperty] private string _username = string.Empty;

    public LoginPageViewModel()
    {
        ManaxApiClient.ResetToken();
        ControlBarVisible = false;

        LanguageSource.Languages
            .Connect()
            .SortAndBind(out _languages, SortExpressionComparer<Language>.Ascending(lang => lang.Code))
            .Subscribe();
        string currentLanguage = Localizer.Language;
        if (string.IsNullOrEmpty(currentLanguage)) currentLanguage = "en";
        SelectedLanguage = Languages.FirstOrDefault(l => l.Code == currentLanguage) ?? Languages.First();

        TryLoadSavedLogin();
    }

    public ReadOnlyObservableCollection<Language> Languages => _languages;

    partial void OnSelectedLanguageChanged(Language value)
    {
        if (value.Code != Localizer.Language) Localizer.Language = value.Code;
    }

    public void Login()
    {
        Block();
        Uri hostUri;
        try
        {
            hostUri = new Uri(Host);
        }
        catch
        {
            Release("LoginPage.Invalid.Host.Port");
            return;
        }

        Task.Run(async () =>
        {
            ManaxApiClient.SetHost(hostUri);
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
        Emoji = MaterialIconKind.TimerOutline;
    }

    private void Release(string localizationKey)
    {
        WeakReferenceMessenger.Default.Send(new NotificationMessage(new Notification(localizationKey)));
        CanLogin = true;
        Emoji = MaterialIconKind.KeyOutline;
    }

    private void CheckToken(UserLoginResultDto result)
    {
        try
        {
            ManaxApiClient.SetToken(result.Token);
            WeakReferenceMessenger.Default.Send(new LoggedInMessage(result.Token));
            UserDto self = result.User;
            const string key = "LoginPage.Connected";
            object[] args = [self.Username, self.Role];
            WeakReferenceMessenger.Default.Send(new NotificationMessage(new Notification(key,args)));
            Logger.LogInfo("Logged in as " + self.Username + " with role " + self.Role);
            SaveLoginValues();
            WeakReferenceMessenger.Default.Send(new PageChangeMessage(new HomePageViewModel()));
        }
        catch (Exception e)
        {
            Logger.LogError("Unknow error while checking token",e);
            WeakReferenceMessenger.Default.Send(new NotificationMessage(new Notification("LoginPage.Token.Failed")));
        }
    }

    private void SaveLoginValues()
    {
        LoginValues loginValues = new()
        {
            Host = Host,
            Username = Username
        };
        StorageManager.Save(StorageManager.LoginFile, loginValues);
    }

    private void TryLoadSavedLogin()
    {
        LoginValues? loginValues = StorageManager.Load<LoginValues>(StorageManager.LoginFile);
        if (loginValues == null) return;
        Host = loginValues.Host;
        Username = loginValues.Username;
    }
}