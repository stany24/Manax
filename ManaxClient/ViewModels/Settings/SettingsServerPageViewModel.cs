using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Controls.Popups.SavePoint;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.SavePoint;
using ManaxLibrary.DTO.Setting;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Settings;

public partial class SettingsServerPageViewModel : PageViewModel
{
    [ObservableProperty] private List<ArchiveFormat> _allArchiveFormats;
    [ObservableProperty] private List<ImageFormat> _allImageFormats;
    [ObservableProperty] private string _problem = string.Empty;
    [ObservableProperty] private SettingsData _settings = null!;
    [ObservableProperty] private string _success = string.Empty;

    public SettingsServerPageViewModel()
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

    public void CreateLibrary()
    {
        SavePointCreatePopup popup = new();
        popup.CloseRequested += async void (_, _) =>
        {
            try
            {
                popup.Close();
                SavePointCreateDto? savePoint = popup.GetResult();
                if (savePoint == null) return;
                Optional<long> postLibraryResponse = await ManaxApiSavePointClient.PostSavePointAsync(savePoint);
                if (postLibraryResponse.Failed) InfoEmitted?.Invoke(this, postLibraryResponse.Error);
            }
            catch (Exception e)
            {
                Logger.LogError("Error creating library", e, Environment.StackTrace);
                InfoEmitted?.Invoke(this, "Error creating library");
            }
        };
        PopupRequested?.Invoke(this, popup);
    }
}