using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Controls.Popups;
using ManaxClient.Models.Collections;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTOs.Rank;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Rank;

public partial class RankPageViewModel : PageViewModel
{
    [ObservableProperty] private SortedObservableCollection<RankDTO> _ranks;
    
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
            RankDTO? firstOrDefault = Ranks.FirstOrDefault(r => r.Id == obj);
            if (firstOrDefault != null)
            {
                Ranks.Remove(firstOrDefault);
            }
        });
    }

    private void OnRankCreated(RankDTO obj)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (Ranks.Any(r => r.Id == obj.Id)) return;
            Ranks.Add(obj);
        });
    }

    private void OnRankUpdated(RankDTO rank)
    {
        Dispatcher.UIThread.Post(() =>
        {
            RankDTO? firstOrDefault = Ranks.FirstOrDefault(r => r.Id == rank.Id);
            if(firstOrDefault != null){Ranks.Remove(firstOrDefault);}
            Ranks.Add(rank);
        });
    }
    
    private async void LoadRanks()
    {
        try
        {
            IEnumerable<RankDTO>? ranks = await ManaxApiRankClient.GetRanksAsync();
            if (ranks == null)
            {
                InfoEmitted?.Invoke(this,"Failed to load ranks from server");
                Logger.LogFailure("Failed to load ranks from server",Environment.StackTrace);
                return;
            }
            Dispatcher.UIThread.Post(() =>
            {
                Ranks = new SortedObservableCollection<RankDTO>(ranks)
                {
                    SortingSelector = r => r.Value,
                    Descending = true
                };
            });
        }
        catch (Exception e)
        {
            InfoEmitted?.Invoke(this,"Failed to load ranks from server");
            Logger.LogError("Failed to load ranks from server",e,Environment.StackTrace);
        }   
    }

    public void UpdateRank(RankDTO rank)
    {
        RankEditPopup rankEditPopup = new(rank);
        rankEditPopup.CloseRequested += async (_,_) =>
        {
            try
            {
                RankDTO result = rankEditPopup.GetResult();
                bool success = await ManaxApiRankClient.UpdateRankAsync(result);
                if (success){rankEditPopup.Close();}
                else
                {
                    InfoEmitted?.Invoke(this,"Failed to update rank on server");
                    Logger.LogFailure("Failed to update rank on server",Environment.StackTrace);
                }
                
            }
            catch (Exception e)
            {
                InfoEmitted?.Invoke(this,"Failed to update rank on server");
                Logger.LogError("Failed to update rank on server",e,Environment.StackTrace);
            }
        };
        PopupRequested?.Invoke(this, rankEditPopup);
    }

    public void DeleteRank(RankDTO rank)
    {
        Task.Run(async () =>
        {
            try
            {
                bool success = await ManaxApiRankClient.DeleteRankAsync(rank.Id);
                if (!success)
                {
                    InfoEmitted?.Invoke(this, "Failed to delete rank on server");
                    Logger.LogFailure("Failed to delete rank on server", Environment.StackTrace);
                }
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
        RankEditPopup rankCreatePopup = new(new RankDTO {Name = "New Rank", Value = 10});
        rankCreatePopup.CloseRequested += async (_, _) =>
        {
            try
            {
                RankDTO result = rankCreatePopup.GetResult();
                bool success = await ManaxApiRankClient.CreateRankAsync(new RankCreateDTO {Name = result.Name, Value = result.Value});
                if (success) { rankCreatePopup.Close(); }
                else
                {
                    InfoEmitted?.Invoke(this, "Failed to create rank on server");
                    Logger.LogFailure("Failed to create rank on server", Environment.StackTrace);
                }
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