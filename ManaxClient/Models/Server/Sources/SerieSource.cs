using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using ManaxClient.Event;
using ManaxClient.Models.Server.Data;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Server.Sources;

public class SerieSource
{
    public readonly SourceCache<Serie, long> Series = new(serie => serie.Id);
    private bool _isLoaded;
    private readonly Lock _seriesLock = new();
    private readonly Lock _loadLock = new();

    public SerieSource()
    {
        NotificationReceiver.OnSerieCreated += OnSerieCreated;
        NotificationReceiver.OnSerieDeleted += OnSerieDeleted;
        WeakReferenceMessenger.Default.Register<LoggedInMessage>(this, (_,_) => LoadSeries());
    }

    private void OnSerieCreated(SerieDto dto)
    {
        Serie serie = new(dto);
        serie.LoadInfo();
        lock (_seriesLock)
        {
            Series.AddOrUpdate(serie);
        }
    }

    private void OnSerieDeleted(long id)
    {
        lock (_seriesLock)
        {
            Series.RemoveKey(id);
        }
    }

    private void LoadSeries()
    {
        Task.Run(() =>
        {
            lock (_loadLock)
            {
                if (_isLoaded) return;
                try
                {
                    Optional<List<long>> seriesIdsResponse = ManaxApiSerieClient.GetSeriesIdsAsync().Result;
                    if (seriesIdsResponse.Failed)
                    {
                        Logger.LogFailure(seriesIdsResponse.Error);
                        return;
                    }

                    List<long> seriesIds = seriesIdsResponse.GetValue();
                    lock (_seriesLock)
                    {
                        Series.AddOrUpdate(seriesIds.Select(serieId => new Serie(serieId)));
                        _isLoaded = true;
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError("Failed to load series", e);
                }
            }
        });
    }
}