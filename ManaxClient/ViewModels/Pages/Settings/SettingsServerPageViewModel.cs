using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
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

    [ObservableProperty] private string _pageTitle = string.Empty;
    [ObservableProperty] private string _pageSubtitle = string.Empty;
    [ObservableProperty] private string _addSavePointText = string.Empty;
    [ObservableProperty] private string _addLibraryText = string.Empty;
    [ObservableProperty] private string _posterConfigTitle = string.Empty;
    [ObservableProperty] private string _posterNameLabel = string.Empty;
    [ObservableProperty] private string _imageFormatLabel = string.Empty;
    [ObservableProperty] private string _qualityLabel = string.Empty;
    [ObservableProperty] private string _maxWidthLabel = string.Empty;
    [ObservableProperty] private string _minWidthLabel = string.Empty;
    [ObservableProperty] private string _chapterConfigTitle = string.Empty;
    [ObservableProperty] private string _maxPageWidthLabel = string.Empty;
    [ObservableProperty] private string _minPageWidthLabel = string.Empty;
    [ObservableProperty] private string _archiveFormatLabel = string.Empty;
    [ObservableProperty] private string _seriesConfigTitle = string.Empty;
    [ObservableProperty] private string _maxDescriptionLengthLabel = string.Empty;
    [ObservableProperty] private string _minDescriptionLengthLabel = string.Empty;
    [ObservableProperty] private string _tipTitle = string.Empty;
    [ObservableProperty] private string _tipMessage = string.Empty;
    [ObservableProperty] private string _saveButtonText = string.Empty;
    [ObservableProperty] private string _settingsUpdatedSuccessText = string.Empty;
    [ObservableProperty] private string _errorCreatingSavePointText = string.Empty;
    [ObservableProperty] private string _errorCreatingLibraryText = string.Empty;

    public SettingsServerPageViewModel()
    {
        AllImageFormats = new List<ImageFormat>(Enum.GetValues(typeof(ImageFormat)).Cast<ImageFormat>());
        AllArchiveFormats = new List<ArchiveFormat>(Enum.GetValues(typeof(ArchiveFormat)).Cast<ArchiveFormat>());
        
        BindLocalizedStrings();
        
        Task.Run(LoadSettings);
    }

    private void BindLocalizedStrings()
    {
        Localize(() => PageTitle, "SettingsServerPage.Title");
        Localize(() => PageSubtitle, "SettingsServerPage.Subtitle");
        Localize(() => AddSavePointText, "SettingsServerPage.AddSavePoint");
        Localize(() => AddLibraryText, "SettingsServerPage.AddLibrary");
        Localize(() => PosterConfigTitle, "SettingsServerPage.PosterConfig");
        Localize(() => PosterNameLabel, "SettingsServerPage.PosterName");
        Localize(() => ImageFormatLabel, "SettingsServerPage.ImageFormat");
        Localize(() => QualityLabel, "SettingsServerPage.Quality");
        Localize(() => MaxWidthLabel, "SettingsServerPage.MaxWidth");
        Localize(() => MinWidthLabel, "SettingsServerPage.MinWidth");
        Localize(() => ChapterConfigTitle, "SettingsServerPage.ChapterConfig");
        Localize(() => MaxPageWidthLabel, "SettingsServerPage.MaxPageWidth");
        Localize(() => MinPageWidthLabel, "SettingsServerPage.MinPageWidth");
        Localize(() => ArchiveFormatLabel, "SettingsServerPage.ArchiveFormat");
        Localize(() => SeriesConfigTitle, "SettingsServerPage.SeriesConfig");
        Localize(() => MaxDescriptionLengthLabel, "SettingsServerPage.MaxDescriptionLength");
        Localize(() => MinDescriptionLengthLabel, "SettingsServerPage.MinDescriptionLength");
        Localize(() => TipTitle, "SettingsServerPage.TipTitle");
        Localize(() => TipMessage, "SettingsServerPage.TipMessage");
        Localize(() => SaveButtonText, "SettingsServerPage.SaveButton");
        Localize(() => SettingsUpdatedSuccessText, "SettingsServerPage.SettingsUpdatedSuccess");
        Localize(() => ErrorCreatingSavePointText, "SettingsServerPage.ErrorCreatingSavePoint");
        Localize(() => ErrorCreatingLibraryText, "SettingsServerPage.ErrorCreatingLibrary");
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

            Success = SettingsUpdatedSuccessText;
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
                InfoEmitted?.Invoke(this, ErrorCreatingSavePointText);
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
                InfoEmitted?.Invoke(this, ErrorCreatingLibraryText);
            }
        };
        PopupRequested?.Invoke(this, popup);
    }
}