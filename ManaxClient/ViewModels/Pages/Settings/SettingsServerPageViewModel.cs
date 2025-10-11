using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Jeek.Avalonia.Localization;
using ManaxClient.ViewModels.Popup.ConfirmCancel;
using ManaxClient.ViewModels.Popup.ConfirmCancel.Content;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Library;
using ManaxLibrary.DTO.SavePoint;
using ManaxLibrary.DTO.Setting;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Pages.Settings;

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
                Logger.LogFailure("Failed to update settings");
                return;
            }

            Success = Localizer.Get("SettingsServerPage.SettingsUpdatedSuccess");
            Logger.LogInfo("Settings updated successfully");
        }
        catch (Exception e)
        {
            Logger.LogError("Error when updating settings", e);
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
                Logger.LogFailure("Failed to load settings");
                return;
            }

            Settings = settingsAsync.GetValue();
        }
        catch (Exception e)
        {
            Logger.LogError("Error when fetching settings", e);
        }
    }

    public void CreateSavePoint()
    {
        SavePointCreateViewModel content = new();
        ConfirmCancelViewModel viewModel = new(content);
        Controls.Popups.Popup popup = new(viewModel);
        popup.Closed += async void (_, _) =>
        {
            try
            {
                if (viewModel.Canceled()) return;
                SavePointCreateDto savePoint = content.GetResult();
                Optional<long> postLibraryResponse = await ManaxApiSavePointClient.PostSavePointAsync(savePoint);
                if (postLibraryResponse.Failed) InfoEmitted?.Invoke(this, postLibraryResponse.Error);
            }
            catch (Exception e)
            {
                Logger.LogError("Error creating save point", e);
                InfoEmitted?.Invoke(this, Localizer.Get("SettingsServerPage.ErrorCreatingSavePoint"));
            }
        };
        PopupRequested?.Invoke(this, popup);
    }

    public void CreateLibrary()
    {
        LibraryCreateViewModel content = new();
        ConfirmCancelViewModel viewModel = new(content);
        Controls.Popups.Popup popup = new(viewModel);
        popup.Closed += async void (_, _) =>
        {
            try
            {
                if (viewModel.Canceled()) return;
                LibraryCreateDto library = content.GetResult();
                Optional<long> postLibraryResponse = await ManaxApiLibraryClient.PostLibraryAsync(library);
                if (postLibraryResponse.Failed) InfoEmitted?.Invoke(this, postLibraryResponse.Error);
            }
            catch (Exception e)
            {
                Logger.LogError("Error creating library", e);
                InfoEmitted?.Invoke(this, Localizer.Get("SettingsServerPage.ErrorCreatingLibrary"));
            }
        };
        PopupRequested?.Invoke(this, popup);
    }
}