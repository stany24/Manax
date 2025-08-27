using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
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

namespace ManaxClient.ViewModels.Stats;

public partial class UserStatsPageViewModel : PageViewModel
{
    [ObservableProperty] private UserStats? _userStats;

    public UserStatsPageViewModel()
    {
        Task.Run(LoadUserStats);
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
                Logger.LogFailure($"Failed to load user stats: {userStats.Error}", Environment.StackTrace);
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
            Logger.LogError($"An error occurred while loading user stats: {e.Message}", e, Environment.StackTrace);
            InfoEmitted?.Invoke(this, $"An error occurred while loading user stats: {e.Message}");
        }
    }

    private void UpdateChartData()
    {
        if (UserStats == null) return;
        SeriesSeries.Add(new PieSeries<long>
        {
            Values = [UserStats.SeriesCompleted],
            Name = "Terminées",
            Fill = new SolidColorPaint(SKColors.Green),
            DataLabelsPaint = new SolidColorPaint(SKColors.White),
            DataLabelsSize = 12,
            DataLabelsPosition = PolarLabelsPosition.Middle
        });
        SeriesSeries.Add(new PieSeries<long>
        {
            Values = [UserStats.SeriesInProgress],
            Name = "En cours",
            Fill = new SolidColorPaint(SKColors.Orange),
            DataLabelsPaint = new SolidColorPaint(SKColors.White),
            DataLabelsSize = 12,
            DataLabelsPosition = PolarLabelsPosition.Middle
        });
        SeriesSeries.Add(new PieSeries<long>
        {
            Values = [UserStats.SeriesRemaining],
            Name = "Restantes",
            Fill = new SolidColorPaint(SKColors.Gray),
            DataLabelsPaint = new SolidColorPaint(SKColors.White),
            DataLabelsSize = 12,
            DataLabelsPosition = PolarLabelsPosition.Middle
        });

        ChaptersSeries.Add(new PieSeries<long>
        {
            Values = [UserStats.ChaptersRead],
            Name = "Lus",
            Fill = new SolidColorPaint(SKColors.Green),
            DataLabelsPaint = new SolidColorPaint(SKColors.White),
            DataLabelsSize = 12,
            DataLabelsPosition = PolarLabelsPosition.Middle
        });
        ChaptersSeries.Add(new PieSeries<long>
        {
            Values = [UserStats.ChaptersRemaining],
            Name = "Restants",
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
                Name = "Nombre de séries",
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
                Name = "Lectures par jour",
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