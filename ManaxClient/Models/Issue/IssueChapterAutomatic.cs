using System;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using ManaxClient.Models.Sources;
using ManaxLibrary.DTO.Issue.Automatic;
using Jeek.Avalonia.Localization;

namespace ManaxClient.Models.Issue;

public partial class IssueChapterAutomatic : ObservableObject
{
    [ObservableProperty] private Chapter _chapter = null!;
    [ObservableProperty] private DateTime _createdAt;
    [ObservableProperty] private IssueChapterAutomaticType _problem;
    private IDisposable? _subscription;

    public static string AutomaticBadgeText => Localizer.Get("IssuesPage.Automatic");
    public string FormattedInfo => string.Format(Localizer.Get("IssuesPage.ChapterInfo"), Chapter?.FileName ?? "", CreatedAt);

    public IssueChapterAutomatic(IssueChapterAutomaticDto dto)
    {
        FromDto(dto);
    }

    private void FromDto(IssueChapterAutomaticDto dto)
    {
        CreatedAt = dto.CreatedAt;
        Problem = dto.Problem;
        _subscription?.Dispose();
        _subscription = ChapterSource.Chapters
            .Connect()
            .AutoRefresh()
            .Filter(o => o.Id == dto.ChapterId)
            .Subscribe(changes =>
            {
                foreach (Change<Chapter, long> change in changes)
                {
                    if (change.Reason is not (ChangeReason.Add or ChangeReason.Update)) continue;
                    Chapter = change.Current;
                    OnPropertyChanged(nameof(FormattedInfo)); // Notifier le changement
                }
            });
    }

    partial void OnChapterChanged(Chapter value)
    {
        OnPropertyChanged(nameof(FormattedInfo));
    }

    partial void OnCreatedAtChanged(DateTime value)
    {
        OnPropertyChanged(nameof(FormattedInfo));
    }
}