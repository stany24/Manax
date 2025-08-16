using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.Extensions;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Stats;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Stats;

public partial class ServerStatsPageViewModel : PageViewModel
{
    [ObservableProperty] private ServerStats? _serverStats;
    [ObservableProperty] private IEnumerable<ISeries>? _userSeries;

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

            Dispatcher.UIThread.Post(() =>
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
        UserSeries = GaugeGenerator.BuildSolidGauge(
            new GaugeItem(
                ServerStats.ActiveUsers,
                series =>
                {
                    series.MaxRadialColumnWidth = 50;
                    series.DataLabelsSize = 50;
                    series.Name = $"Utilisateurs actifs: {ServerStats.ActiveUsers} / {ServerStats.Users}";
                }));
    }
}