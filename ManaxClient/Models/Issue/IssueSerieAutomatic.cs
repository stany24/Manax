using System;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using Jeek.Avalonia.Localization;
using ManaxClient.Models.Sources;
using ManaxLibrary.DTO.Issue.Automatic;

namespace ManaxClient.Models.Issue;

public partial class IssueSerieAutomatic : ObservableObject
{
    [ObservableProperty] private Serie _serie = null!;
    [ObservableProperty] private DateTime _createdAt;
    [ObservableProperty] private IssueSerieAutomaticType _problem;
    private IDisposable? _subscription;

    public static string AutomaticBadgeText => Localizer.Get("IssuesPage.Automatic");
    public string FormattedInfo => string.Format(Localizer.Get("IssuesPage.SeriesInfo"), Serie?.Title ?? "", CreatedAt);

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
                foreach (Change<Serie, long> change in changes)
                {
                    if (change.Reason is not (ChangeReason.Add or ChangeReason.Update)) continue;
                    Serie = change.Current;
                    OnPropertyChanged(nameof(FormattedInfo));
                }
            });
    }

    partial void OnSerieChanged(Serie value)
    {
        OnPropertyChanged(nameof(FormattedInfo));
    }

    partial void OnCreatedAtChanged(DateTime value)
    {
        OnPropertyChanged(nameof(FormattedInfo));
    }
}