using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using ManaxClient.Models.Sources;
using ManaxClient.ViewModels.Popup.ConfirmCancel;
using ManaxClient.ViewModels.Popup.ConfirmCancel.Content;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Pages.Rank;

public class RankPageViewModel : PageViewModel
{
    private readonly ReadOnlyObservableCollection<Models.Rank> _ranks;

    public RankPageViewModel()
    {
        SortExpressionComparer<Models.Rank> comparer = SortExpressionComparer<Models.Rank>.Descending(t => t.Value);
        RankSource.Ranks.Connect()
            .SortAndBind(out _ranks, comparer)
            .Subscribe();
    }

    public ReadOnlyObservableCollection<Models.Rank> Ranks => _ranks;

    public void UpdateRank(Models.Rank rank)
    {
        RankUpdateDto update = new()
        {
            Id = rank.Id,
            Name = rank.Name,
            Value = rank.Value
        };
        RankEditViewModel content = new(update);
        ConfirmCancelViewModel viewModel = new(content);
        Controls.Popups.Popup popup = new(viewModel);
        popup.Closed += async void (_, _) =>
        {
            try
            {
                if (viewModel.Canceled()) return;
                RankUpdateDto result = content.GetResult();
                Optional<bool> updateRankAsync = await ManaxApiRankClient.UpdateRankAsync(result);
                if (updateRankAsync.Failed)
                    InfoEmitted?.Invoke(this, updateRankAsync.Error);
            }
            catch (Exception e)
            {
                InfoEmitted?.Invoke(this, "Failed to update rank on server");
                Logger.LogError("Failed to update rank on server", e);
            }
        };
        PopupRequested?.Invoke(this, popup);
    }

    public void DeleteRank(Models.Rank rank)
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
                Logger.LogError("Failed to delete rank on server", e);
            }
        });
    }

    public void CreateRank()
    {
        RankEditViewModel content = new(new RankUpdateDto { Name = "New Rank", Value = 10 });
        ConfirmCancelViewModel viewModel = new(content);
        Controls.Popups.Popup popup = new(viewModel);
        popup.Closed += async void (_, _) =>
        {
            try
            {
                if (viewModel.Canceled()) return;
                RankUpdateDto result = content.GetResult();
                Optional<bool> rankResponse = await ManaxApiRankClient.CreateRankAsync(new RankCreateDto
                    { Name = result.Name, Value = result.Value });

                if (rankResponse.Failed)
                    InfoEmitted?.Invoke(this, rankResponse.Error);
            }
            catch (Exception e)
            {
                InfoEmitted?.Invoke(this, "Failed to create rank on server");
                Logger.LogError("Failed to create rank on server", e);
            }
        };
        PopupRequested?.Invoke(this, popup);
    }
}