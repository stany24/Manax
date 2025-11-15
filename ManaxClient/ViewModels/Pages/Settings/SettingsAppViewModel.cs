using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using Jeek.Avalonia.Localization;
using ManaxClient.Localization;
using ManaxClient.Models.Theme;

namespace ManaxClient.ViewModels.Pages.Settings;

public partial class SettingsAppViewModel : PageViewModel
{
    [ObservableProperty] private List<ThemeSettingsData> _availableThemes;
    [ObservableProperty] private Language? _selectedLanguage;
    private ThemeSettingsData _selectedThemeSettingsData;
    private readonly ReadOnlyObservableCollection<Language> _languages;
    
    public ReadOnlyObservableCollection<Language> Languages => _languages;

    public SettingsAppViewModel()
    {
        _availableThemes = ThemeSettings.GetPresets();
        _selectedThemeSettingsData = AvailableThemes
            .FirstOrDefault(t => t.Name == ThemeSettings.Current.Name) ?? AvailableThemes[0];
        IsDarkMode = ThemeSettings.Current.IsDark;
        
        LanguageSource.Languages
            .Connect()
            .SortAndBind(out _languages, SortExpressionComparer<Language>.Ascending(lang => lang.Code))
            .Subscribe();
        string currentLanguage = Localizer.Language;
        if (string.IsNullOrEmpty(currentLanguage)) currentLanguage = "en";
        SelectedLanguage = Languages.FirstOrDefault(l => l.Code == currentLanguage) ?? Languages.First();
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

    partial void OnSelectedLanguageChanged(Language? value)
    {
        if (value != null && value.Code != Localizer.Language) Localizer.Language = value.Code;
    }

    private void UpdateTheme()
    {
        SelectedThemeSettingsData.IsDark = IsDarkMode;
        ThemeSettings.UpdateTheme(SelectedThemeSettingsData);
    }
}