using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models.Collections;
using ManaxClient.ViewModels.Pages.Serie;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Search;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.ViewModels.Pages;

public abstract partial class BaseSeries : PageViewModel
{
    private readonly object _serieLock = new();
    [ObservableProperty] private SortedObservableCollection<Models.Serie.Serie> _series;

    protected BaseSeries()
    {
        Series = new SortedObservableCollection<Models.Serie.Serie>([])
            { SortingSelector = serie => serie.Title, Descending = false };
        ServerNotification.OnSerieCreated += OnSerieCreated;
        ServerNotification.OnSerieDeleted += OnSerieDeleted;
    }

    ~BaseSeries()
    {
        ServerNotification.OnSerieCreated -= OnSerieCreated;
        ServerNotification.OnSerieDeleted -= OnSerieDeleted;
    }

    protected abstract void OnSerieCreated(SerieDto serie);

    protected void AddSerieToCollection(SerieDto serieDto)
    {
        if (Series.Any(s => s.Id == serieDto.Id)) { return; }
        Models.Serie.Serie serie = new(serieDto);
        serie.ErrorEmitted += (_, info) => { InfoEmitted?.Invoke(this, info); };
        serie.LoadInfo();
        serie.LoadPoster();
        Dispatcher.UIThread.Post(() =>
        {
            lock (_serieLock)
            {
                Series.Add(serie);
            }
        });
    }

    private void OnSerieDeleted(long serieId)
    {
        Dispatcher.UIThread.Post(() =>
        {
            lock (_serieLock)
            {
                Models.Serie.Serie? existingSerie = Series.FirstOrDefault(s => s.Id == serieId);
                if (existingSerie != null) Series.Remove(existingSerie);
            }
        });
    }

    protected async void LoadSeries(Search search)
    {
        try
        {
            Optional<List<long>> searchResultResponse = await ManaxApiSerieClient.GetSearchResult(search);
            if (searchResultResponse.Failed)
            {
                InfoEmitted?.Invoke(this, searchResultResponse.Error);
                return;
            }

            foreach (long serieId in searchResultResponse.GetValue())
            {
                lock (_serieLock)
                {
                    if (Series.FirstOrDefault(s => s.Id == serieId) != null) continue;
                }

                Optional<SerieDto> serieInfoAsync = await ManaxApiSerieClient.GetSerieInfoAsync(serieId);
                if (serieInfoAsync.Failed)
                {
                    InfoEmitted?.Invoke(this, serieInfoAsync.Error);
                    continue;
                }

                Models.Serie.Serie serie = new(serieInfoAsync.GetValue());
                serie.ErrorEmitted += (_, info) => { InfoEmitted?.Invoke(this, info); };
                serie.LoadInfo();
                serie.LoadPoster();

                Dispatcher.UIThread.Post(() =>
                {
                    lock (_serieLock)
                    {
                        Series.Add(serie);
                    }
                });
            }
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to load series", e, Environment.StackTrace);
        }
    }

    public void MoveToSeriePage(Models.Serie.Serie serie)
    {
        SeriePageViewModel seriePageViewModel = new(serie.Id);
        PageChangedRequested?.Invoke(this, seriePageViewModel);
    }
}