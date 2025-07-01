using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxApp.ViewModels.Chapter;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTOs;
using ManaxLibrary.DTOs.Rank;
using ManaxLibrary.DTOs.Serie;

namespace ManaxApp.ViewModels.Serie;

public partial class SeriePageViewModel : PageViewModel
{
    [ObservableProperty] private Bitmap? _poster;
    [ObservableProperty] private SerieDTO? _serie;
    [ObservableProperty] private ObservableCollection<ChapterDTO> _chapters = [];
    [ObservableProperty] private ObservableCollection<RankDTO> _ranks = [];
    [ObservableProperty] private RankDTO? _selectedRank;

    public SeriePageViewModel(long serieId)
    {
        ControlBarVisible = true;
        Task.Run(async () =>
        {
            SerieDTO? libraryAsync = await ManaxApiSerieClient.GetSerieInfoAsync(serieId);
            if (libraryAsync == null) return;
            Dispatcher.UIThread.Post(() => Serie= libraryAsync);
        });
        
        Task.Run(async () =>
        {
            byte[]? posterBytes = await ManaxApiSerieClient.GetSeriePosterAsync(serieId);
            if (posterBytes != null)
            {
                try { Poster = new Bitmap(new MemoryStream(posterBytes)); }
                catch { Poster = null; }
            }
            else { Poster = null; }
        });
        
        Task.Run(async () =>
        {
            List<long>? chaptersIds = await ManaxApiSerieClient.GetSerieChaptersAsync(serieId);
            
            if (chaptersIds == null) return;
            foreach (long chapterId in chaptersIds)
            {
                ChapterDTO? chapter = await ManaxApiChapterClient.GetChapterAsync(chapterId);
                if (chapter == null) continue;
                Dispatcher.UIThread.Post(() => Chapters.Add(chapter));
            }
        });

        Task task = Task.Run(async () =>
        {
            List<RankDTO>? ranks = await ManaxApiRankClient.GetRanksAsync();
            if (ranks == null) return;
            Dispatcher.UIThread.Post(() =>
            {
                foreach (RankDTO rank in ranks)
                {
                    Ranks.Add(rank);
                }
            });

            List<UserRankDTO>? userRanks = await ManaxApiRankClient.GetRankingAsync();
            UserRankDTO? rank = userRanks?.FirstOrDefault(rank => rank.SerieId == serieId);
            if (rank == null) return;
            RankDTO? userRank = ranks.FirstOrDefault(r => r.Id == rank.RankId);
            Dispatcher.UIThread.Post(() =>
            {
                SelectedRank = userRank;
            });
        });
        task.ContinueWith(_ =>
        {
            PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(SelectedRank))
                {
                    if (SelectedRank == null) return;
                    UserRankCreateDTO userRankCreateDto = new()
                    {
                        SerieId = serieId,
                        RankId = SelectedRank.Id
                    };
                    Task.Run(async () =>
                    {
                        bool success = await ManaxApiRankClient.SetUserRankAsync(userRankCreateDto);
                        if (success) return;
                        InfoEmitted?.Invoke(this, "Failed to set rank");
                    });
                }
            };
        });
    }
    
    public void MoveToChapterPage(ChapterDTO chapter)
    {
        ChapterPageViewModel chapterPageViewModel = new(chapter.Id);
        PageChangedRequested?.Invoke(this, chapterPageViewModel);
    }
}