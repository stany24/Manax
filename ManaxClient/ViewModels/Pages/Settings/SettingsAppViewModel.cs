using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models.Theme;

namespace ManaxClient.ViewModels.Pages.Settings;

public partial class SettingsAppViewModel : PageViewModel
{
    [ObservableProperty] private List<ManaxTheme> _availableThemes;

    private bool _isDarkMode;
    private ManaxTheme _selectedTheme;

    public SettingsAppViewModel()
    {
        _availableThemes = ThemePresets.GetPresets();
        _selectedTheme = _availableThemes[0];
        _isDarkMode = false;
    }

    public ManaxTheme SelectedTheme
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