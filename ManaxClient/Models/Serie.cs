using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
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
    private readonly ReadOnlyObservableCollection<Tag> _tags;
    private readonly ReadOnlyObservableCollection<Person> _persons;
    
    private SourceList<long> _tagIds = new();
    private SourceList<long> _personIds = new();
    
    [ObservableProperty] private DateTime _creation;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private long _id;
    [ObservableProperty] private DateTime _lastModification;
    [ObservableProperty] private long? _libraryId;
    [ObservableProperty] private Bitmap? _poster;
    [ObservableProperty] private Status _status;
    [ObservableProperty] private string _title = string.Empty;
    
    private bool _posterLoaded;
    private bool _infoLoaded;

    public Serie(long id) : this(new SerieDto { Id = id })
    {
    }

    public Serie(SerieDto dto)
    {
        ServerNotification.OnSerieUpdated += OnSerieUpdated;
        ServerNotification.OnPosterModified += OnPosterModified;
        ServerNotification.OnReadCreated += OnReadCreated;
        ServerNotification.OnReadDeleted += OnReadDeleted;
        
        FromSerieDto(dto);
        ChapterSource.Chapters
            .Connect()
            .Filter(chapter => chapter.SerieId == Id)
            .SortAndBind(out _chapters, SortExpressionComparer<Chapter>.Ascending(chapter => chapter.Number))
            .Subscribe();
        TagSource.Tags
            .Connect()
            .Filter(_tagIds.Connect().Select(_ => (Func<Tag, bool>)(tag => _tagIds.Items.Contains(tag.Id))))
            .SortAndBind(out _tags, SortExpressionComparer<Tag>.Ascending(tag => tag.Name))
            .Subscribe();

        PersonSource.Persons
            .Connect()
            .Filter(_personIds.Connect().Select(_ => (Func<Person, bool>)(person => _personIds.Items.Contains(person.Id))))
            .SortAndBind(out _persons, SortExpressionComparer<Person>.Ascending(person => person.LastName))
            .Subscribe();
    }

    public ReadOnlyObservableCollection<Chapter> Chapters => _chapters;
    public ReadOnlyObservableCollection<Tag> Tags => _tags;
    public ReadOnlyObservableCollection<Person> Persons => _persons;

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
        LibraryId = dto.LibraryId;
        _tagIds.Clear();
        _tagIds.AddRange(dto.TagIds);
        _personIds.Clear();
        _personIds.AddRange(dto.PersonIds);
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