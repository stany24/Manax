using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Controls.Popups.Rank;
using ManaxClient.Models.Collections;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.ViewModels.Rank;

public partial class RankPageViewModel : PageViewModel
{
    [ObservableProperty] private SortedObservableCollection<RankDto> _ranks = new([]);

    public RankPageViewModel()
    {
        Task.Run(LoadRanks);
        ServerNotification.OnRankUpdated += OnRankUpdated;
        ServerNotification.OnRankCreated += OnRankCreated;
        ServerNotification.OnRankDeleted += OnRankDeleted;
    }

    private void OnRankDeleted(long obj)
    {
        Dispatcher.UIThread.Post(() =>
        {
            RankDto? firstOrDefault = Ranks.FirstOrDefault(r => r.Id == obj);
            if (firstOrDefault != null) Ranks.Remove(firstOrDefault);
        });
    }

    private void OnRankCreated(RankDto obj)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (Ranks.Any(r => r.Id == obj.Id)) return;
            Ranks.Add(obj);
        });
    }

    private void OnRankUpdated(RankDto rank)
    {
        Dispatcher.UIThread.Post(() =>
        {
            RankDto? firstOrDefault = Ranks.FirstOrDefault(r => r.Id == rank.Id);
            if (firstOrDefault != null) Ranks.Remove(firstOrDefault);
            Ranks.Add(rank);
        });
    }

    private async void LoadRanks()
    {
        try
        {
            Optional<List<RankDto>> ranksResponse = await ManaxApiRankClient.GetRanksAsync();
            if (ranksResponse.Failed)
            {
                InfoEmitted?.Invoke(this, ranksResponse.Error);
                return;
            }

            Dispatcher.UIThread.Post(() =>
            {
                Ranks = new SortedObservableCollection<RankDto>(ranksResponse.GetValue())
                {
                    SortingSelector = r => r.Value,
                    Descending = true
                };
            });
        }
        catch (Exception e)
        {
            InfoEmitted?.Invoke(this, "Failed to load ranks from server");
            Logger.LogError("Failed to load ranks from server", e, Environment.StackTrace);
        }
    }

    public void UpdateRank(RankDto rank)
    {
        RankEditPopup rankEditPopup = new(rank);
        rankEditPopup.CloseRequested += async void (_, _) =>
        {
            try
            {
                if (rankEditPopup.Canceled) { return; }
                RankDto result = rankEditPopup.GetResult();
                Optional<bool> updateRankAsync = await ManaxApiRankClient.UpdateRankAsync(result);
                if (updateRankAsync.Failed)
                    InfoEmitted?.Invoke(this, updateRankAsync.Error);
                else
                    rankEditPopup.Close();
            }
            catch (Exception e)
            {
                InfoEmitted?.Invoke(this, "Failed to update rank on server");
                Logger.LogError("Failed to update rank on server", e, Environment.StackTrace);
            }
        };
        PopupRequested?.Invoke(this, rankEditPopup);
    }

    public void DeleteRank(RankDto rank)
    {
        Task.Run(async () =>
        {
            try
            {
                Optional<bool> deleteRankResponse = await ManaxApiRankClient.DeleteRankAsync(rank.Id);
                if (deleteRankResponse.Failed) InfoEmitted?.Invoke(this, deleteRankResponse.Error);
            }
            catch (Exception e)
            {
                InfoEmitted?.Invoke(this, "Failed to delete rank on server");
                Logger.LogError("Failed to delete rank on server", e, Environment.StackTrace);
            }
        });
    }

    public void CreateRank()
    {
        RankEditPopup rankCreatePopup = new(new RankDto { Name = "New Rank", Value = 10 });
        rankCreatePopup.CloseRequested += async void (_, _) =>
        {
            try
            {
                if (rankCreatePopup.Canceled) { return; }
                RankDto result = rankCreatePopup.GetResult();
                Optional<bool> rankResponse = await ManaxApiRankClient.CreateRankAsync(new RankCreateDto
                    { Name = result.Name, Value = result.Value });

                if (rankResponse.Failed)
                    InfoEmitted?.Invoke(this, rankResponse.Error);
                else
                    rankCreatePopup.Close();
            }
            catch (Exception e)
            {
                InfoEmitted?.Invoke(this, "Failed to create rank on server");
                Logger.LogError("Failed to create rank on server", e, Environment.StackTrace);
            }
        };
        PopupRequested?.Invoke(this, rankCreatePopup);
    }
}