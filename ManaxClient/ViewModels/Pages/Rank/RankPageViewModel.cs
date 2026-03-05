using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using DynamicData.Binding;
using ManaxClient.Event;
using ManaxClient.ViewModels.Popup.ConfirmCancel;
using ManaxClient.ViewModels.Popup.ConfirmCancel.Content;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Pages.Rank;

public class RankPageViewModel : PageViewModel
{
    private readonly ReadOnlyObservableCollection<Models.Server.Data.Rank> _ranks;

    public RankPageViewModel()
    {
        SortExpressionComparer<Models.Server.Data.Rank> comparer =
            SortExpressionComparer<Models.Server.Data.Rank>.Descending(t => t.Value);
        MainWindowViewModel.Instance.RankSource.Ranks.Connect()
            .SortAndBind(out _ranks, comparer)
            .Subscribe();
    }

    public ReadOnlyObservableCollection<Models.Server.Data.Rank> Ranks => _ranks;

    public void UpdateRank(Models.Server.Data.Rank rank)
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
                    WeakReferenceMessenger.Default.Send(new NotificationMessage(new Notification(updateRankAsync.Error)));
            }
            catch (Exception e)
            {
                WeakReferenceMessenger.Default.Send(new NotificationMessage(new Notification("RankPage.Update.Failed")));
                Logger.LogError("Failed to update rank on server", e);
            }
        };
        WeakReferenceMessenger.Default.Send(new PopupChangeMessage(popup));
    }

    public void DeleteRank(Models.Server.Data.Rank rank)
    {
        Task.Run(async () =>
        {
            try
            {
                Optional<bool> deleteRankResponse = await ManaxApiRankClient.DeleteRankAsync(rank.Id);
                if (deleteRankResponse.Failed)
                    WeakReferenceMessenger.Default.Send(new NotificationMessage(new Notification(deleteRankResponse.Error)));
            }
            catch (Exception e)
            {
                WeakReferenceMessenger.Default.Send(new NotificationMessage(new Notification("RankPage.Delete.Failed")));
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
                    WeakReferenceMessenger.Default.Send(new NotificationMessage(new Notification(rankResponse.Error)));
            }
            catch (Exception e)
            {
                WeakReferenceMessenger.Default.Send(new NotificationMessage(new Notification("RankPage.Create.Failed")));
                Logger.LogError("Failed to create rank on server", e);
            }
        };
        WeakReferenceMessenger.Default.Send(new PopupChangeMessage(popup));
    }
}