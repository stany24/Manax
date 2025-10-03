using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using ManaxClient.Models.Collections;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models;

public partial class Serie:ObservableObject
{
    public static readonly SourceCache<Serie, long> Series = new (x => x.Id);
    public static readonly object SeriesLock = new();
    public static readonly object LoadLock = new();
    
   [ObservableProperty] private long _id;
   [ObservableProperty] private long? _libraryId;
   [ObservableProperty] private string _title = string.Empty;
   [ObservableProperty] private string _description = string.Empty;
   [ObservableProperty] private Status _status;

   [ObservableProperty] private DateTime _creation;
   [ObservableProperty] private DateTime _lastModification;
   [ObservableProperty] private List<Tag> _tags = [];
   
   [ObservableProperty] private Bitmap? _poster;
   [ObservableProperty] private SortedObservableCollection<Chapter> _chapters= new([]) { SortingSelector = dto => dto.Number };
   
   private bool _posterLoaded;
   private bool _chaptersLoaded;
   private bool _infoLoaded;
   
   public EventHandler<string>? ErrorEmitted;

   public Serie(long id) : this(new SerieDto { Id = id })
   {
   }

   static Serie()
   {
       ServerNotification.OnSerieCreated += OnSerieCreated;
       ServerNotification.OnSerieDeleted += OnSerieDeleted;
   }
   public Serie(SerieDto dto)
   {
       FromSerieDto(dto);
       ServerNotification.OnSerieUpdated += OnSerieUpdated;
       ServerNotification.OnPosterModified += OnPosterModified;
       ServerNotification.OnChapterAdded += OnChapterAdded;
       ServerNotification.OnChapterDeleted += OnChapterDeleted;
       ServerNotification.OnReadCreated += OnReadCreated;
       ServerNotification.OnReadDeleted += OnReadDeleted;
   }

   ~Serie()
   {
       ServerNotification.OnSerieUpdated -= OnSerieUpdated;
       ServerNotification.OnPosterModified -= OnPosterModified;
       ServerNotification.OnChapterAdded -= OnChapterAdded;
       ServerNotification.OnChapterDeleted -= OnChapterDeleted;
       ServerNotification.OnReadCreated -= OnReadCreated;
       ServerNotification.OnReadDeleted -= OnReadDeleted;
   }
   
   private static void OnSerieCreated(SerieDto dto)
   {
       Serie serie = new(dto);
       serie.LoadInfo();
       serie.LoadPoster();
       lock (SeriesLock)
       {
           Series.AddOrUpdate(serie);
       }
   }

   private static void OnSerieDeleted(long id)
   {
       lock (SeriesLock)
       {
           Series.RemoveKey(id);
       }
   }
   
   private void FromSerieDto(SerieDto dto)
   {
       Id = dto.Id;
       Title = dto.Title;
       Description = dto.Description;
       Status = dto.Status;
       Creation = dto.Creation;
       LastModification = dto.LastModification;
       Tags = dto.Tags.Select(t => new Tag(t)).ToList();
       LibraryId = dto.LibraryId;
   }

   public static void LoadSeries()
   {
         Task.Run(() =>
         {
             lock (LoadLock)
             {
                 try
                 {
                     Optional<List<long>> seriesIdsResponse = ManaxApiSerieClient.GetSeriesIdsAsync().Result;
                     if (seriesIdsResponse.Failed)
                     {
                         Logger.LogFailure(seriesIdsResponse.Error,Environment.StackTrace);
                         return;
                     }
    
                     List<long> seriesIds = seriesIdsResponse.GetValue();
                     lock (SeriesLock)
                     {
                         Series.AddOrUpdate(seriesIds.Select(serieId => new Serie(serieId)));
                     }
                 }
                 catch (Exception e)
                 {
                     Logger.LogError("Failed to load series", e, Environment.StackTrace);
                 }
             }
         });
   }
   
   public void LoadInfo()
   {
       if (_infoLoaded) return;
       Task.Run(() =>
       {
           try
           {
               Optional<SerieDto> serieInfoResponse = ManaxApiSerieClient.GetSerieInfoAsync(Id).Result;
               if (serieInfoResponse.Failed) ErrorEmitted?.Invoke(this, serieInfoResponse.Error);

               FromSerieDto(serieInfoResponse.GetValue());
                _infoLoaded = true;
           }
           catch (Exception e)
           {
               string message = "Failed to load serie with ID: " + Id;
               ErrorEmitted?.Invoke(this, message);
               Logger.LogError(message, e, Environment.StackTrace);
           }
       });
   }
   
