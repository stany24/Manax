using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models.Collections;
using ManaxClient.ViewModels.Popup.ConfirmCancel;
using ManaxClient.ViewModels.Popup.ConfirmCancel.Content;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.ViewModels.Pages.Rank;

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
            RankDto? firstOrDefault = Ranks.FirstOrDefault<RankDto>(r => r.Id == obj);
            if (firstOrDefault != null) Ranks.Remove(firstOrDefault);
        });
    }

    private void OnRankCreated(RankDto obj)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (Ranks.Any<RankDto>(r => r.Id == obj.Id)) return;
            Ranks.Add(obj);
        });
    }

    private void OnRankUpdated(RankDto rank)
    {
        Dispatcher.UIThread.Post(() =>
        {
            RankDto? firstOrDefault = Ranks.FirstOrDefault<RankDto>(r => r.Id == rank.Id);
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
        RankEditViewModel content = new(rank);
        ConfirmCancelViewModel viewModel = new(content);
        Controls.Popups.Popup popup = new(viewModel);
        popup.Closed += async (_, _) =>
        {
            try
            {
                if (viewModel.Canceled())
                {
                    return;
                }

                RankDto result = content.GetResult();
                Optional<bool> updateRankAsync = await ManaxApiRankClient.UpdateRankAsync(result);
                if (updateRankAsync.Failed)
                    InfoEmitted?.Invoke(this, updateRankAsync.Error);
            }
            catch (Exception e)
            {
                InfoEmitted?.Invoke(this, "Failed to update rank on server");
                Logger.LogError("Failed to update rank on server", e, Environment.StackTrace);
            }
        };
        PopupRequested?.Invoke(this, popup);
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
        RankEditViewModel content = new(new RankDto { Name = "New Rank", Value = 10 });
        ConfirmCancelViewModel viewModel = new(content);
        Controls.Popups.Popup popup = new(viewModel);
        popup.Closed += async (_, _) =>
        {
            try
            {
                if (viewModel.Canceled()) return;
                RankDto result = content.GetResult();
                Optional<bool> rankResponse = await ManaxApiRankClient.CreateRankAsync(new RankCreateDto
                    { Name = result.Name, Value = result.Value });

                if (rankResponse.Failed)
                    InfoEmitted?.Invoke(this, rankResponse.Error);
            }
            catch (Exception e)
            {
                InfoEmitted?.Invoke(this, "Failed to create rank on server");
                Logger.LogError("Failed to create rank on server", e, Environment.StackTrace);
            }
        };
        PopupRequested?.Invoke(this, popup);
    }
}