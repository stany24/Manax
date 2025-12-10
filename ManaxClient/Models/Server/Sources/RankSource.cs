using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using ManaxClient.Event;
using ManaxClient.Manager;
using ManaxClient.Models.Server.Data;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Feature;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Server.Sources;

public class RankSource
{
    public readonly SourceCache<Rank, long> Ranks = new(x => x.Id);
    private bool _loaded;
    private readonly Lock _loadLock = new();
    private readonly Lock _ranksLock = new();

    public RankSource()
    {
        FeatureManager.FeatureChanged += (_, features) =>
        {
            if (features is { Key: FeatureType.Ranks, Value: true })
                LoadRanks();
            else
                Ranks.Clear();
        };
        NotificationReceiver.OnRankCreated += OnRankCreated;
        NotificationReceiver.OnRankDeleted += OnRankDeleted;
    }

    private void OnRankDeleted(long id)
    {
        lock (_ranksLock)
        {
            Ranks.RemoveKey(id);
        }
    }

    private void OnRankCreated(RankDto dto)
    {
        lock (_ranksLock)
        {
            Ranks.AddOrUpdate(new Rank(dto));
        }
    }

    private void LoadRanks()
    {
        Task.Run(() =>
        {
            lock (_loadLock)
            {
                if (_loaded) return;
                try
                {
                    Optional<List<RankDto>> ranksResponse = ManaxApiRankClient.GetRanksAsync().Result;
                    if (ranksResponse.Failed)
                    {
                        Logger.LogFailure(ranksResponse.Error);
                        WeakReferenceMessenger.Default.Send(new NotificationMessage(ranksResponse.Error));
                        return;
                    }

                    lock (_ranksLock)
                    {
                        Ranks.Edit(updater =>
                        {
                            updater.Clear();
                            List<Rank> ranks = ranksResponse.GetValue().Select(dto => new Rank(dto)).ToList();
                            updater.AddOrUpdate(ranks);
                        });
                    }

                    _loaded = true;
                }
                catch (Exception e)
                {
                    const string error = "Failed to load ranks from server";
                    Logger.LogError(error, e);
                    WeakReferenceMessenger.Default.Send(new NotificationMessage(error));
                }
            }
        });
    }
}