using System;
using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Chapter;

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
}