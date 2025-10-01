using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models.Collections;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.DTO.Tag;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Serie;

public partial class Serie:ObservableObject
{
   [ObservableProperty] private long _id;
   [ObservableProperty] private long? _libraryId;
   [ObservableProperty] private string _title = string.Empty;
   [ObservableProperty] private string _description = string.Empty;
   [ObservableProperty] private Status _status;

   [ObservableProperty] private DateTime _creation;
   [ObservableProperty] private DateTime _lastModification;
   [ObservableProperty] private List<TagDto> _tags = [];
   
   [ObservableProperty] private Bitmap? _poster;
   [ObservableProperty] private SortedObservableCollection<ClientChapter> _chapters= new([]) { SortingSelector = dto => dto.Info.Number };
   
   public EventHandler<string>? ErrorEmitted;

   public Serie(long id) : this(new SerieDto { Id = id })
   {
   }

   public Serie(SerieDto dto)
   {
       FromSerieDto(dto);
       ServerNotification.OnSerieUpdated += OnSerieUpdated;
       ServerNotification.OnPosterModified += OnPosterModified;
       ServerNotification.OnChapterAdded += OnChapterAdded;
       ServerNotification.OnChapterModified += OnChapterModified;
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
   
   private void FromSerieDto(SerieDto dto)
   {
       Id = dto.Id;
       Title = dto.Title;
       Description = dto.Description;
       Status = dto.Status;
       Creation = dto.Creation;
       LastModification = dto.LastModification;
       Tags = dto.Tags;
       LibraryId = dto.LibraryId;
   }
   
   public void LoadInfo()
   {
       Task.Run(() =>
       {
           try
           {
               Optional<SerieDto> serieInfoResponse = ManaxApiSerieClient.GetSerieInfoAsync(Id).Result;
               if (serieInfoResponse.Failed) ErrorEmitted?.Invoke(this, serieInfoResponse.Error);

               FromSerieDto(serieInfoResponse.GetValue());
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
                    foreach (ChapterDto chapter in chapters) Chapters.Add(new ClientChapter { Info = chapter });
                });
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
                    ClientChapter? chapter = Chapters.FirstOrDefault(c => c.Info.Id == read.ChapterId);
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
        ClientChapter? chapter = Chapters.FirstOrDefault(c => c.Info.Id == obj);
        if (chapter == null) return;
        chapter.Read = null;
    }

    private void OnReadCreated(ReadDto read)
    {
        ClientChapter? chapter = Chapters.FirstOrDefault(c => c.Info.Id == read.ChapterId);
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
        Chapters.Add(new ClientChapter { Info = chapter });
    }

    private void OnChapterModified(ChapterDto chapter)
    {
        if (chapter.SerieId != Id) return;
        ClientChapter? firstOrDefault = Chapters.FirstOrDefault(c => c.Info.Id == chapter.Id);
        if (firstOrDefault == null)
            Chapters.Add(new ClientChapter { Info = chapter });
        else
            firstOrDefault.Info = chapter;
    }

    private void OnChapterDeleted(long chapterId)
    {
        ClientChapter? chapter = Chapters.FirstOrDefault(c => c.Info.Id == chapterId);
        if (chapter == null) return;
        Chapters.Remove(chapter);
    }
    
    private void OnPosterModified(long id)
    {
        if (id != Id) return;
        LoadPoster();
    }
}