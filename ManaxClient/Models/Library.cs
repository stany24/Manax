using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models.Collections;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Library;
using ManaxLibrary.DTO.Search;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models;

public partial class Library:ObservableObject
{
    
    [ObservableProperty] private long _id;
    [ObservableProperty] private  string _name = string.Empty;
    [ObservableProperty] private  DateTime _creation;
    
    public SortedObservableCollection<Serie> Series { get; set; }
    
    
    public EventHandler<string>? ErrorEmitted;

    public Library(LibraryDto dto)
    {
        FromLibraryDto(dto);
        ServerNotification.OnLibraryUpdated += OnLibraryUpdated;
        ServerNotification.OnSerieCreated += OnSerieCreated;
        ServerNotification.OnSerieDeleted += OnSerieDeleted;
        Series = new SortedObservableCollection<Serie>([])
            { SortingSelector = serie => serie.Title, Descending = false };
    }

    public Library(long id):this(new LibraryDto { Id = id })
    {
    }
    
    ~Library()
    {
        ServerNotification.OnLibraryUpdated += OnLibraryUpdated;
        ServerNotification.OnSerieCreated -= OnSerieCreated;
        ServerNotification.OnSerieDeleted -= OnSerieDeleted;
    }

    private void FromLibraryDto(LibraryDto dto)
    {
        Id = dto.Id;
        Name = dto.Name;
        Creation = dto.Creation;
    }

    private void OnLibraryUpdated(LibraryDto dto)
    {
        if (dto.Id != Id) return;
        FromLibraryDto(dto);
    }
    
    private void OnSerieCreated(SerieDto dto)
    {
        if(dto.LibraryId != Id) return;
        if (Series.Any(s => s.Id == dto.Id)) { return; }
        Serie serie = new(dto);
        serie.ErrorEmitted += (_, info) => { ErrorEmitted?.Invoke(this, info); };
        serie.LoadInfo();
        serie.LoadPoster();
        Dispatcher.UIThread.Post(() =>
        {
            Series.Add(serie);
        });
    }

    private void OnSerieDeleted(long serieId)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Serie? existingSerie = Series.FirstOrDefault(s => s.Id == serieId);
            if (existingSerie != null) Series.Remove(existingSerie);
        });
    }
    
    public void LoadInfo()
    {
        Task.Run(() =>
        {
            try
            {
                Optional<LibraryDto> libraryResponse = ManaxApiLibraryClient.GetLibraryAsync(Id).Result;
                if (libraryResponse.Failed)
                {
                    ErrorEmitted?.Invoke(this, libraryResponse.Error);
                    return;
                }

                FromLibraryDto(libraryResponse.GetValue());
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to load the library with ID: " + Id, e, Environment.StackTrace);
            }
        });
    }
    
    public void LoadSeries(Search? search = null)
    {
        Task.Run(() =>
        {
            try
            {
                search ??= new Search { IncludedLibraries = [Id] };

                Optional<List<long>> searchResultResponse = ManaxApiSerieClient.GetSearchResult(search).Result;
                if (searchResultResponse.Failed)
                {
                    ErrorEmitted?.Invoke(this, searchResultResponse.Error);
                    return;
                }

                foreach (long serieId in searchResultResponse.GetValue())
                {
                    if (Series.FirstOrDefault(s => s.Id == serieId) != null) continue;

                    Optional<SerieDto> serieInfoAsync = ManaxApiSerieClient.GetSerieInfoAsync(serieId).Result;
                    if (serieInfoAsync.Failed)
                    {
                        ErrorEmitted?.Invoke(this, serieInfoAsync.Error);
                        continue;
                    }

                    Serie serie = new(serieInfoAsync.GetValue());
                    serie.ErrorEmitted += (_, info) => { ErrorEmitted?.Invoke(this, info); };
                    serie.LoadInfo();
                    serie.LoadPoster();

                    Dispatcher.UIThread.Post(() =>
                    {
                        Series.Add(serie);
                    });
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to load series", e, Environment.StackTrace);
            }
        });
    }
}