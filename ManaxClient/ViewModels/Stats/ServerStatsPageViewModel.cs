using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Stats;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Stats;

public partial class ServerStatsPageViewModel : PageViewModel
{
    [ObservableProperty] private ServerStats _serverStats = null!;
    
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
                Logger.LogFailure($"Failed to load server stats: {serverStats.Error}",Environment.StackTrace);
                InfoEmitted?.Invoke(this, $"Failed to load server stats: {serverStats.Error}");
                return;
            }

            Dispatcher.UIThread.Post(() =>
            {
                ServerStats = serverStats.GetValue();
            });
        }
        catch (Exception e)
        {
            Logger.LogError($"An error occurred while loading server stats: {e.Message}",e, Environment.StackTrace);
            InfoEmitted?.Invoke(this, $"An error occurred while loading server stats: {e.Message}");
        }
    }
}