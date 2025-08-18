using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Chapter;

public partial class ChapterPageViewModel : PageViewModel
{
    [ObservableProperty] private bool _controlBordersVisible;
    [ObservableProperty] private ClientChapter _chapter = new();
    [ObservableProperty] private Vector _scrollOffset = new(0, 0);
    private readonly List<ClientChapter> _chapters;
    private int _currentPage;
    private CancellationTokenSource? _loadPagesCts;

    public ChapterPageViewModel(List<ClientChapter> chapters,ClientChapter chapter)
    {
        _chapters = chapters;
        ControlBarVisible = false;
        Chapter = chapter;
        LoadPages(chapter.Info.Pages, chapter.Info.Id);
        PropertyChanged += HandleOffsetChanged;
    }

    public void ChangeBordersVisibility()
    {
        ControlBordersVisible = !ControlBordersVisible;
    }

    public void PreviousChapter()
    {
        int index = _chapters.FindIndex(c => c.Info.Id == Chapter.Info.Id);
        if(index == 0){return;}
        Chapter.Pages.Clear();
        Chapter =  _chapters[index - 1];
        LoadPages(Chapter.Info.Pages, Chapter.Info.Id);
        ScrollOffset = new Vector(0, 0);
    }

    public void NextChapter()
    {
        int index = _chapters.FindIndex(c => c.Info.Id == Chapter.Info.Id);
        if(index+1 == _chapters.Count){return;}
        Chapter.Pages.Clear();
        Chapter =  _chapters[index + 1];
        LoadPages(Chapter.Info.Pages, Chapter.Info.Id);
        ScrollOffset = new Vector(0, 0);
    }
    
    private void HandleOffsetChanged(object? sender, PropertyChangedEventArgs e)
    {
        if(e.PropertyName != nameof(ScrollOffset)){return;}

        double height = 0;
        for (int i = 0; i < Chapter.Pages.Count; i++)
        {
            height += Chapter.Pages[i].Size.Height;
            if (!(height > ScrollOffset.Y)) continue;
            _currentPage = i;
            Console.WriteLine(_currentPage);
            return;
        }
    }

    private void LoadPages(int pageCount, long chapterId)
    {
        _loadPagesCts?.Cancel();
        _loadPagesCts = new CancellationTokenSource();
        CancellationToken token = _loadPagesCts.Token;
        Task.Run(async () =>
        {
            Chapter.Pages = new ObservableCollection<Bitmap>(new Bitmap[pageCount]);
            for (int i = 0; i < pageCount; i++)
            {
                if (token.IsCancellationRequested) return;
                int index = i;
                Optional<byte[]> chapterPageResponse = await ManaxApiChapterClient.GetChapterPageAsync(chapterId, i);
                if (chapterPageResponse.Failed)
                {
                    InfoEmitted?.Invoke(this, chapterPageResponse.Error);
                    continue;
                }

                try
                {
                    Bitmap page = new(new MemoryStream(chapterPageResponse.GetValue()));
                    Dispatcher.UIThread.Post(() =>
                    {
                        if (token.IsCancellationRequested) return;
                        Chapter.Pages[index] = page;
                    });
                }
                catch (Exception e)
                {
                    InfoEmitted?.Invoke(this, "Error loading page " + index);
                    Logger.LogError("Failed to load page " + index + " for chapter " + chapterId, e,
                        Environment.StackTrace);
                }
            }
        }, token);
    }

    public override void OnPageClosed()
    {
        if(_currentPage == 0){return;}
        ReadCreateDto readCreateDto = new()
        {
            ChapterId = Chapter.Info.Id,
            Page = _currentPage
        };
        Task.Run(async () =>
        {
            Optional<bool> response = await ManaxApiReadClient.MarkAsRead(readCreateDto);
            if (response.Failed) { InfoEmitted?.Invoke(this, response.Error); }
        });
    }
}