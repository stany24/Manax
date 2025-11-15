using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamicData;
using ManaxClient.ViewModels;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Feature;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Sources;

public static class RankSource
{
    public static readonly SourceCache<Rank, long> Ranks = new(x => x.Id);
    private static bool _loaded;
    private static readonly Lock LoadLock = new();
    private static readonly Lock RanksLock = new();

    static RankSource()
    {
        MainWindowViewModel.FeatureChanged += (_, features) =>
        {
            if (features is { Key: FeatureType.Ranks, Value: true })
                LoadRanks();
            else
                Ranks.Clear();
        };
        ServerNotification.OnRankCreated += OnRankCreated;
        ServerNotification.OnRankDeleted += OnRankDeleted;
    }

    public static EventHandler<string>? ErrorEmitted { get; set; }

    private static void OnRankDeleted(long id)
    {
        lock (RanksLock)
        {
            Ranks.RemoveKey(id);
        }
    }

    private static void OnRankCreated(RankDto dto)
    {
        lock (RanksLock)
        {
            Ranks.AddOrUpdate(new Rank(dto));
        }
    }

    public static void LoadRanks()
    {
        Task.Run(() =>
        {
            lock (LoadLock)
            {
                if (_loaded) return;
                try
                {
                    Optional<List<RankDto>> ranksResponse = ManaxApiRankClient.GetRanksAsync().Result;
                    if (ranksResponse.Failed)
                    {
                        Logger.LogFailure(ranksResponse.Error);
                        ErrorEmitted?.Invoke(null, ranksResponse.Error);
                        return;
                    }

                    lock (RanksLock)
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
                    ErrorEmitted?.Invoke(null, error);
                }
            }
        });
    }
}