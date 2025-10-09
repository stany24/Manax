using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
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

    [ObservableProperty] private string _pageTitle = string.Empty;
    [ObservableProperty] private string _pageSubtitle = string.Empty;
    [ObservableProperty] private string _diskUsageTitle = string.Empty;
    [ObservableProperty] private string _usedDiskLabel = string.Empty;
    [ObservableProperty] private string _availableDiskLabel = string.Empty;
    [ObservableProperty] private string _userActivityTitle = string.Empty;
    [ObservableProperty] private string _activeUsersLabel = string.Empty;
    [ObservableProperty] private string _inactiveUsersLabel = string.Empty;
    [ObservableProperty] private string _serverContentTitle = string.Empty;
    [ObservableProperty] private string _seriesLabel = string.Empty;
    [ObservableProperty] private string _chaptersLabel = string.Empty;
    [ObservableProperty] private string _libraryDistributionTitle = string.Empty;
    [ObservableProperty] private string _neverReadSeriesTitle = string.Empty;
    [ObservableProperty] private string _usedText = string.Empty;
    [ObservableProperty] private string _availableText = string.Empty;
    [ObservableProperty] private string _activeText = string.Empty;
    [ObservableProperty] private string _inactiveText = string.Empty;

    public ServerStatsPageViewModel()
    {
        BindLocalizedStrings();
        Task.Run(LoadServerStats);
    }

    private void BindLocalizedStrings()
    {
        Localize(() => PageTitle, "ServerStatsPage.Title");
        Localize(() => PageSubtitle, "ServerStatsPage.Subtitle");
        Localize(() => DiskUsageTitle, "ServerStatsPage.DiskUsage.Title");
        Localize(() => UsedDiskLabel, "ServerStatsPage.DiskUsage.Used");
        Localize(() => AvailableDiskLabel, "ServerStatsPage.DiskUsage.Available");
        Localize(() => UserActivityTitle, "ServerStatsPage.UserActivity.Title");
        Localize(() => ActiveUsersLabel, "ServerStatsPage.UserActivity.Active");
        Localize(() => InactiveUsersLabel, "ServerStatsPage.UserActivity.Inactive");
        Localize(() => ServerContentTitle, "ServerStatsPage.Content.Title");
        Localize(() => SeriesLabel, "ServerStatsPage.Content.Series");
        Localize(() => ChaptersLabel, "ServerStatsPage.Content.Chapters");
        Localize(() => LibraryDistributionTitle, "ServerStatsPage.LibraryDistribution.Title");
        Localize(() => NeverReadSeriesTitle, "ServerStatsPage.NeverRead.Title");
        Localize(() => UsedText, "ServerStatsPage.Charts.Used");
        Localize(() => AvailableText, "ServerStatsPage.Charts.Available");
        Localize(() => ActiveText, "ServerStatsPage.Charts.Active");
        Localize(() => InactiveText, "ServerStatsPage.Charts.Inactive");
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
            Name = UsedText,
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
            Name = AvailableText,
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
            Name = ActiveText,
            Fill = new SolidColorPaint(SKColors.DodgerBlue),
            DataLabelsPaint = new SolidColorPaint(SKColors.White),
            DataLabelsSize = 12,
            DataLabelsPosition = PolarLabelsPosition.Middle
        });
        UserActivitySeries.Add(new PieSeries<int>
        {
            Values = [ServerStats.Users - ServerStats.ActiveUsers],
            Name = InactiveText,
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