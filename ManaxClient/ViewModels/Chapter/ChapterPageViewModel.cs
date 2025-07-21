using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTOs;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Chapter;

public partial class ChapterPageViewModel : PageViewModel
{
    [ObservableProperty] private ClientChapter _chapter = new();

    public ChapterPageViewModel(long chapterId)
    {
        ControlBarVisible = false;
        Task.Run(async () =>
        {
            Optional<ChapterDTO> chapterResponse = await ManaxApiChapterClient.GetChapterAsync(chapterId);
            if (chapterResponse.Failed)
            {
                InfoEmitted?.Invoke(this, chapterResponse.Error);
                return;
            }
            ChapterDTO chapter = chapterResponse.GetValue();
            Dispatcher.UIThread.Post(() =>
            {
                Chapter.Info = chapter;
                Chapter.Pages = new ObservableCollection<Bitmap>(new Bitmap[chapter.Pages]);
            });
            for (int i = 0; i < chapter.Pages; i++)
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
                    Logger.LogError("Failed to load page " + index + " for chapter " + chapterId,e,Environment.StackTrace);
                }
            }
        });
    }
}