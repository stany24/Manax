using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models.Collections;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.Models.Rank;

public partial class Rank:ObservableObject
{
    public static SortedObservableCollection<Rank> Ranks { get; } = new([]) { SortingSelector = r => r.Value,Descending = true };
    private static bool _loaded;
    private static readonly object LoadLock = new();
    
    [ObservableProperty] private long _id;
    [ObservableProperty] private int _value;
    [ObservableProperty] private string _name = string.Empty;

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
        ServerNotification.OnRankUpdated += OnRankUpdated;
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
    
    private static void OnRankDeleted(long obj)
    {
        Rank? firstOrDefault = Ranks.FirstOrDefault(r => r.Id == obj);
        if (firstOrDefault != null) Ranks.Remove(firstOrDefault);
    }

    private static void OnRankCreated(RankDto obj)
    {
        if (Ranks.Any(r => r.Id == obj.Id)) return;
        Ranks.Add(new Rank(obj));
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
                        Logger.LogFailure(ranksResponse.Error,Environment.StackTrace);
                        Ranks.Clear();
                    }

                    List<Rank> ranks = ranksResponse.GetValue().Select(dto => new Rank(dto)).ToList();
                    foreach (Rank rank in ranks)
                    {
                        Ranks.Add(rank);
                    }
                    _loaded = true;
                }
                catch (Exception e)
                {
                    const string error = "Failed to load ranks from server";
                    Logger.LogError(error,e,Environment.StackTrace);
                    Ranks.Clear();
                }
            }
        });
    }
}