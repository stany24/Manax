using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using Jeek.Avalonia.Localization;
using ManaxClient.Localization;
using ManaxClient.Models.Theme;

namespace ManaxClient.ViewModels.Pages.Settings;

public partial class SettingsAppViewModel : PageViewModel
{
    private readonly ReadOnlyObservableCollection<Language> _languages;
    [ObservableProperty] private Language? _selectedLanguage;
    private bool _isDarkMode;
    private HsvColor _themeColor;

    public SettingsAppViewModel()
    {
        _isDarkMode = ThemeSettings.Current.IsDark;
        _themeColor = ThemeSettings.Current.AccentColor.ToHsv();

        LanguageSource.Languages
            .Connect()
            .SortAndBind(out _languages, SortExpressionComparer<Language>.Ascending(lang => lang.Code))
            .Subscribe();
        string currentLanguage = Localizer.Language;
        if (string.IsNullOrEmpty(currentLanguage)) currentLanguage = "en";
        SelectedLanguage = Languages.FirstOrDefault(l => l.Code == currentLanguage) ?? Languages.First();
    }

    public ReadOnlyObservableCollection<Language> Languages => _languages;
    

    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (SetProperty(ref _isDarkMode, value)) UpdateTheme();
        }
    }
    
    public HsvColor ThemeColor
    {
        get => _themeColor;
        set
        {
            if (SetProperty(ref _themeColor, value)) UpdateTheme();
        }
    }

    partial void OnSelectedLanguageChanged(Language? value)
    {
        if (value != null && value.Code != Localizer.Language) Localizer.Language = value.Code;
    }

    private void UpdateTheme()
    {
        ThemeSettings.UpdateTheme(new ThemeSettingsData(ThemeColor.ToHsl(), IsDarkMode));
    }
}