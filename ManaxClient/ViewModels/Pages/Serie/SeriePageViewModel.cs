using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using DynamicData.Binding;
using Jeek.Avalonia.Localization;
using ManaxClient.Event;
using ManaxClient.ViewModels.Popup.ConfirmCancel;
using ManaxClient.ViewModels.Popup.ConfirmCancel.Content;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.DTO.Serie;
using Logger = ManaxLibrary.Logging.Logger;

namespace ManaxClient.ViewModels.Pages.Serie;

public partial class SeriePageViewModel : PageViewModel
{
    private readonly ReadOnlyObservableCollection<Models.Server.Data.Rank> _ranks;
    [ObservableProperty] private bool _isFilePickerOpen;
    [ObservableProperty] private Models.Server.Data.Rank? _selectedRank;
    [ObservableProperty] private Models.Server.Data.Serie _serie;

    public SeriePageViewModel(Models.Server.Data.Serie serie)
    {
        Serie = serie;
        SortExpressionComparer<Models.Server.Data.Rank> comparer =
            SortExpressionComparer<Models.Server.Data.Rank>.Descending(t => t.Value);
        MainWindowViewModel.Instance.RankSource.Ranks
            .Connect()
            .SortAndBind(out _ranks, comparer)
            .Subscribe();
        
        Serie.LoadInfo();
        Serie.LoadChapters();
        Serie.LoadPoster();
        Serie.LoadBanner();
    }

    public ReadOnlyObservableCollection<Models.Server.Data.Rank> Ranks => _ranks;

    partial void OnSelectedRankChanged(Models.Server.Data.Rank? value)
    {
        if (value == null) return;
        UserRankCreateDto userRankCreateDto = new()
        {
            SerieId = Serie.Id,
            RankId = value.Id
        };
        Task.Run(async () =>
        {
            Optional<bool> userRankResponse = await ManaxApiRankClient.SetUserRankAsync(userRankCreateDto);
            string message = userRankResponse.Failed
                ? userRankResponse.Error
                : Localizer.Get("SeriePage.RankSetCorrectly");
            WeakReferenceMessenger.Default.Send(new NotificationMessage(message));
        });
    }

    public void UpdateSerie()
    {
        SerieUpdateViewModel content = new(Serie);
        ConfirmCancelViewModel viewModel = new(content);
        Controls.Popups.Popup popup = new(viewModel);
        popup.Closed += async void (_, _) =>
        {
            try
            {
                if (viewModel.Canceled()) return;
                SerieUpdateDto serie = content.GetResult();
                Optional<bool> serieResponse = await ManaxApiSerieClient.PutSerieAsync(Serie.Id, serie);
                if (!serieResponse.Failed) return;
                WeakReferenceMessenger.Default.Send(new NotificationMessage(Localizer.Get("SeriePage.ErrorUpdatingSerie")));
                Logger.LogFailure("Failed to update serie with ID: " + Serie.Id);
            }
            catch (Exception e)
            {
                WeakReferenceMessenger.Default.Send(new NotificationMessage(Localizer.Get("SeriePage.ErrorUpdatingSerie")));
                Logger.LogError("Failed to update serie with ID: " + Serie.Id, e);
            }
        };
        WeakReferenceMessenger.Default.Send(new PopupChangeMessage(popup));
    }

    public async void ReplacePoster()
    {
        try
        {
            if (IsFilePickerOpen) return;
            IsFilePickerOpen = true;

            Window? window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;
            if (window?.StorageProvider == null) return;

            IReadOnlyList<IStorageFile> files = await window.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = Localizer.Get("SeriePage.SelectPosterImage"),
                    AllowMultiple = false,
                    FileTypeFilter =
                    [
                        new FilePickerFileType("Images")
                        {
                            Patterns = ["*.jpg", "*.webp", "*.jpeg", "*.png", "*.bmp", "*.gif"]
                        }
                    ]
                });
            IsFilePickerOpen = false;

            if (files.Count == 0) return;
            string filePath = files[0].Path.LocalPath;
            if (string.IsNullOrEmpty(filePath)) return;

            Optional<bool> replacePosterResponse = await ManaxApiUploadClient.ReplacePosterAsync(
                filePath,
                files[0].Name,
                Serie.Id);

            if (replacePosterResponse.Failed)
            {
                WeakReferenceMessenger.Default.Send(new NotificationMessage(replacePosterResponse.Error));
            }
            else
            {
                WeakReferenceMessenger.Default.Send(new NotificationMessage(Localizer.Get("SeriePage.PosterReplacedSuccess")));
                Logger.LogInfo("Poster replaced successfully for serie ID: " + Serie.Id);
            }
        }
        catch (Exception e)
        {
            WeakReferenceMessenger.Default.Send(new NotificationMessage(Localizer.Get("SeriePage.ErrorReplacingPoster")));
            Logger.LogError("Error replacing poster for serie ID: " + Serie.Id, e);
        }
    }
}