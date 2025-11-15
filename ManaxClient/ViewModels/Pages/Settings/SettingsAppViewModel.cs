using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Jeek.Avalonia.Localization;
using ManaxClient.Models;
using ManaxClient.Models.Theme;

namespace ManaxClient.ViewModels.Pages.Settings;

public partial class SettingsAppViewModel : PageViewModel
{
    [ObservableProperty] private List<LanguageItem> _availableLanguages = [];
    [ObservableProperty] private List<ThemeSettingsData> _availableThemes;

    [ObservableProperty] private LanguageItem? _selectedLanguage;
    private ThemeSettingsData _selectedThemeSettingsData;

    public SettingsAppViewModel()
    {
        _availableThemes = ThemeSettings.GetPresets();
        _selectedThemeSettingsData = AvailableThemes
            .FirstOrDefault(t => t.Name == ThemeSettings.Current.Name) ?? AvailableThemes[0];
        IsDarkMode = ThemeSettings.Current.IsDark;
        InitializeLanguages();
    }

    public ThemeSettingsData SelectedThemeSettingsData
    {
        get => _selectedThemeSettingsData;
        set
        {
            if (SetProperty(ref _selectedThemeSettingsData, value)) UpdateTheme();
        }
    }

    public bool IsDarkMode
    {
        get;
        set
        {
            if (SetProperty(ref field, value)) UpdateTheme();
        }
    }

    private void InitializeLanguages()
    {
        AvailableLanguages =
        [
            new LanguageItem { Code = "en", DisplayName = "English" },
            new LanguageItem { Code = "fr", DisplayName = "Français" }
        ];

        string currentLanguage = Localizer.Language;
        if (string.IsNullOrEmpty(currentLanguage)) currentLanguage = "en";
        SelectedLanguage = AvailableLanguages.FirstOrDefault(l => l.Code == currentLanguage);
    }

    partial void OnSelectedLanguageChanged(LanguageItem? value)
    {
        if (value != null && value.Code != Localizer.Language) Localizer.Language = value.Code;
    }

    private void UpdateTheme()
    {
        SelectedThemeSettingsData.IsDark = IsDarkMode;
        ThemeSettings.UpdateTheme(SelectedThemeSettingsData);
    }
}