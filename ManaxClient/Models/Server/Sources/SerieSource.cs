using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using ManaxClient.Models.Server.Data;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Server.Sources;

public static class SerieSource
{
    public static readonly SourceCache<Serie, long> Series = new(serie => serie.Id);
    private static bool _isLoaded;
    private static readonly Lock SeriesLock = new();
    private static readonly Lock LoadLock = new();

    static SerieSource()
    {
        ServerNotification.OnSerieCreated += OnSerieCreated;
        ServerNotification.OnSerieDeleted += OnSerieDeleted;
        LoadSeries();
    }

    private static void OnSerieCreated(SerieDto dto)
    {
        Serie serie = new(dto);
        serie.LoadInfo();
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

    private static void LoadSeries()
    {
        Task.Run(() =>
        {
            lock (LoadLock)
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
                    lock (SeriesLock)
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