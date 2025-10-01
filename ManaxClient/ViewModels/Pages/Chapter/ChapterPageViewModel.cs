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

namespace ManaxClient.ViewModels.Pages.Chapter;

public partial class ChapterPageViewModel : PageViewModel
{
    private readonly List<Models.Chapter.Chapter> _chapters;
    [ObservableProperty] private Models.Chapter.Chapter _chapter = new();
    [ObservableProperty] private bool _controlBordersVisible;
    [ObservableProperty] private int _currentPage;
    private CancellationTokenSource? _loadPagesCts;
    [ObservableProperty] private Vector _scrollOffset = new(0, 0);

    public ChapterPageViewModel(List<Models.Chapter.Chapter> chapters, Models.Chapter.Chapter chapter)
    {
        _chapters = chapters;
        ControlBarVisible = false;
        HasMargin = false;
        Chapter = chapter;
        LoadPages(chapter.PageNumber, chapter.Id);
        PropertyChanged += HandleOffsetChanged;
    }

    ~ChapterPageViewModel()
    {
        _loadPagesCts?.Dispose();
    }

    public void ChangeBordersVisibility()
    {
        ControlBordersVisible = !ControlBordersVisible;
    }
    
    public void NextPage()
    {
        if (CurrentPage + 1 == Chapter.Pages.Count)
        {
            return;
        }

        ScrollOffset  = new Vector(0,ScrollOffset.Y + Chapter.Pages[CurrentPage].Size.Height);
    }

    public void PreviousPage()
    {
        if (CurrentPage == 0)
        {
            return;
        }

        ScrollOffset  = new Vector(0,ScrollOffset.Y - Chapter.Pages[CurrentPage].Size.Height);
    }

    public void PreviousChapter()
    {
        MarkAsRead();
        int index = _chapters.FindIndex(c => c.Id == Chapter.Id);
        if (index == 0) return;
        Chapter.Pages.Clear();
        Chapter = _chapters[index - 1];
        LoadPages(Chapter.PageNumber, Chapter.Id);
        ScrollOffset = new Vector(0, 0);
    }

    public void NextChapter()
    {
        MarkAsRead();
        int index = _chapters.FindIndex(c => c.Id == Chapter.Id);
        if (index + 1 == _chapters.Count) return;
        Chapter.Pages.Clear();
        Chapter = _chapters[index + 1];
        LoadPages(Chapter.PageNumber, Chapter.Id);
        ScrollOffset = new Vector(0, 0);
    }

    private void HandleOffsetChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ScrollOffset)) return;

        double height = 0;
        for (int i = 0; i < Chapter.Pages.Count; i++)
        {
            height += Chapter.Pages[i].Size.Height;
            if (!(height > ScrollOffset.Y)) continue;
            CurrentPage = i;
            Console.WriteLine(CurrentPage);
            return;
        }
    }

    private void LoadPages(int pageCount, long chapterId)
    {
        _loadPagesCts?.Cancel();
        _loadPagesCts = new CancellationTokenSource();
        CancellationToken token = _loadPagesCts.Token;
        Task.Run((Func<Task?>)(async () =>
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
        }), token);
    }

    public override void OnPageClosed()
    {
        if (CurrentPage == 0) return;
        MarkAsRead();
    }

    private void MarkAsRead()
    {
        ReadCreateDto readCreateDto = new()
        {
            ChapterId = Chapter.Id,
            Page = CurrentPage
        };
        Task.Run(async () =>
        {
            Optional<bool> response = await ManaxApiReadClient.MarkAsRead(readCreateDto);
            if (response.Failed) InfoEmitted?.Invoke(this, response.Error);
        });
    }
}