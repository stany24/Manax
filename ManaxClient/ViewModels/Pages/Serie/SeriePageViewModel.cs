using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using Jeek.Avalonia.Localization;
using ManaxClient.Models.Sources;
using ManaxClient.ViewModels.Pages.Chapter;
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
    private readonly ReadOnlyObservableCollection<Models.Rank> _ranks;
    [ObservableProperty] private bool _isFilePickerOpen;
    [ObservableProperty] private Models.Rank? _selectedRank;
    [ObservableProperty] private Models.Serie _serie;

    [ObservableProperty] private string _chapterCountText = string.Empty;

    public SeriePageViewModel(Models.Serie serie)
    {
        SortExpressionComparer<Models.Rank> comparer = SortExpressionComparer<Models.Rank>.Descending(t => t.Value);
        RankSource.Ranks
            .Connect()
            .SortAndBind(out _ranks, comparer)
            .Subscribe();
        Serie = serie;
        Serie.LoadInfo();
        Serie.LoadChapters();
        Serie.LoadPoster();
        
        BindLocalizedStrings();
    }

    partial void OnSelectedRankChanged(Models.Rank? value)
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
            InfoEmitted?.Invoke(this, userRankResponse.Failed ? userRankResponse.Error : Localizer.Get("SeriePage.RankSetCorrectly"));
        });
    }

    private void BindLocalizedStrings()
    {
        Localize(() => ChapterCountText, "SeriePage.ChapterCount", () => Serie.Chapters.Count);
    }

    public ReadOnlyObservableCollection<Models.Rank> Ranks => _ranks;

    public void MoveToChapterPage(Models.Chapter chapter)
    {
        ChapterPageViewModel chapterPageViewModel = new(Serie.Chapters.ToList(), chapter);
        PageChangedRequested?.Invoke(this, chapterPageViewModel);
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
                if (serieResponse.Failed) InfoEmitted?.Invoke(this, serieResponse.Error);
            }
            catch (Exception e)
            {
                InfoEmitted?.Invoke(this, Localizer.Get("SeriePage.ErrorUpdatingSerie"));
                Logger.LogError("Failed to update serie with ID: " + Serie.Id, e);
            }
        };
        PopupRequested?.Invoke(this, popup);
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
                InfoEmitted?.Invoke(this, replacePosterResponse.Error);
            }
            else
            {
                InfoEmitted?.Invoke(this, Localizer.Get("SeriePage.PosterReplacedSuccess"));
                Logger.LogInfo("Poster replaced successfully for serie ID: " + Serie.Id);
            }
        }
        catch (Exception e)
        {
            InfoEmitted?.Invoke(this, Localizer.Get("SeriePage.ErrorReplacingPoster"));
            Logger.LogError("Error replacing poster for serie ID: " + Serie.Id, e);
        }
    }
}