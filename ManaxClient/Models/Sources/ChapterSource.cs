using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using DynamicData;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Sources;

public static class ChapterSource
{
    public static readonly SourceCache<Chapter, long> Chapters = new (x => x.Id);
    private static readonly object ChaptersLock = new ();
    
    public static EventHandler<string>? ErrorEmitted { get; set; }
    
    static ChapterSource()
    {
        ServerNotification.OnChapterAdded += OnChapterCreated;
        ServerNotification.OnChapterDeleted += OnChapterDeleted;
    }
    
    private static void OnChapterDeleted(long id)
    {
        lock (ChaptersLock)
        {
            Chapters.RemoveKey(id);
        }
    }

    private static void OnChapterCreated(ChapterDto dto)
    {
        lock (ChaptersLock)
        {
            Chapters.AddOrUpdate(new Chapter(dto));
        }
    }

    public static void LoadSerieChapters(long id, bool loadReads = true)
    {
        Task.Run(() =>
        {
            try
            {
                Optional<List<long>> response = ManaxApiSerieClient.GetSerieChaptersAsync(id).Result;
                if (response.Failed)
                {
                    Logger.LogFailure(response.Error);
                    ErrorEmitted?.Invoke(null,response.Error);
                    return;
                }
                
                foreach (long chapterId in response.GetValue())
                {
                    LoadChapter(chapterId);
                }
                if(loadReads){ LoadSerieReads(id);}
            }
            catch (Exception e)
            {
                const string error = "Failed to load chapters from server";
                Logger.LogError(error,e);
                ErrorEmitted?.Invoke(null,error);
            }
        });
    }

    private static void LoadChapter(long id)
    {
        lock (ChaptersLock)
        {
            if (Chapters.Keys.Contains(id)) { return; }
        }

        try
        {
            Optional<ChapterDto> response = ManaxApiChapterClient.GetChapterAsync(id).Result;
            if (response.Failed)
            {
                Logger.LogFailure(response.Error);
                ErrorEmitted?.Invoke(null,response.Error);
                return;
            }

            lock (ChaptersLock)
            {
                Chapters.AddOrUpdate(new Chapter(response.GetValue()));
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    private static void LoadSerieReads(long serieId)
    {
        try
        {
            Optional<List<ReadDto>> response = ManaxApiSerieClient.GetSerieChaptersReadAsync(serieId).Result;
            if (response.Failed)
            {
                ErrorEmitted?.Invoke(null, response.Error);
                return;
            }

            List<ReadDto> reads = response.GetValue();

            Dispatcher.UIThread.Post(() =>
            {
                foreach (ReadDto read in reads)
                {
                    lock (ChaptersLock)
                    {
                        Chapter? chapter = Chapters.Items.FirstOrDefault(c => c.Id == read.ChapterId);
                        if (chapter == null) continue;
                        chapter.Read = read;
                    }
                }
            });
        }
        catch (Exception e)
        {
            string message = "Failed to load chapters for serie with ID: " + serieId;
            ErrorEmitted?.Invoke(null, message);
            Logger.LogError(message, e);
        }
    }
}