using System;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Stats;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Stats;

public partial class UserStatsPageViewModel : PageViewModel
{
    [ObservableProperty] private UserStats _userStats = null!;
    
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
                Logger.LogFailure($"Failed to load user stats: {userStats.Error}",Environment.StackTrace);
                InfoEmitted?.Invoke(this, $"Failed to load user stats: {userStats.Error}");
                return;
            }

            Dispatcher.UIThread.Post(() =>
            {
                UserStats = userStats.GetValue();
            });
        }
        catch (Exception e)
        {
            Logger.LogError($"An error occurred while loading user stats: {e.Message}",e, Environment.StackTrace);
            InfoEmitted?.Invoke(this, $"An error occurred while loading user stats: {e.Message}");
        }
    }
}