using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using ManaxClient.Models.Sources;
using ManaxLibrary.DTO.Issue.Automatic;

namespace ManaxClient.Models.Issue;

public partial class IssueSerieAutomatic : ObservableObject
{
    [ObservableProperty] private DateTime _createdAt;
    [ObservableProperty] private IssueSerieAutomaticType _problem;
    [ObservableProperty] private Serie _serie = null!;
    private IDisposable? _subscription;

    public IssueSerieAutomatic(IssueSerieAutomaticDto dto)
    {
        FromDto(dto);
    }

    private void FromDto(IssueSerieAutomaticDto dto)
    {
        CreatedAt = dto.CreatedAt;
        Problem = dto.Problem;

        _subscription?.Dispose();
        _subscription = SerieSource.Series
            .Connect()
            .AutoRefresh()
            .Filter(o => o.Id == dto.SerieId)
            .Subscribe(changes =>
            {
                using IEnumerator<Change<Serie, long>> enumerator = changes.GetEnumerator();
                if (enumerator.MoveNext()) Serie = enumerator.Current.Current;
            });
    }
}