   public void LoadPoster()
   {
         if (_posterLoaded) return;
       Task.Run(() =>
       {
           try
           {
               Optional<byte[]> seriePosterResponse = ManaxApiSerieClient.GetSeriePosterAsync(Id).Result;
               if (seriePosterResponse.Failed)
               {
                   Poster = null;
                   ErrorEmitted?.Invoke(this, seriePosterResponse.Error);
                   return;
               }

               Poster = new Bitmap(new MemoryStream(seriePosterResponse.GetValue()));
                _posterLoaded = true;
           }
           catch (Exception e)
           {
               string message = "Failed to load poster for serie with ID: " + Id;
               Logger.LogError(message, e, Environment.StackTrace);
               ErrorEmitted?.Invoke(this, message);
           }
       });
   }

    public void LoadChapters()
    {
        if (_chaptersLoaded) return;
        Task.Run(() =>
        {
            try
            {
                Optional<List<long>> response = ManaxApiSerieClient.GetSerieChaptersAsync(Id).Result;
                if (response.Failed)
                {
                    return response.Error;
                }

                List<long> chaptersIds = response.GetValue();
                List<ChapterDto> chapters = [];
                foreach (long chapterId in chaptersIds)
                {
                    Optional<ChapterDto> chapterResponse = ManaxApiChapterClient.GetChapterAsync(chapterId).Result;
                    if (chapterResponse.Failed)
                    {
                        ErrorEmitted?.Invoke(this, chapterResponse.Error);
                        continue;
                    }
                    chapters.Add(chapterResponse.GetValue());
                }

                Dispatcher.UIThread.Invoke(() =>
                {
                    foreach (ChapterDto chapter in chapters) Chapters.Add(new Chapter(chapter));
                });
                _chaptersLoaded = true;
                LoadReads(Id);
            }
            catch (Exception e)
            {
                string message = "Failed to load chapters for serie with ID: " + Id;
                ErrorEmitted?.Invoke(this, message);
                Logger.LogError(message, e, Environment.StackTrace);
            }

            return null;
        });
    }
    
    private void LoadReads(long serieId)
    {
        try
        {
            Optional<List<ReadDto>> response = ManaxApiSerieClient.GetSerieChaptersReadAsync(serieId).Result;
            if (response.Failed)
            {
                ErrorEmitted?.Invoke(this, response.Error);
                return;
            }

            List<ReadDto> reads = response.GetValue();

            Dispatcher.UIThread.Post(() =>
            {
                foreach (ReadDto read in reads)
                {
                    Chapter? chapter = Chapters.FirstOrDefault(c => c.Id == read.ChapterId);
                    if (chapter == null) continue;
                    chapter.Read = read;
                }
            });
        }
        catch (Exception e)
        {
            string message = "Failed to load chapters for serie with ID: " + serieId;
            ErrorEmitted?.Invoke(this, message);
            Logger.LogError(message, e, Environment.StackTrace);
        }
    }
    
    private void OnReadDeleted(long obj)
    {
        Chapter? chapter = Chapters.FirstOrDefault(c => c.Id == obj);
        if (chapter == null) return;
        chapter.Read = null;
    }

    private void OnReadCreated(ReadDto read)
    {
        Chapter? chapter = Chapters.FirstOrDefault(c => c.Id == read.ChapterId);
        if (chapter == null) return;
        chapter.Read = read;
    }

    private void OnSerieUpdated(SerieDto serie)
    {
        if (serie.Id != Id) return;
        FromSerieDto(serie);
    }

    private void OnChapterAdded(ChapterDto chapter)
    {
        if (chapter.SerieId != Id) return;
        Chapters.Add(new Chapter(chapter));
    }

    private void OnChapterDeleted(long chapterId)
    {
        Chapter? chapter = Chapters.FirstOrDefault(c => c.Id == chapterId);
        if (chapter == null) return;
        Chapters.Remove(chapter);
    }
    
    private void OnPosterModified(long id)
    {
        if (id != Id) return;
        LoadPoster();
    }
}