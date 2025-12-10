using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using DynamicData.Binding;
using ManaxClient.Event;
using ManaxClient.ViewModels;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Read;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Server.Data;

public partial class Serie : ObservableObject, IDisposable
{
    private readonly ReadOnlyObservableCollection<Chapter> _chapters;
    private readonly SourceList<long> _personIds = new();
    private readonly ReadOnlyObservableCollection<Person> _persons;
    private readonly SourceList<long> _tagIds = new();
    private readonly ReadOnlyObservableCollection<Tag> _tags;
    [ObservableProperty] private Bitmap? _banner;
    private bool _bannerLoaded;

    [ObservableProperty] private DateTime _creation;
    [ObservableProperty] private string _description = string.Empty;
    [ObservableProperty] private long _id;
    private bool _infoLoaded;
    [ObservableProperty] private DateTime _lastModification;
    [ObservableProperty] private long? _libraryId;
    [ObservableProperty] private Bitmap? _poster;

    private bool _posterLoaded;
    [ObservableProperty] private Status _status;
    [ObservableProperty] private string _title = string.Empty;

    public Serie(long id) : this(new SerieDto { Id = id })
    {
    }

    public Serie(SerieDto dto)
    {
        NotificationReceiver.OnSerieUpdated += OnSerieUpdated;
        NotificationReceiver.OnPosterModified += OnPosterModified;
        NotificationReceiver.OnReadCreated += OnReadCreated;
        NotificationReceiver.OnReadDeleted += OnReadDeleted;

        FromSerieDto(dto);
        MainWindowViewModel.Instance.ChapterSource.Chapters
            .Connect()
            .Filter(chapter => chapter.SerieId == Id)
            .SortAndBind(out _chapters, SortExpressionComparer<Chapter>.Ascending(chapter => chapter.Number))
            .Subscribe();
        MainWindowViewModel.Instance.TagSource.Tags
            .Connect()
            .Filter(_tagIds.Connect().Select(_ => (Func<Tag, bool>)(tag => _tagIds.Items.Contains(tag.Id))))
            .SortAndBind(out _tags, SortExpressionComparer<Tag>.Ascending(tag => tag.Name))
            .Subscribe();

        MainWindowViewModel.Instance.PersonSource.Persons
            .Connect()
            .Filter(_personIds.Connect()
                .Select(_ => (Func<Person, bool>)(person => _personIds.Items.Contains(person.Id))))
            .SortAndBind(out _persons, SortExpressionComparer<Person>.Ascending(person => person.LastName))
            .Subscribe();
    }

    public ReadOnlyObservableCollection<Chapter> Chapters => _chapters;
    public ReadOnlyObservableCollection<Tag> Tags => _tags;
    public ReadOnlyObservableCollection<Person> Persons => _persons;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~Serie()
    {
        Dispose(false);
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
        Task.Run(async () =>
        {
            try
            {
                Optional<SerieDto> serieInfoResponse = await ManaxApiSerieClient.GetSerieInfoAsync(Id);
                if (serieInfoResponse.Failed) 
                    WeakReferenceMessenger.Default.Send(new NotificationMessage(serieInfoResponse.Error));

                FromSerieDto(serieInfoResponse.GetValue());
                _infoLoaded = true;
            }
            catch (Exception e)
            {
                string message = "Failed to load serie with ID: " + Id;
                WeakReferenceMessenger.Default.Send(new NotificationMessage(message));
                Logger.LogError(message, e);
            }
        });
    }

    public void LoadPoster()
    {
        if (_posterLoaded) return;
        Task.Run(async () =>
        {
            try
            {
                Optional<byte[]> seriePosterResponse = await ManaxApiSerieClient.GetSeriePosterAsync(Id);
                if (seriePosterResponse.Failed)
                {
                    Poster = null;
                    WeakReferenceMessenger.Default.Send(new NotificationMessage(seriePosterResponse.Error));
                    return;
                }

                Poster = new Bitmap(new MemoryStream(seriePosterResponse.GetValue()));
                _posterLoaded = true;
            }
            catch (Exception e)
            {
                string message = "Failed to load poster for serie with ID: " + Id;
                Logger.LogError(message, e);
                WeakReferenceMessenger.Default.Send(new NotificationMessage(message));
            }
        });
    }

    public void LoadBanner()
    {
        if (_bannerLoaded) return;
        Task.Run(async () =>
        {
            try
            {
                Optional<byte[]> serieBannerResponse = await ManaxApiSerieClient.GetSerieBannerAsync(Id);
                if (serieBannerResponse.Failed)
                {
                    Banner = null;
                    return;
                }

                Banner = new Bitmap(new MemoryStream(serieBannerResponse.GetValue()));
                _bannerLoaded = true;
            }
            catch (Exception)
            {
                // ignored
            }
        });
    }

    public void LoadChapters()
    {
        MainWindowViewModel.Instance.ChapterSource.LoadSerieChapters(Id);
    }

    private void OnReadDeleted(long obj)
    {
        Chapter? chapter = Chapters.FirstOrDefault(c => c.Id == obj);
        chapter?.Read = null;
    }

    private void OnReadCreated(ReadDto read)
    {
        Chapter? chapter = Chapters.FirstOrDefault(c => c.Id == read.ChapterId);
        chapter?.Read = read;
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

    private void ReleaseUnmanagedResources()
    {
        NotificationReceiver.OnSerieUpdated -= OnSerieUpdated;
        NotificationReceiver.OnPosterModified -= OnPosterModified;
        NotificationReceiver.OnReadCreated -= OnReadCreated;
        NotificationReceiver.OnReadDeleted -= OnReadDeleted;
    }

    private void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (!disposing) return;
        _personIds.Dispose();
        _tagIds.Dispose();
        Poster?.Dispose();
    }
}