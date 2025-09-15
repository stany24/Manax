using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models;
using ManaxClient.Models.Collections;
using ManaxClient.ViewModels.Pages.Chapter;
using ManaxClient.ViewModels.Popup.ConfirmCancel;
using ManaxClient.ViewModels.Popup.ConfirmCancel.Content;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.Notifications;
using Logger = ManaxLibrary.Logging.Logger;

namespace ManaxClient.ViewModels.Pages.Serie;

public partial class SeriePageViewModel : PageViewModel
{
    [ObservableProperty] private SortedObservableCollection<ClientChapter> _chapters;
    [ObservableProperty] private bool _isFilePickerOpen;
    [ObservableProperty] private Bitmap? _poster;
    [ObservableProperty] private ObservableCollection<RankDto> _ranks = [];
    [ObservableProperty] private RankDto? _selectedRank;
    [ObservableProperty] private SerieDto? _serie;

    public SeriePageViewModel(long serieId)
    {
        Chapters = new SortedObservableCollection<ClientChapter>([])
        {
            SortingSelector = dto => dto.Info.Number
        };
        Serie = new SerieDto { Id = serieId };
        Task.Run(() => { LoadSerieInfo(serieId); });
        Task.Run(() => { LoadPoster(serieId); });
        Task.Run(() => { LoadChapters(serieId); });
        Task.Run(() => { LoadRanks(serieId); });
        ServerNotification.OnSerieUpdated += UpdateSerieInfo;
        ServerNotification.OnPosterModified += LoadPoster;
        ServerNotification.OnChapterAdded += OnChapterAdded;
        ServerNotification.OnChapterModified += OnChapterModified;
        ServerNotification.OnChapterDeleted += OnChapterDeleted;
        ServerNotification.OnReadCreated += OnReadCreated;
        ServerNotification.OnReadDeleted += OnReadDeleted;
    }

    ~SeriePageViewModel()
    {
        ServerNotification.OnSerieUpdated -= UpdateSerieInfo;
        ServerNotification.OnPosterModified -= LoadPoster;
        ServerNotification.OnChapterAdded -= OnChapterAdded;
        ServerNotification.OnChapterDeleted -= OnChapterDeleted;
        ServerNotification.OnReadCreated -= OnReadCreated;
        ServerNotification.OnReadDeleted -= OnReadDeleted;
    }

    private void OnReadDeleted(long obj)
    {
        if (Serie == null) return;
        ClientChapter? chapter = Chapters.FirstOrDefault(c => c.Info.Id == obj);
        if (chapter == null) return;
        Dispatcher.UIThread.Post(() => { chapter.Read = null; });
    }

    private void OnReadCreated(ReadDto read)
    {
        if (Serie == null) return;
        ClientChapter? chapter = Chapters.FirstOrDefault(c => c.Info.Id == read.ChapterId);
        if (chapter == null) return;
        Dispatcher.UIThread.Post(() => { chapter.Read = read; });
    }

    private void UpdateSerieInfo(SerieDto serie)
    {
        if (serie.Id != Serie?.Id) return;
        Dispatcher.UIThread.Post(() => { Serie = serie; });
    }

    private void OnChapterAdded(ChapterDto chapter)
    {
        if (chapter.SerieId != Serie?.Id) return;
        Dispatcher.UIThread.Post(() => Chapters.Add(new ClientChapter { Info = chapter }));
    }

    private void OnChapterModified(ChapterDto chapter)
    {
        if (chapter.SerieId != Serie?.Id) return;
        Dispatcher.UIThread.Post(() =>
        {
            ClientChapter? firstOrDefault = Chapters.FirstOrDefault(c => c.Info.Id == chapter.Id);
            if (firstOrDefault == null)
                Chapters.Add(new ClientChapter { Info = chapter });
            else
                firstOrDefault.Info = chapter;
        });
    }

    private void OnChapterDeleted(long chapterId)
    {
        ClientChapter? chapter = Chapters.FirstOrDefault(c => c.Info.Id == chapterId);
        if (chapter == null) return;
        Dispatcher.UIThread.Post(() => Chapters.Remove(chapter));
    }

