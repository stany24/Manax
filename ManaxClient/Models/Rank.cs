using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models;

public partial class Rank:ObservableObject
{
    public static readonly SourceCache<Rank, long> Ranks = new (x => x.Id);
    private static bool _loaded;
    private static readonly object LoadLock = new();
    private static readonly object RanksLock = new ();
    
    [ObservableProperty] private long _id;
    [ObservableProperty] private int _value;
    [ObservableProperty] private string _name = string.Empty;
    
    public static EventHandler<string>? ErrorEmitted { get; set; }

    static Rank()
    {
        ServerNotification.OnRankCreated += OnRankCreated;
        ServerNotification.OnRankDeleted += OnRankDeleted;
    }
    
    public Rank(RankDto dto)
    {
        ServerNotification.OnRankUpdated += OnRankUpdated;
        FromDto(dto);
    }

    ~Rank()
    {
        ServerNotification.OnRankUpdated -= OnRankUpdated;
    }
    
    private void FromDto(RankDto dto)
    {
        Id = dto.Id;
        Value = dto.Value;
        Name = dto.Name;
    }
    
    private void OnRankUpdated(RankDto dto)
    {
        if(Id != dto.Id) return;
        FromDto(dto);
    }
    
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
                        ErrorEmitted?.Invoke(null,ranksResponse.Error);
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
                    Logger.LogError(error,e);
                    ErrorEmitted?.Invoke(null,error);
                }
            }
        });
    }
}