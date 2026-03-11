using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ManaxClient.Event;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Server.Data;

public partial class Chapter : ObservableObject, IDisposable
{
    [ObservableProperty] private DateTime _creation;
    [ObservableProperty] private long _id;
    [ObservableProperty] private DateTime _lastModification;

    private CancellationTokenSource? _loadPagesCts;
    [ObservableProperty] private uint _number;
    [ObservableProperty] private uint _pageNumber;
    [ObservableProperty] private ObservableCollection<Bitmap> _pages = [];
    [ObservableProperty] private ReadDto? _read;
    [ObservableProperty] private long _serieId;

    public Chapter(ChapterDto chapter)
    {
        FromChapterDto(chapter);
        NotificationReceiver.OnChapterUpdated += ChapterUpdated;
    }

    public Chapter() : this(new ChapterDto())
    {
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Chapter()
    {
        Dispose(false);
    }

    private void ChapterUpdated(ChapterDto chapter)
    {
        if (chapter.Id != Id) return;
        FromChapterDto(chapter);
    }

    private void FromChapterDto(ChapterDto dto)
    {
        Id = dto.Id;
        SerieId = dto.SerieId;
        Number = dto.Number;
        PageNumber = dto.PageNumber;
        Creation = dto.Creation;
        LastModification = dto.LastModification;
    }


    public void CancelLoadingPages()
    {
        _loadPagesCts?.Cancel();
    }

    public void LoadPages()
    {
        CancelLoadingPages();
        _loadPagesCts = new CancellationTokenSource();
        CancellationToken token = _loadPagesCts.Token;

        Task.Run((Func<Task?>)(async () =>
        {
            Pages = new ObservableCollection<Bitmap>(new Bitmap[PageNumber]);
            for (int i = 0; i < PageNumber; i++)
            {
                if (token.IsCancellationRequested)
                    break;
                Optional<byte[]> chapterPageResponse = await ManaxApiChapterClient.GetChapterPageAsync(Id, i);
                if (chapterPageResponse.Failed)
                {
                    WeakReferenceMessenger.Default.Send(new NotificationMessage(new Notification(chapterPageResponse.Error)));
                    Logger.LogFailure("Loading page " + i + " for chapter " + Id + " failed: " +
                                      chapterPageResponse.Error);
                    continue;
                }

                try
                {
                    Bitmap page = new(new MemoryStream(chapterPageResponse.GetValue()));
                    if (token.IsCancellationRequested)
                        break;
                    Pages[i] = page;
                }
                catch (Exception e)
                {
                    WeakReferenceMessenger.Default.Send(
                        new NotificationMessage(new Notification("Chapter.LoadPageFailed",[i])));
                    Logger.LogError("Loading page " + i + " for chapter " + Id + " failed", e);
                }
            }
        }), token);
    }

    public void MarkAsRead(uint page)
    {
        if (page == 0) return;
        ReadCreateDto readCreateDto = new()
        {
            ChapterId = Id,
            Page = page
        };
        Task.Run(async () =>
        {
            Optional<bool> response = await ManaxApiReadClient.MarkAsRead(readCreateDto);
            if (response.Failed) WeakReferenceMessenger.Default.Send(new NotificationMessage(new Notification(response.Error)));
        });
    }

    private void ReleaseUnmanagedResources()
    {
        NotificationReceiver.OnChapterUpdated -= ChapterUpdated;
    }

    private void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing) _loadPagesCts?.Dispose();
    }
}