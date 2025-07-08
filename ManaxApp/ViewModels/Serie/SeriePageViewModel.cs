using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxApp.Controls;
using ManaxApp.ViewModels.Chapter;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTOs;
using ManaxLibrary.DTOs.Rank;
using ManaxLibrary.DTOs.Serie;

namespace ManaxApp.ViewModels.Serie;

public partial class SeriePageViewModel : PageViewModel
{
    [ObservableProperty] private ObservableCollection<ChapterDTO> _chapters = [];
    [ObservableProperty] private Bitmap? _poster;
    [ObservableProperty] private ObservableCollection<RankDTO> _ranks = [];
    [ObservableProperty] private RankDTO? _selectedRank;
    [ObservableProperty] private SerieDTO? _serie;

    public SeriePageViewModel(long serieId)
    {
        ControlBarVisible = true;
        Task.Run(() => { LoadSerieInfo(serieId); });
        Task.Run(() => { LoadPoster(serieId); });
        Task.Run(() => { LoadChapters(serieId); });

        Task task = Task.Run(async () =>
        {
            List<RankDTO>? ranks = await ManaxApiRankClient.GetRanksAsync();
            if (ranks == null) return;
            Dispatcher.UIThread.Post(() =>
            {
                foreach (RankDTO rank in ranks) Ranks.Add(rank);
            });

            List<UserRankDTO>? userRanks = await ManaxApiRankClient.GetRankingAsync();
            UserRankDTO? rank = userRanks?.FirstOrDefault(rank => rank.SerieId == serieId);
            if (rank == null) return;
            RankDTO? userRank = ranks.FirstOrDefault(r => r.Id == rank.RankId);
            Dispatcher.UIThread.Post(() => { SelectedRank = userRank; });
        });
        task.ContinueWith(_ =>
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
                    bool success = await ManaxApiRankClient.SetUserRankAsync(userRankCreateDto);
                    if (success) return;
                    InfoEmitted?.Invoke(this, "Failed to set rank");
                });
            };
        });
    }

    private async void LoadSerieInfo(long serieId)
    {
        try
        {
            SerieDTO? libraryAsync = await ManaxApiSerieClient.GetSerieInfoAsync(serieId);
            if (libraryAsync == null) return;
            Dispatcher.UIThread.Post(() => Serie = libraryAsync);
        }
        catch (Exception)
        {
            InfoEmitted?.Invoke(this, "Error loading serie info");
        }
    }

    private async void LoadPoster(long serieId)
    {
        try
        {
            byte[]? posterBytes = await ManaxApiSerieClient.GetSeriePosterAsync(serieId);
            if (posterBytes != null)
                try
                {
                    Poster = new Bitmap(new MemoryStream(posterBytes));
                }
                catch
                {
                    Poster = null;
                }
            else
                Poster = null;
        }
        catch (Exception)
        {
            InfoEmitted?.Invoke(this, "Error loading poster");
        }
    }

    private async void LoadChapters(long serieId)
    {
        try
        {
            List<long>? chaptersIds = await ManaxApiSerieClient.GetSerieChaptersAsync(serieId);

            if (chaptersIds == null) return;
            List<ChapterDTO> chapters = [];
            foreach (long chapterId in chaptersIds)
            {
                ChapterDTO? chapter = await ManaxApiChapterClient.GetChapterAsync(chapterId);
                if (chapter == null) continue;
                chapters.Add(chapter);
            }
            Dispatcher.UIThread.Post(() =>
            {
                foreach (ChapterDTO chapter in chapters)
                {
                    Chapters.Add(chapter);
                }
            });
        }
        catch (Exception)
        {
            InfoEmitted?.Invoke(this, "Error loading chapters");
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
                bool success = await ManaxApiSerieClient.PutSerieAsync(Serie.Id, serie);
                InfoEmitted?.Invoke(this, success ? "Serie update successful" : "Serie update failed");
                if (success)
                {
                    _ = Task.Run(() =>
                    {
                        LoadSerieInfo(Serie.Id);
                    });
                }
            }
            catch (Exception)
            {
                InfoEmitted?.Invoke(this, "Error updating serie");
            }
        };
        PopupRequested?.Invoke(this, popup);
    }
}