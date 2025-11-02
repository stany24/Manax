using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models.Theme;

namespace ManaxClient.ViewModels.Pages.Settings;

public partial class SettingsAppViewModel : PageViewModel
{
    [ObservableProperty] private List<ThemeSettingsData> _availableThemes;

    private bool _isDarkMode;
    private ThemeSettingsData _selectedThemeSettingsData;

    public SettingsAppViewModel()
    {
        _availableThemes = ThemeSettings.GetPresets();
        _selectedThemeSettingsData = AvailableThemes
            .FirstOrDefault(t => t.Name == ThemeSettings.Current.Name) ?? AvailableThemes[0];
        IsDarkMode = ThemeSettings.Current.IsDark;
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
        get => _isDarkMode;
        set
        {
            if (SetProperty(ref _isDarkMode, value)) UpdateTheme();
        }
    }

    private void UpdateTheme()
    {
        SelectedThemeSettingsData.IsDark = IsDarkMode;
        ThemeSettings.UpdateTheme(SelectedThemeSettingsData);
    }
}