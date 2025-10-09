using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
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

public partial class UserStatsPageViewModel : PageViewModel
{
    [ObservableProperty] private UserStats? _userStats;
    
    [ObservableProperty] private string _pageTitle = string.Empty;
    [ObservableProperty] private string _pageSubtitle = string.Empty;
    [ObservableProperty] private string _seriesProgressTitle = string.Empty;
    [ObservableProperty] private string _seriesCompletedLabel = string.Empty;
    [ObservableProperty] private string _seriesInProgressLabel = string.Empty;
    [ObservableProperty] private string _seriesRemainingLabel = string.Empty;
    [ObservableProperty] private string _chaptersProgressTitle = string.Empty;
    [ObservableProperty] private string _chaptersReadLabel = string.Empty;
    [ObservableProperty] private string _chaptersRemainingLabel = string.Empty;
    [ObservableProperty] private string _rankDistributionTitle = string.Empty;
    [ObservableProperty] private string _readsPerDayTitle = string.Empty;
    [ObservableProperty] private string _completedText = string.Empty;
    [ObservableProperty] private string _inProgressText = string.Empty;
    [ObservableProperty] private string _remainingText = string.Empty;
    [ObservableProperty] private string _readText = string.Empty;
    [ObservableProperty] private string _remainingChaptersText = string.Empty;
    [ObservableProperty] private string _seriesCountText = string.Empty;
    [ObservableProperty] private string _dailyReadsText = string.Empty;

    public UserStatsPageViewModel()
    {
        BindLocalizedStrings();
        Task.Run(LoadUserStats);
    }

    private void BindLocalizedStrings()
    {
        Localize(() => PageTitle, "UserStatsPage.Title");
        Localize(() => PageSubtitle, "UserStatsPage.Subtitle");
        Localize(() => SeriesProgressTitle, "UserStatsPage.SeriesProgress.Title");
        Localize(() => SeriesCompletedLabel, "UserStatsPage.SeriesProgress.Completed");
        Localize(() => SeriesInProgressLabel, "UserStatsPage.SeriesProgress.InProgress");
        Localize(() => SeriesRemainingLabel, "UserStatsPage.SeriesProgress.Remaining");
        Localize(() => ChaptersProgressTitle, "UserStatsPage.ChaptersProgress.Title");
        Localize(() => ChaptersReadLabel, "UserStatsPage.ChaptersProgress.Read");
        Localize(() => ChaptersRemainingLabel, "UserStatsPage.ChaptersProgress.Remaining");
        Localize(() => RankDistributionTitle, "UserStatsPage.RankDistribution.Title");
        Localize(() => ReadsPerDayTitle, "UserStatsPage.ReadsPerDay.Title");
        Localize(() => CompletedText, "UserStatsPage.Charts.Completed");
        Localize(() => InProgressText, "UserStatsPage.Charts.InProgress");
        Localize(() => RemainingText, "UserStatsPage.Charts.Remaining");
        Localize(() => ReadText, "UserStatsPage.Charts.Read");
        Localize(() => RemainingChaptersText, "UserStatsPage.Charts.RemainingChapters");
        Localize(() => SeriesCountText, "UserStatsPage.Charts.SeriesCount");
        Localize(() => DailyReadsText, "UserStatsPage.Charts.DailyReads");
    }

    public ObservableCollection<Axis> ReadsXAxes { get; set; } = new([new Axis()]);
    public ObservableCollection<Axis> XAxes { get; set; } = new([new Axis()]);
    public ObservableCollection<ISeries> ChaptersSeries { get; set; } = new([]);
    public ObservableCollection<ISeries> RanksSeries { get; set; } = new([]);
    public ObservableCollection<ISeries> ReadsPerDaySeries { get; set; } = new([]);
    public ObservableCollection<ISeries> SeriesSeries { get; set; } = new([]);

