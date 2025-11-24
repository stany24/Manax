using System;
using System.Globalization;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using Jeek.Avalonia.Localization;
using ManaxClient.Models.Server.Data;
using ManaxClient.Models.Server.Sources;
using ManaxLibrary.DTO.Issue.Automatic;

namespace ManaxClient.Models.Issue;

public partial class IssueSerieAutomatic : ObservableObject
{
    private static CompositeFormat? _serieInfoFormat;
    [ObservableProperty] private DateTime _createdAt;
    [ObservableProperty] private IssueSerieAutomaticType _problem;
    [ObservableProperty] private Serie _serie = null!;
    private IDisposable? _subscription;

    public IssueSerieAutomatic(IssueSerieAutomaticDto dto)
    {
        FromDto(dto);
    }

    public static string AutomaticBadgeText => Localizer.Get("IssuesPage.Automatic");

    public string FormattedInfo
    {
        get
        {
            _serieInfoFormat ??= CompositeFormat.Parse(Localizer.Get("IssuesPage.SeriesInfo"));
            return string.Format(CultureInfo.InvariantCulture, _serieInfoFormat, Serie.Title, CreatedAt);
        }
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