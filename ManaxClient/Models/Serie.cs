using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using ManaxClient.Models.Sources;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models;

public partial class Serie : ObservableObject
{
    private readonly ReadOnlyObservableCollection<Chapter> _chapters;
    [ObservableProperty] private DateTime _creation;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private long _id;

    private bool _infoLoaded;
    [ObservableProperty] private DateTime _lastModification;
    [ObservableProperty] private long? _libraryId;
    [ObservableProperty] private Bitmap? _poster;
    private bool _posterLoaded;
    [ObservableProperty] private Status _status;
    [ObservableProperty] private List<Tag> _tags = [];
    [ObservableProperty] private string _title = string.Empty;

    public Serie(long id) : this(new SerieDto { Id = id })
    {
    }

    public Serie(SerieDto dto)
    {
        FromSerieDto(dto);
        ServerNotification.OnSerieUpdated += OnSerieUpdated;
        ServerNotification.OnPosterModified += OnPosterModified;
        ServerNotification.OnReadCreated += OnReadCreated;
        ServerNotification.OnReadDeleted += OnReadDeleted;
        SortExpressionComparer<Chapter> comparer = SortExpressionComparer<Chapter>.Ascending(chapter => chapter.Number);
        ChapterSource.Chapters
            .Connect()
            .Filter(chapter => chapter.SerieId == Id)
            .SortAndBind(out _chapters, comparer)
            .Subscribe();
    }

    public ReadOnlyObservableCollection<Chapter> Chapters => _chapters;

    public static EventHandler<string>? ErrorEmitted { get; set; }

    ~Serie()
    {
        ServerNotification.OnSerieUpdated -= OnSerieUpdated;
        ServerNotification.OnPosterModified -= OnPosterModified;
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
        Tags = dto.Tags.Select(t => new Tag(t)).ToList();
        LibraryId = dto.LibraryId;
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
                Logger.LogError(message, e);
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
                Logger.LogError(message, e);
                ErrorEmitted?.Invoke(this, message);
            }
        });
    }

    public void LoadChapters()
    {
        ChapterSource.LoadSerieChapters(Id);
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

    private void OnPosterModified(long id)
    {
        if (id != Id) return;
        LoadPoster();
    }
}