    private async void LoadUserStats()
    {
        try
        {
            Optional<UserStats> userStats = await ManaxApiStatsClient.GetUserStats();
            if (userStats.Failed)
            {
                Logger.LogFailure($"Failed to load user stats: {userStats.Error}");
                InfoEmitted?.Invoke(this, $"Failed to load user stats: {userStats.Error}");
                return;
            }

            Dispatcher.UIThread.Post(() =>
            {
                UserStats = userStats.GetValue();
                UpdateChartData();
            });
        }
        catch (Exception e)
        {
            Logger.LogError($"An error occurred while loading user stats: {e.Message}", e);
            InfoEmitted?.Invoke(this, $"An error occurred while loading user stats: {e.Message}");
        }
    }

    private void UpdateChartData()
    {
        if (UserStats == null) return;
        SeriesSeries.Add(new PieSeries<long>
        {
            Values = [UserStats.SeriesCompleted],
            Name = CompletedText,
            Fill = new SolidColorPaint(SKColors.Green),
            DataLabelsPaint = new SolidColorPaint(SKColors.White),
            DataLabelsSize = 12,
            DataLabelsPosition = PolarLabelsPosition.Middle
        });
        SeriesSeries.Add(new PieSeries<long>
        {
            Values = [UserStats.SeriesInProgress],
            Name = InProgressText,
            Fill = new SolidColorPaint(SKColors.Orange),
            DataLabelsPaint = new SolidColorPaint(SKColors.White),
            DataLabelsSize = 12,
            DataLabelsPosition = PolarLabelsPosition.Middle
        });
        SeriesSeries.Add(new PieSeries<long>
        {
            Values = [UserStats.SeriesRemaining],
            Name = RemainingText,
            Fill = new SolidColorPaint(SKColors.Gray),
            DataLabelsPaint = new SolidColorPaint(SKColors.White),
            DataLabelsSize = 12,
            DataLabelsPosition = PolarLabelsPosition.Middle
        });

        ChaptersSeries.Add(new PieSeries<long>
        {
            Values = [UserStats.ChaptersRead],
            Name = ReadText,
            Fill = new SolidColorPaint(SKColors.Green),
            DataLabelsPaint = new SolidColorPaint(SKColors.White),
            DataLabelsSize = 12,
            DataLabelsPosition = PolarLabelsPosition.Middle
        });
        ChaptersSeries.Add(new PieSeries<long>
        {
            Values = [UserStats.ChaptersRemaining],
            Name = RemainingChaptersText,
            Fill = new SolidColorPaint(SKColors.Gray),
            DataLabelsPaint = new SolidColorPaint(SKColors.White),
            DataLabelsSize = 12,
            DataLabelsPosition = PolarLabelsPosition.Middle
        });

        if (UserStats.Ranks.Count > 0)
        {
            List<RankCount> sortedRanks = UserStats.Ranks.OrderBy(r => r.Rank.Value).ToList();
            List<double> values = [];
            List<string> labels = [];

            foreach (RankCount rankCount in sortedRanks)
            {
                values.Add(rankCount.Count);
                labels.Add(rankCount.Rank.Name);
            }

            RanksSeries.Add(new ColumnSeries<double>
            {
                Values = values,
                Name = SeriesCountText,
                Fill = new SolidColorPaint(SKColors.DodgerBlue)
            });

            XAxes.Clear();
            XAxes.Add(new Axis
            {
                Labels = labels,
                LabelsRotation = -15,
                TextSize = 14
            });
        }

        if (UserStats.Reads.Count > 0)
        {
            var readsPerDay = UserStats.Reads
                .GroupBy(r => r.Date.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToList();

            List<double> readCounts = readsPerDay.Select(x => (double)x.Count).ToList();
            List<string> dateLabels = readsPerDay.Select(x => x.Date.ToString("dd/MM", CultureInfo.InvariantCulture))
                .ToList();

            ReadsPerDaySeries.Add(new LineSeries<double>
            {
                Values = readCounts,
                Name = DailyReadsText,
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.Purple) { StrokeThickness = 3 },
                GeometryFill = new SolidColorPaint(SKColors.Purple),
                GeometryStroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 }
            });

            ReadsXAxes.Clear();
            ReadsXAxes.Add(new Axis
            {
                Labels = dateLabels,
                LabelsRotation = -45,
                TextSize = 12
            });
        }
    }
}