using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using ManaxClient.Models.Sources;
using ManaxLibrary.DTO.Issue.Automatic;

namespace ManaxClient.Models.Issue;

public partial class IssueSerieAutomatic : ObservableObject
{
    private IDisposable _subscription = null!;
    [ObservableProperty] private DateTime _createdAt;
    [ObservableProperty] private Serie _serie = null!;
    [ObservableProperty] private IssueSerieAutomaticType _problem;

    public IssueSerieAutomatic(IssueSerieAutomaticDto dto)
    {
        FromDto(dto);
    }

    private void FromDto(IssueSerieAutomaticDto dto)
    {
        CreatedAt = dto.CreatedAt;
        Problem = dto.Problem;
        
        _subscription.Dispose();
        _subscription = SerieSource.Series
            .Connect()
            .AutoRefresh(o => o)
            .Filter(o => o.Id == dto.SerieId)
            .Subscribe(changes =>
            {
                using IEnumerator<Change<Serie, long>> enumerator = changes.GetEnumerator();
                Serie = enumerator.Current.Current;
            });
    }
}