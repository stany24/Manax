using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models;
using ManaxClient.Models.Collections;
using ManaxClient.ViewModels.Serie;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Search;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.ViewModels;

public abstract partial class BaseSeriesViewModel : PageViewModel
{
    private readonly object _serieLock = new();
    [ObservableProperty] private SortedObservableCollection<ClientSerie> _series;

    protected BaseSeriesViewModel()
    {
        Series = new SortedObservableCollection<ClientSerie>([])
            { SortingSelector = serie => serie.Info.Title, Descending = false };
        ServerNotification.OnSerieCreated += OnSerieCreated;
        ServerNotification.OnPosterModified += OnPosterModified;
        ServerNotification.OnSerieDeleted += OnSerieDeleted;
    }

    ~BaseSeriesViewModel()
    {
        ServerNotification.OnSerieCreated -= OnSerieCreated;
        ServerNotification.OnPosterModified -= OnPosterModified;
        ServerNotification.OnSerieDeleted -= OnSerieDeleted;
    }

    protected abstract void OnSerieCreated(SerieDto serie);

    protected void AddSerieToCollection(SerieDto serie)
    {
        Dispatcher.UIThread.Post(() =>
        {
            lock (_serieLock)
            {
                ClientSerie? existingSerie = Series.FirstOrDefault(s => s.Info.Id == serie.Id);
                if (existingSerie != null) return;
                Series.Add(new ClientSerie(serie));
            }
        });
    }

    private void OnSerieDeleted(long serieId)
    {
        Dispatcher.UIThread.Post(() =>
        {
            lock (_serieLock)
            {
                ClientSerie? existingSerie = Series.FirstOrDefault(s => s.Info.Id == serieId);
                if (existingSerie != null) Series.Remove(existingSerie);
            }
        });
    }

    private async void OnPosterModified(long serieId)
    {
        try
        {
            ClientSerie? serie;
            lock (_serieLock)
            {
                serie = Series.FirstOrDefault(s => s.Info.Id == serieId);
            }

            if (serie == null) return;
            Optional<byte[]> seriePosterResponse = await ManaxApiSerieClient.GetSeriePosterAsync(serieId);
            if (seriePosterResponse.Failed)
            {
                InfoEmitted?.Invoke(this, seriePosterResponse.Error);
                return;
            }

            try
            {
                Bitmap poster = new(new MemoryStream(seriePosterResponse.GetValue()));
                Dispatcher.UIThread.Post(() => serie.Poster = poster);
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to load the new poster for serie with ID: " + serieId, e,
                    Environment.StackTrace);
            }
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to receive the new poster for serie with ID: " + serieId, e,
                Environment.StackTrace);
        }
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
                    if (Series.FirstOrDefault(s => s.Info.Id == serieId) != null) continue;
                }

                Optional<SerieDto> serieInfoAsync = await ManaxApiSerieClient.GetSerieInfoAsync(serieId);
                if (serieInfoAsync.Failed)
                {
                    InfoEmitted?.Invoke(this, serieInfoAsync.Error);
                    continue;
                }

                ClientSerie serie = new(serieInfoAsync.GetValue());
                serie.InfoEmitted += (_, info) => { InfoEmitted?.Invoke(this, info); };

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

    public void MoveToSeriePage(ClientSerie serie)
    {
        SeriePageViewModel seriePageViewModel = new(serie.Info.Id);
        PageChangedRequested?.Invoke(this, seriePageViewModel);
    }
}