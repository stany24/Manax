using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Setting;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Settings;

public partial class SettingsPageViewModel : PageViewModel
{
    [ObservableProperty] private List<ArchiveFormat> _allArchiveFormats;
    [ObservableProperty] private List<ImageFormat> _allImageFormats;
    [ObservableProperty] private string _problem = string.Empty;
    [ObservableProperty] private SettingsData _settings = null!;
    [ObservableProperty] private string _success = string.Empty;

    public SettingsPageViewModel()
    {
        AllImageFormats = new List<ImageFormat>(Enum.GetValues(typeof(ImageFormat)).Cast<ImageFormat>());
        AllArchiveFormats = new List<ArchiveFormat>(Enum.GetValues(typeof(ArchiveFormat)).Cast<ArchiveFormat>());
        Task.Run(LoadSettings);
    }

    public async void Update()
    {
        try
        {
            Optional<bool> updateTask = await ManaxApiSettingsClient.UpdateSettingsAsync(Settings);
            if (updateTask.Failed)
            {
                Problem = updateTask.Error;
                Logger.LogFailure("Failed to update settings", Environment.StackTrace);
                return;
            }

            Success = "Settings updated successfully.";
            Logger.LogInfo("Settings updated successfully");
        }
        catch (Exception e)
        {
            Logger.LogError("Error when updating settings", e, Environment.StackTrace);
        }
    }

    private async void LoadSettings()
    {
        try
        {
            Optional<SettingsData> settingsAsync = await ManaxApiSettingsClient.GetSettingsAsync();
            if (settingsAsync.Failed)
            {
                Problem = settingsAsync.Error;
                Logger.LogFailure("Failed to load settings", Environment.StackTrace);
                return;
            }

            Settings = settingsAsync.GetValue();
        }
        catch (Exception e)
        {
            Logger.LogError("Error when fetching settings", e, Environment.StackTrace);
        }
    }
}