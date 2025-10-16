using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Jeek.Avalonia.Localization;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Stats;
using ManaxLibrary.Logging;
using SkiaSharp;

namespace ManaxClient.ViewModels.Pages.Stats;

public partial class ServerStatsPageViewModel : PageViewModel
{
    [ObservableProperty] private double _availableDiskSizeInGb;
    [ObservableProperty] private double _diskSizeInGb;
    [ObservableProperty] private ServerStats? _serverStats;

    public ServerStatsPageViewModel()
    {
        Task.Run(LoadServerStats);
    }


    public ObservableCollection<Models.Serie> NeverReadSeries { get; set; } = new([]);
    public ObservableCollection<ISeries> UserActivitySeries { get; set; } = new([]);
    public ObservableCollection<ISeries> LibraryDistributionSeries { get; set; } = new([]);
    public ObservableCollection<ISeries> DiskUsageSeries { get; set; } = new([]);

    private async void LoadServerStats()
    {
        try
        {
            Optional<ServerStats> serverStats = await ManaxApiStatsClient.GetServerStats();
            if (serverStats.Failed)
            {
                Logger.LogFailure($"Failed to load server stats: {serverStats.Error}");
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
            Logger.LogError($"An error occurred while loading server stats: {e.Message}", e);
            InfoEmitted?.Invoke(this, $"An error occurred while loading server stats: {e.Message}");
        }
    }

    private void UpdateChartData()
    {
        if (ServerStats == null) return;

        DiskSizeInGb = ServerStats.DiskSize / 1024.0 / 1024.0 / 1024.0;
        AvailableDiskSizeInGb = ServerStats.AvailableDiskSize / 1024.0 / 1024.0 / 1024.0;

        DiskUsageSeries.Add(new PieSeries<double>
        {
            Values = [DiskSizeInGb],
            Name = Localizer.Get("ServerStatsPage.Charts.Used"),
            Fill = new SolidColorPaint(SKColors.OrangeRed),
            DataLabelsPaint = new SolidColorPaint(SKColors.White),
            DataLabelsSize = 12,
            DataLabelsPosition = PolarLabelsPosition.Middle,
            DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue:F2} GB",
            ToolTipLabelFormatter = point => $"{point.Coordinate.PrimaryValue:F2} GB"
        });
        DiskUsageSeries.Add(new PieSeries<double>
        {
            Values = [AvailableDiskSizeInGb],
            Name = Localizer.Get("ServerStatsPage.Charts.Available"),
            Fill = new SolidColorPaint(SKColors.Green),
            DataLabelsPaint = new SolidColorPaint(SKColors.White),
            DataLabelsSize = 12,
            DataLabelsPosition = PolarLabelsPosition.Middle,
            DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue:F2} GB",
            ToolTipLabelFormatter = point => $"{point.Coordinate.PrimaryValue:F2} GB"
        });

        UserActivitySeries.Add(new PieSeries<int>
        {
            Values = [ServerStats.ActiveUsers],
            Name = Localizer.Get("ServerStatsPage.Charts.Active"),
            Fill = new SolidColorPaint(SKColors.DodgerBlue),
            DataLabelsPaint = new SolidColorPaint(SKColors.White),
            DataLabelsSize = 12,
            DataLabelsPosition = PolarLabelsPosition.Middle
        });
        UserActivitySeries.Add(new PieSeries<int>
        {
            Values = [ServerStats.Users - ServerStats.ActiveUsers],
            Name = Localizer.Get("ServerStatsPage.Charts.Inactive"),
            Fill = new SolidColorPaint(SKColors.Gray),
            DataLabelsPaint = new SolidColorPaint(SKColors.White),
            DataLabelsSize = 12,
            DataLabelsPosition = PolarLabelsPosition.Middle
        });

        SKColor[] colors =
        [
            SKColors.Purple, SKColors.Orange, SKColors.Teal, SKColors.Pink, SKColors.Brown, SKColors.Navy
        ];
        int colorIndex = 0;

        foreach (KeyValuePair<string, int> library in ServerStats.SeriesInLibraries)
        {
            LibraryDistributionSeries.Add(new PieSeries<int>
            {
                Values = [library.Value],
                Name = library.Key,
                Fill = new SolidColorPaint(colors[colorIndex % colors.Length]),
                DataLabelsPaint = new SolidColorPaint(SKColors.White),
                DataLabelsSize = 12,
                DataLabelsPosition = PolarLabelsPosition.Middle
            });
            colorIndex++;
        }

        foreach (Models.Serie serie in ServerStats.NeverReadSeries.ConvertAll(s => new Models.Serie(s)))
            NeverReadSeries.Add(serie);
    }
}