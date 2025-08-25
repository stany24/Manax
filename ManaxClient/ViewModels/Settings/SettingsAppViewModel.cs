using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models;

namespace ManaxClient.ViewModels.Settings;

public partial class SettingsAppViewModel : PageViewModel
{
    private readonly ThemeService _themeService;
    
    [ObservableProperty] private List<Theme> _availableThemes;
    private Theme _selectedTheme;
    public Theme SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            if(SetProperty(ref _selectedTheme, value))
            {
                UpdateTheme();
            }
        }
    }

    private bool _isDarkMode;
    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (SetProperty(ref _isDarkMode, value))
            {
                UpdateTheme();
            }
        }
    }

    public SettingsAppViewModel()
    {
        _themeService = new ThemeService();
        _availableThemes = ThemePresets.GetPresets();
        _selectedTheme = _availableThemes[0];
        _isDarkMode = false;
    }
    
    private void UpdateTheme()
    {
        _themeService.UpdateTheme(SelectedTheme, IsDarkMode);
    }
}
