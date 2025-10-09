using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTO.Rank;
using ManaxLibrary.Notifications;
using ManaxClient.ViewModels;

namespace ManaxClient.Models;

public partial class Rank : LocalizedObject
{
    [ObservableProperty] private long _id;
    [ObservableProperty] private string _name = string.Empty;
    [ObservableProperty] private int _value;
    
    [ObservableProperty] private string _displayLevel = string.Empty;

    public Rank(RankDto dto)
    {
        ServerNotification.OnRankUpdated += OnRankUpdated;
        FromDto(dto);
        BindLocalizedStrings();
    }

    ~Rank()
    {
        ServerNotification.OnRankUpdated -= OnRankUpdated;
    }

    private void BindLocalizedStrings()
    {
        Localize(() => DisplayLevel, "RankPage.Level", () => Value);
    }

    private void FromDto(RankDto dto)
    {
        Id = dto.Id;
        Value = dto.Value;
        Name = dto.Name;
    }

    private void OnRankUpdated(RankDto dto)
    {
        if (Id != dto.Id) return;
        FromDto(dto);
    }
}