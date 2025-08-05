using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Controls.Popups.Serie;
using ManaxClient.Models.Collections;
using ManaxClient.ViewModels.Chapter;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTOs.Chapter;
using ManaxLibrary.DTOs.Rank;
using ManaxLibrary.DTOs.Serie;
using ManaxLibrary.Notifications;
using Logger = ManaxLibrary.Logging.Logger;

namespace ManaxClient.ViewModels.Serie;

public partial class SeriePageViewModel : PageViewModel
{
    [ObservableProperty] private SortedObservableCollection<ChapterDTO> _chapters;
    [ObservableProperty] private Bitmap? _poster;
    [ObservableProperty] private ObservableCollection<RankDTO> _ranks = [];
    [ObservableProperty] private RankDTO? _selectedRank;
    [ObservableProperty] private SerieDTO? _serie;

    public SeriePageViewModel(long serieId)
    {
        Chapters = new SortedObservableCollection<ChapterDTO>([])
        {
            SortingSelector = dto => dto.Number
        };
        Serie = new SerieDTO { Id = serieId };
        Task.Run(() => { LoadSerieInfo(serieId); });
        Task.Run(() => { LoadPoster(serieId); });
        Task.Run(() => { LoadChapters(serieId); });
        Task.Run(() => { LoadRanks(serieId); }).ContinueWith(_ => BindToRankChange(serieId));
        ServerNotification.OnSerieUpdated += UpdateSerieInfo;
        ServerNotification.OnPosterModified += LoadPoster;
        ServerNotification.OnChapterAdded += OnChapterAdded;
        ServerNotification.OnChapterDeleted += OnChapterDeleted;
    }

    ~SeriePageViewModel()
    {
        ServerNotification.OnSerieUpdated -= UpdateSerieInfo;
        ServerNotification.OnPosterModified -= LoadPoster;
        ServerNotification.OnChapterAdded -= OnChapterAdded;
        ServerNotification.OnChapterDeleted -= OnChapterDeleted;
    }

    private void UpdateSerieInfo(SerieDTO serie)
    {
        if(serie.Id != Serie?.Id) return;
        Dispatcher.UIThread.Post(() =>
        {
            Serie = serie;
        });
    }
    
    private void OnChapterAdded(ChapterDTO chapter)
    {
        if (chapter.SerieId != Serie?.Id) return;
        Dispatcher.UIThread.Post(() => Chapters.Add(chapter));
    }
    
    private void OnChapterDeleted(long chapterId)
    {
        ChapterDTO? chapter = Chapters.FirstOrDefault(c => c.Id == chapterId);
        if (chapter == null) return;
        Dispatcher.UIThread.Post(() => Chapters.Remove(chapter));
    }

    private async void LoadRanks(long serieId)
    {
        Optional<List<RankDTO>> ranksResponse = await ManaxApiRankClient.GetRanksAsync();
        if (ranksResponse.Failed)
        {
            InfoEmitted?.Invoke(this,ranksResponse.Error);
            return;
        }
        List<RankDTO> ranks = ranksResponse.GetValue();
        Dispatcher.UIThread.Post(() =>
        {
            foreach (RankDTO rank in ranks) Ranks.Add(rank);
        });

        Optional<List<UserRankDTO>> rankingResponse = await ManaxApiRankClient.GetRankingAsync();
        if (rankingResponse.Failed)
        {
            InfoEmitted?.Invoke(this, rankingResponse.Error);
            return;
        }
        UserRankDTO? rank = rankingResponse.GetValue().FirstOrDefault(rank => rank.SerieId == serieId);
        if (rank == null) return;
        RankDTO? userRank = ranks.FirstOrDefault(r => r.Id == rank.RankId);
        Dispatcher.UIThread.Invoke(() => { SelectedRank = userRank; });
    }

    private void BindToRankChange(long serieId)
    {
        PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != nameof(SelectedRank)) return;
            if (SelectedRank == null) return;
            UserRankCreateDTO userRankCreateDto = new()
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
            Optional<SerieDTO> serieInfoResponse = await ManaxApiSerieClient.GetSerieInfoAsync(serieId);
            if (serieInfoResponse.Failed)
            {
                InfoEmitted?.Invoke(this,serieInfoResponse.Error);
            }
            Dispatcher.UIThread.Post(() => Serie = serieInfoResponse.GetValue());
        }
        catch (Exception e)
        {
            InfoEmitted?.Invoke(this, "Error loading serie info");
            Logger.LogError("Failed to load serie with ID: " + serieId,e, Environment.StackTrace);
        }
    }

    private async void LoadPoster(long serieId)
    {
        try
        {
            if(serieId != Serie?.Id) return;
            Optional<byte[]> seriePosterResponse = await ManaxApiSerieClient.GetSeriePosterAsync(serieId);
            if (seriePosterResponse.Failed)
            {
                InfoEmitted?.Invoke(this, seriePosterResponse.Error);
                Poster = null;
                return;
            }
            try { Poster = new Bitmap(new MemoryStream(seriePosterResponse.GetValue())); }
            catch { Poster = null; }
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
            List<ChapterDTO> chapters = [];
            foreach (long chapterId in chaptersIds)
            {
                Optional<ChapterDTO> chapterResponse = await ManaxApiChapterClient.GetChapterAsync(chapterId);
                if (chapterResponse.Failed)
                {
                    InfoEmitted?.Invoke(this, chapterResponse.Error);
                    continue;
                }
                chapters.Add(chapterResponse.GetValue());
            }
            Dispatcher.UIThread.Post(() =>
            {
                foreach (ChapterDTO chapter in chapters)
                {
                    Chapters.Add(chapter);
                }
            });
        }
        catch (Exception e)
        {
            InfoEmitted?.Invoke(this, "Error loading chapters");
            Logger.LogError("Failed to load chapters for serie with ID: " + serieId, e, Environment.StackTrace);
        }
    }

    public void MoveToChapterPage(ChapterDTO chapter)
    {
        ChapterPageViewModel chapterPageViewModel = new(chapter.Id);
        PageChangedRequested?.Invoke(this, chapterPageViewModel);
    }
    
    public void UpdateSerie()
    {
        if (Serie == null) return;
        SerieUpdatePopup popup = new(Serie);
        popup.CloseRequested += async void (_, _) =>
        {
            try
            {
                popup.Close();
                SerieUpdateDTO? serie = popup.GetResult();
                if (serie == null) return;
                Optional<bool> serieResponse = await ManaxApiSerieClient.PutSerieAsync(Serie.Id, serie);
                if (serieResponse.Failed){
                    InfoEmitted?.Invoke(this, serieResponse.Error);
                }
            }
            catch (Exception e)
            {
                InfoEmitted?.Invoke(this, "Error updating serie");
                Logger.LogError("Failed to update serie with ID: " + Serie.Id,e, Environment.StackTrace);
            }
        };
        PopupRequested?.Invoke(this, popup);
    }
}