    private async void LoadRanks(long serieId)
    {
        try
        {
            Optional<List<RankDto>> ranksResponse = await ManaxApiRankClient.GetRanksAsync();
            if (ranksResponse.Failed)
            {
                InfoEmitted?.Invoke(this, ranksResponse.Error);
                return;
            }

            List<RankDto> ranks = ranksResponse.GetValue();
            Dispatcher.UIThread.Post(() =>
            {
                foreach (RankDto rank in ranks) Ranks.Add(rank);
            });

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

            RankDto? userRank = ranks.FirstOrDefault(r => r.Id == rank.RankId);
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

    private async void LoadSerieInfo(long serieId)
    {
        try
        {
            Optional<SerieDto> serieInfoResponse = await ManaxApiSerieClient.GetSerieInfoAsync(serieId);
            if (serieInfoResponse.Failed) InfoEmitted?.Invoke(this, serieInfoResponse.Error);
            Dispatcher.UIThread.Post(() => Serie = serieInfoResponse.GetValue());
        }
        catch (Exception e)
        {
            InfoEmitted?.Invoke(this, "Error loading serie info");
            Logger.LogError("Failed to load serie with ID: " + serieId, e, Environment.StackTrace);
        }
    }

    private async void LoadPoster(long serieId)
    {
        try
        {
            if (serieId != Serie?.Id) return;
            Optional<byte[]> seriePosterResponse = await ManaxApiSerieClient.GetSeriePosterAsync(serieId);
            if (seriePosterResponse.Failed)
            {
                InfoEmitted?.Invoke(this, seriePosterResponse.Error);
                Poster = null;
                return;
            }

            try
            {
                Poster = new Bitmap(new MemoryStream(seriePosterResponse.GetValue()));
            }
            catch
            {
                Poster = null;
            }
        }
        catch (Exception e)
        {
            InfoEmitted?.Invoke(this, "Error loading poster");
            Logger.LogError("Failed to load poster for serie with ID: " + serieId, e, Environment.StackTrace);
        }
    }

    private async void LoadChapters(long serieId)
    {
        try
        {
            Optional<List<long>> response = await ManaxApiSerieClient.GetSerieChaptersAsync(serieId);
            if (response.Failed)
            {
                InfoEmitted?.Invoke(this, response.Error);
                return;
            }

            List<long> chaptersIds = response.GetValue();
            List<ChapterDto> chapters = [];
            foreach (long chapterId in chaptersIds)
            {
                Optional<ChapterDto> chapterResponse = await ManaxApiChapterClient.GetChapterAsync(chapterId);
                if (chapterResponse.Failed)
                {
                    InfoEmitted?.Invoke(this, chapterResponse.Error);
                    continue;
                }

                chapters.Add(chapterResponse.GetValue());
            }

            Dispatcher.UIThread.Invoke(() =>
            {
                foreach (ChapterDto chapter in chapters) Chapters.Add(new ClientChapter { Info = chapter });
            });
            LoadReads(serieId);
        }
        catch (Exception e)
        {
            InfoEmitted?.Invoke(this, "Error loading chapters");
            Logger.LogError("Failed to load chapters for serie with ID: " + serieId, e, Environment.StackTrace);
        }
    }

    private async void LoadReads(long serieId)
    {
        try
        {
            Optional<List<ReadDto>> response = await ManaxApiSerieClient.GetSerieChaptersReadAsync(serieId);
            if (response.Failed)
            {
                InfoEmitted?.Invoke(this, response.Error);
                return;
            }

            List<ReadDto> reads = response.GetValue();

            Dispatcher.UIThread.Post(() =>
            {
                foreach (ReadDto read in reads)
                {
                    ClientChapter? chapter = Chapters.FirstOrDefault(c => c.Info.Id == read.ChapterId);
                    if (chapter == null) continue;
                    chapter.Read = read;
                }
            });
        }
        catch (Exception e)
        {
            InfoEmitted?.Invoke(this, "Error loading chapters");
            Logger.LogError("Failed to load chapters for serie with ID: " + serieId, e, Environment.StackTrace);
        }
    }

    public void MoveToChapterPage(ClientChapter chapter)
    {
        ChapterPageViewModel chapterPageViewModel = new(Chapters.ToList(), chapter);
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