using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Painting;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Stats;
using ManaxLibrary.Logging;
using SkiaSharp;

namespace ManaxClient.ViewModels.Stats;

public partial class UserStatsPageViewModel : PageViewModel
{
    [ObservableProperty] private IEnumerable<ISeries>? _chaptersSeries;
    [ObservableProperty] private IEnumerable<ISeries>? _ranksSeries;
    [ObservableProperty] private IEnumerable<ISeries>? _seriesSeries;
    [ObservableProperty] private UserStats? _userStats;
    [ObservableProperty] private List<Axis>? _xAxes;

    public UserStatsPageViewModel()
    {
        Task.Run(LoadUserStats);
    }

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
        if(UserStats == null){return;}
        SeriesSeries = GaugeGenerator.BuildSolidGauge(
            new GaugeItem(
                UserStats.SeriesRead,
                series =>
                {
                    series.MaxRadialColumnWidth = 50;
                    series.DataLabelsSize = 50;
                    series.Name = $"Séries lues: {UserStats.SeriesRead} / {UserStats.SeriesTotal}";
                }));

        ChaptersSeries = GaugeGenerator.BuildSolidGauge(
            new GaugeItem(
                UserStats.ChaptersRead,
                series =>
                {
                    series.MaxRadialColumnWidth = 50;
                    series.DataLabelsSize = 50;
                    series.Name = $"Chapitres lus: {UserStats.ChaptersRead} / {UserStats.ChaptersTotal}";
                }));

        if (UserStats.Ranks.Count == 0) return;

        List<RankCount> sortedRanks = UserStats.Ranks.OrderBy(r => r.Rank.Value).ToList();
        List<double> values = [];
        List<string> labels = [];

        foreach (RankCount rankCount in sortedRanks)
        {
            values.Add(rankCount.Count);
            labels.Add(rankCount.Rank.Name);
        }

        RanksSeries =
        [
            new ColumnSeries<double>
            {
                Values = values,
                Name = "Nombre de séries",
                Fill = new SolidColorPaint(SKColors.DodgerBlue)
            }
        ];

        XAxes =
        [
            new Axis
            {
                Labels = labels,
                LabelsRotation = -15,
                TextSize = 14
            }
        ];
    }
}