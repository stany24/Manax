using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using ManaxClient.Event;
using ManaxClient.Models.Server.Data;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Server.Sources;

public class ChapterSource
{
    private readonly Lock _chaptersLock = new();
    public readonly SourceCache<Chapter, long> Chapters = new(x => x.Id);

    public ChapterSource()
    {
        NotificationReceiver.OnChapterAdded += OnChapterCreated;
        NotificationReceiver.OnChapterDeleted += OnChapterDeleted;
    }

    private void OnChapterDeleted(long id)
    {
        lock (_chaptersLock)
        {
            Chapters.RemoveKey(id);
        }
    }

    private void OnChapterCreated(ChapterDto dto)
    {
        lock (_chaptersLock)
        {
            Chapters.AddOrUpdate(new Chapter(dto));
        }
    }

    public void LoadSerieChapters(long id, bool loadReads = true)
    {
        Task.Run(async () =>
        {
            try
            {
                Optional<List<long>> response = await ManaxApiSerieClient.GetSerieChaptersAsync(id);
                if (response.Failed)
                {
                    Logger.LogFailure(response.Error);
                    WeakReferenceMessenger.Default.Send(new NotificationMessage(response.Error));
                    return;
                }

                foreach (long chapterId in response.GetValue()) _ = LoadChapter(chapterId);
                if (loadReads) _ = LoadSerieReads(id);
            }
            catch (Exception e)
            {
                const string error = "Failed to load chapters from server";
                Logger.LogError(error, e);
                WeakReferenceMessenger.Default.Send(new NotificationMessage(error));
            }
        });
    }

    private async Task LoadChapter(long id)
    {
        lock (_chaptersLock)
        {
            if (Chapters.Keys.Contains(id)) return;
        }

        try
        {
            Optional<ChapterDto> response = await ManaxApiChapterClient.GetChapterAsync(id);
            if (response.Failed)
            {
                Logger.LogFailure(response.Error);
                WeakReferenceMessenger.Default.Send(new NotificationMessage(response.Error));
                return;
            }

            lock (_chaptersLock)
            {
                Dispatcher.UIThread.Post(() => { Chapters.AddOrUpdate(new Chapter(response.GetValue())); });
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task LoadSerieReads(long serieId)
    {
        try
        {
            Optional<List<ReadDto>> response = await ManaxApiSerieClient.GetSerieChaptersReadAsync(serieId);
            if (response.Failed)
            {
                WeakReferenceMessenger.Default.Send(new NotificationMessage(response.Error));
                return;
            }

            List<ReadDto> reads = response.GetValue();

            Dispatcher.UIThread.Post(() =>
            {
                foreach (ReadDto read in reads)
                    lock (_chaptersLock)
                    {
                        Chapter? chapter = Chapters.Items.FirstOrDefault(c => c.Id == read.ChapterId);
                        if (chapter == null) continue;
                        chapter.Read = read;
                    }
            });
        }
        catch (Exception e)
        {
            string message = "Failed to load chapters for serie with ID: " + serieId;
            WeakReferenceMessenger.Default.Send(new NotificationMessage(message));
            Logger.LogError(message, e);
        }
    }
}