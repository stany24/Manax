using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models.Theme;

namespace ManaxClient.ViewModels.Pages.Settings;

public partial class SettingsAppViewModel : PageViewModel
{
    [ObservableProperty] private List<Theme> _availableThemes;

    private bool _isDarkMode;
    private Theme _selectedTheme;

    // Propriétés localisées
    [ObservableProperty] private string _pageTitle = string.Empty;
    [ObservableProperty] private string _pageSubtitle = string.Empty;
    [ObservableProperty] private string _displayModeTitle = string.Empty;
    [ObservableProperty] private string _darkModeLabel = string.Empty;
    [ObservableProperty] private string _colorThemeTitle = string.Empty;
    [ObservableProperty] private string _selectedThemeLabel = string.Empty;
    [ObservableProperty] private string _colorPreviewLabel = string.Empty;
    [ObservableProperty] private string _primaryColorLabel = string.Empty;
    [ObservableProperty] private string _secondaryColorLabel = string.Empty;
    [ObservableProperty] private string _tertiaryColorLabel = string.Empty;

    public SettingsAppViewModel()
    {
        _availableThemes = ThemePresets.GetPresets();
        _selectedTheme = _availableThemes[0];
        _isDarkMode = false;
        
        BindLocalizedStrings();
    }

    private void BindLocalizedStrings()
    {
        Localize(() => PageTitle, "SettingsAppPage.Title");
        Localize(() => PageSubtitle, "SettingsAppPage.Subtitle");
        Localize(() => DisplayModeTitle, "SettingsAppPage.DisplayMode");
        Localize(() => DarkModeLabel, "SettingsAppPage.DarkMode");
        Localize(() => ColorThemeTitle, "SettingsAppPage.ColorTheme");
        Localize(() => SelectedThemeLabel, "SettingsAppPage.SelectedTheme");
        Localize(() => ColorPreviewLabel, "SettingsAppPage.ColorPreview");
        Localize(() => PrimaryColorLabel, "SettingsAppPage.PrimaryColor");
        Localize(() => SecondaryColorLabel, "SettingsAppPage.SecondaryColor");
        Localize(() => TertiaryColorLabel, "SettingsAppPage.TertiaryColor");
    }

    public Theme SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if (SetProperty(ref _selectedTheme, value)) UpdateTheme();
        }
    }

    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (SetProperty(ref _isDarkMode, value)) UpdateTheme();
        }
    }

    private void UpdateTheme()
    {
        ThemeService.UpdateTheme(SelectedTheme, IsDarkMode);
    }
}