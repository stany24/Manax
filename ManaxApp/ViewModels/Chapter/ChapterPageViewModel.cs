using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxApp.Models;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTOs;

namespace ManaxApp.ViewModels.Chapter;

public partial class ChapterPageViewModel : PageViewModel
{
    [ObservableProperty] private ClientChapter _chapter = new();

    public ChapterPageViewModel(long chapterId)
    {
        ControlBarVisible = true;
        Task.Run(async () =>
        {
            ChapterDTO? chapterAsync = await ManaxApiChapterClient.GetChapterAsync(chapterId);
            if (chapterAsync == null) return;
            Dispatcher.UIThread.Post(() =>
            {
                Chapter.Info = chapterAsync;
                Chapter.Pages = new ObservableCollection<Bitmap>(new Bitmap[chapterAsync.Pages]);
            });
            Console.WriteLine(chapterAsync.Pages);
            for (int i = 0; i < chapterAsync.Pages; i++)
            {
                int index = i;
                byte[]? pageBites = await ManaxApiChapterClient.GetChapterPageAsync(chapterId, i);
                if (pageBites == null) continue;
                try
                {
                    Bitmap page = new(new MemoryStream(pageBites));

                    Dispatcher.UIThread.Post(() => { Chapter.Pages[index] = page; });
                }
                catch (Exception)
                {
                    InfoEmitted?.Invoke(this, "Error loading page " + index);
                }
            }
        });
    }
}