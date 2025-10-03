using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models;

public partial class Chapter:ObservableObject
{
    [ObservableProperty] private long _id;
    [ObservableProperty] private long _serieId;
    [ObservableProperty] private string _fileName = string.Empty;
    [ObservableProperty] private int _number;
    [ObservableProperty] private int _pageNumber;

    [ObservableProperty] private DateTime _creation;
    [ObservableProperty] private DateTime _lastModification;
    [ObservableProperty] private ObservableCollection<Bitmap> _pages = [];
    [ObservableProperty] private ReadDto? _read;
    
    public EventHandler<string>? ErrorEmitted;
    
    private CancellationTokenSource? _loadPagesCts;

    public Chapter(ChapterDto chapter)
    {
        FromChapterDto(chapter);
        ServerNotification.OnChapterModified += OnChapterModified;
    }

    public Chapter():this(new ChapterDto())
    {
    }

    ~Chapter()
    {
        
        ServerNotification.OnChapterModified -= OnChapterModified;
    }
    
    private void OnChapterModified(ChapterDto chapter)
    {
        if (chapter.Id != Id) return;
        FromChapterDto(chapter);
    }
    
    private void FromChapterDto(ChapterDto dto)
    {
        Id = dto.Id;
        SerieId = dto.SerieId;
        FileName = dto.FileName;
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
        
        Task.Run(async () =>
        {
            Pages = new ObservableCollection<Bitmap>(new Bitmap[PageNumber]);
            for (int i = 0; i < PageNumber; i++)
            {
                if (token.IsCancellationRequested)
                    break;

                int index = i;
                Optional<byte[]> chapterPageResponse = await ManaxApiChapterClient.GetChapterPageAsync(Id, i);
                if (chapterPageResponse.Failed)
                {
                    ErrorEmitted?.Invoke(this, chapterPageResponse.Error);
                    continue;
                }

                try
                {
                    Bitmap page = new(new MemoryStream(chapterPageResponse.GetValue()));
                    Pages[index] = page;
                }
                catch (Exception e)
                {
                    ErrorEmitted?.Invoke(this, "Erreur lors du chargement de la page " + index);
                    Logger.LogError("Échec du chargement de la page " + index + " pour le chapitre " + Id, e,
                        Environment.StackTrace);
                }
            }
        }, token);
    }
    
    public void MarkAsRead(int page)
    {
        if(page == 0){return;}
        ReadCreateDto readCreateDto = new()
        {
            ChapterId = Id,
            Page = page
        };
        Task.Run(async () =>
        {
            Optional<bool> response = await ManaxApiReadClient.MarkAsRead(readCreateDto);
            if (response.Failed) ErrorEmitted?.Invoke(this, response.Error);
        });
    }
}