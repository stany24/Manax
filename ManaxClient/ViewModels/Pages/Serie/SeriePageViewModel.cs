using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
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
    [ObservableProperty] private bool _isFilePickerOpen;
    [ObservableProperty] private Models.Rank? _selectedRank;
    [ObservableProperty] private Models.Serie? _serie;
    
    private readonly ReadOnlyObservableCollection<Models.Rank> _ranks;
    public ReadOnlyObservableCollection<Models.Rank> Ranks => _ranks;

    public SeriePageViewModel(long serieId)
    {
        Models.Rank.LoadRanks();
        SortExpressionComparer<Models.Rank> comparer = SortExpressionComparer<Models.Rank>.Descending(t => t.Value);
        Models.Rank.NewRanks
            .Connect()
            .SortAndBind(out _ranks, comparer)
            .Subscribe();
        Serie = new Models.Serie(serieId);
        Serie.ErrorEmitted += (_, msg) => { InfoEmitted?.Invoke(this, msg); };
        Serie.LoadInfo();
        Serie.LoadChapters();
        Serie.LoadPoster();
        Task.Run(() => { LoadRanks(serieId); });
    }

    private async void LoadRanks(long serieId)
    {
        try
        {
            Optional<List<UserRankDto>> rankingResponse = await ManaxApiRankClient.GetRankingAsync();
            if (rankingResponse.Failed)
            {
                InfoEmitted?.Invoke(this, rankingResponse.Error);
                return;
            }

            UserRankDto? rank = rankingResponse.GetValue().FirstOrDefault(rank => rank.SerieId == serieId);
            if (rank == null)
            {
                BindToRankChange(serieId);
                return;
            }

            Models.Rank? userRank = Ranks.FirstOrDefault(r => r.Id == rank.RankId);
            Dispatcher.UIThread.Invoke(() => { SelectedRank = userRank; });
            BindToRankChange(serieId);
        }
        catch (Exception e)
        {
            InfoEmitted?.Invoke(this, "Error loading ranks");
            Logger.LogError("Failed to load ranks for serie with ID: " + serieId, e, Environment.StackTrace);
        }
    }

    private void BindToRankChange(long serieId)
    {
        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != nameof(SelectedRank)) return;
            if (SelectedRank == null) return;
            UserRankCreateDto userRankCreateDto = new()
            {
                SerieId = serieId,
                RankId = SelectedRank.Id
            };
            Task.Run(async () =>
            {
                Optional<bool> userRankResponse = await ManaxApiRankClient.SetUserRankAsync(userRankCreateDto);
                InfoEmitted?.Invoke(this, userRankResponse.Failed ? userRankResponse.Error : "Rank set correctly");
            });
        };
    }

    public void MoveToChapterPage(Models.Chapter chapter)
    {
        ChapterPageViewModel chapterPageViewModel = new(Serie.Chapters.ToList(), chapter);
        PageChangedRequested?.Invoke(this, chapterPageViewModel);
    }

    public void UpdateSerie()
    {
        if (Serie == null) return;
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
                InfoEmitted?.Invoke(this, "Error updating serie");
                Logger.LogError("Failed to update serie with ID: " + Serie.Id, e, Environment.StackTrace);
            }
        };
        PopupRequested?.Invoke(this, popup);
    }

    public async void ReplacePoster()
    {
        try
        {
            if (Serie == null || IsFilePickerOpen) return;
            IsFilePickerOpen = true;

            Window? window = Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop
                ? desktop.MainWindow
                : null;
            if (window?.StorageProvider == null) return;

            IReadOnlyList<IStorageFile> files = await window.StorageProvider.OpenFilePickerAsync(
                new FilePickerOpenOptions
                {
                    Title = "Sélectionnez une image pour le poster",
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
                InfoEmitted?.Invoke(this, "Poster replaced successfully");
                Logger.LogInfo("Poster replaced successfully for serie ID: " + Serie.Id);
            }
        }
        catch (Exception e)
        {
            InfoEmitted?.Invoke(this, "Error replacing poster");
            Logger.LogError("Error replacing poster for serie ID: " + Serie?.Id, e, Environment.StackTrace);
        }
    }
}