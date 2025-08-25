using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ManaxClient.Models;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Stats;
using ManaxLibrary.Logging;
using SkiaSharp;

namespace ManaxClient.ViewModels.Stats;

public partial class ServerStatsPageViewModel : PageViewModel
{
    [ObservableProperty] private ServerStats? _serverStats;
    [ObservableProperty] private IEnumerable<ISeries>? _diskUsageSeries;
    [ObservableProperty] private IEnumerable<ISeries>? _userActivitySeries;
    [ObservableProperty] private IEnumerable<ISeries>? _libraryDistributionSeries;
    [ObservableProperty] private List<ClientSerie> _neverReadSeries = [];
    [ObservableProperty] private double _diskSizeInGb;
    [ObservableProperty] private double _availableDiskSizeInGb;

    public ServerStatsPageViewModel()
    {
        Task.Run(LoadServerStats);
    }

    private async void LoadServerStats()
    {
        try
        {
            Optional<ServerStats> serverStats = await ManaxApiStatsClient.GetServerStats();
            if (serverStats.Failed)
            {
                Logger.LogFailure($"Failed to load server stats: {serverStats.Error}", Environment.StackTrace);
                InfoEmitted?.Invoke(this, $"Failed to load server stats: {serverStats.Error}");
                return;
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                ServerStats = serverStats.GetValue();
                UpdateChartData();
            });
        }
        catch (Exception e)
        {
            Logger.LogError($"An error occurred while loading server stats: {e.Message}", e, Environment.StackTrace);
            InfoEmitted?.Invoke(this, $"An error occurred while loading server stats: {e.Message}");
        }
    }

    private void UpdateChartData()
    {
        if (ServerStats == null) return;

        DiskSizeInGb = ServerStats.DiskSize / 1024.0 / 1024.0 / 1024.0;
        AvailableDiskSizeInGb = ServerStats.AvailableDiskSize / 1024.0 / 1024.0 / 1024.0;

        DiskUsageSeries =
        [
            new PieSeries<double>
            {
                Values = [DiskSizeInGb],
                Name = "Utilisé",
                Fill = new SolidColorPaint(SKColors.OrangeRed),
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsSize = 12,
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue:F2} GB",
                ToolTipLabelFormatter = point => $"{point.Coordinate.PrimaryValue:F2} GB"
            },
            new PieSeries<double>
            {
                Values = [AvailableDiskSizeInGb],
                Name = "Disponible",
                Fill = new SolidColorPaint(SKColors.Green),
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsSize = 12,
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle,
                DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue:F2} GB",
                ToolTipLabelFormatter = point => $"{point.Coordinate.PrimaryValue:F2} GB"
            }
        ];

        UserActivitySeries =
        [
            new PieSeries<int>
            {
                Values = [ServerStats.ActiveUsers],
                Name = "Actifs",
                Fill = new SolidColorPaint(SKColors.DodgerBlue),
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsSize = 12,
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle
            },
            new PieSeries<int>
            {
                Values = [ServerStats.Users - ServerStats.ActiveUsers],
                Name = "Inactifs",
                Fill = new SolidColorPaint(SKColors.Gray),
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsSize = 12,
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle
            }
        ];

        List<ISeries> librarySeries = [];
        SKColor[] colors = [SKColors.Purple, SKColors.Orange, SKColors.Teal, SKColors.Pink, SKColors.Brown, SKColors.Navy
        ];
        int colorIndex = 0;

        foreach (KeyValuePair<string, int> library in ServerStats.SeriesInLibraries)
        {
            librarySeries.Add(new PieSeries<int>
            {
                Values = [library.Value],
                Name = library.Key,
                Fill = new SolidColorPaint(colors[colorIndex % colors.Length]),
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsSize = 12,
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle
            });
            colorIndex++;
        }

        LibraryDistributionSeries = librarySeries;

        NeverReadSeries = ServerStats.NeverReadSeries.ConvertAll(s => new ClientSerie()
        {
            Info = s
        });

        Task.Run(async () =>
        {
            foreach (ClientSerie serie in NeverReadSeries)
            {
                Optional<byte[]> seriePosterAsync = await ManaxApiSerieClient.GetSeriePosterAsync(serie.Info.Id);
                if (seriePosterAsync.Failed)
                {
                    InfoEmitted?.Invoke(this, seriePosterAsync.Error);
                    continue;
                }
                try
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        serie.Poster = new Bitmap(new MemoryStream(seriePosterAsync.GetValue()));
                    });
                }
                catch (Exception e)
                {
                    Logger.LogError("Failed to convert byte[] to image", e, Environment.StackTrace);
                    InfoEmitted?.Invoke(this, "Invalid image received for serie " + serie.Info.Id);
                    serie.Poster = null;
                }
            }
        });
    }
}