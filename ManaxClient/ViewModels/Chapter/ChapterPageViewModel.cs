using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Chapter;

public partial class ChapterPageViewModel : PageViewModel
{
    [ObservableProperty] private bool _controlBordersVisible;
    [ObservableProperty] private ClientChapter _chapter = new();
    [ObservableProperty] private Vector _scrollOffset = new(0, 0);
    private int _currentPage;

    public ChapterPageViewModel(ClientChapter chapter)
    {
        ControlBarVisible = false;
        Chapter = chapter;
        Task.Run(() => LoadPages(chapter.Info.Pages, chapter.Info.Id));
        PropertyChanged += HandleOffsetChanged;
    }

    public void ChangeBordersVisibility()
    {
        ControlBordersVisible = !ControlBordersVisible;
    }

    public void PreviousChapter()
    {
        
    }

    public void NextChapter()
    {
        
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

    public ChapterPageViewModel(long chapterId)
    {
        ControlBarVisible = false;
        Task.Run(async () =>
        {
            Optional<ChapterDto> chapterResponse = await ManaxApiChapterClient.GetChapterAsync(chapterId);
            if (chapterResponse.Failed)
            {
                InfoEmitted?.Invoke(this, chapterResponse.Error);
                return;
            }

            ChapterDto chapter = chapterResponse.GetValue();
            Dispatcher.UIThread.Post(() =>
            {
                Chapter.Info = chapter;
                Chapter.Pages = new ObservableCollection<Bitmap>(new Bitmap[chapter.Pages]);
            });
            LoadPages(chapter.Pages, chapter.Id);
        });
    }

    private async void LoadPages(int pageCount,long chapterId)
    {
        Chapter.Pages = new ObservableCollection<Bitmap>(new Bitmap[pageCount]);
        for (int i = 0; i < pageCount; i++)
        {
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

                Dispatcher.UIThread.Post(() => { Chapter.Pages[index] = page; });
            }
            catch (Exception e)
            {
                InfoEmitted?.Invoke(this, "Error loading page " + index);
                Logger.LogError("Failed to load page " + index + " for chapter " + chapterId, e, Environment.StackTrace);
            }
        }
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