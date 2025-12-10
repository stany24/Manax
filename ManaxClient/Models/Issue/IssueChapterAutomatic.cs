using System;
using System.Globalization;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using Jeek.Avalonia.Localization;
using ManaxClient.Models.Server.Data;
using ManaxClient.ViewModels;
using ManaxLibrary.DTO.Issue.Automatic;

namespace ManaxClient.Models.Issue;

public partial class IssueChapterAutomatic : ObservableObject
{
    private static CompositeFormat? _chapterInfoFormat;
    [ObservableProperty] private Chapter _chapter = null!;
    [ObservableProperty] private DateTime _createdAt;
    [ObservableProperty] private IssueChapterAutomaticType _problem;
    private IDisposable? _subscription;

    public IssueChapterAutomatic(IssueChapterAutomaticDto dto)
    {
        FromDto(dto);
    }

    public static string AutomaticBadgeText => Localizer.Get("IssuesPage.Automatic");

    public string FormattedInfo
    {
        get
        {
            _chapterInfoFormat ??= CompositeFormat.Parse(Localizer.Get("IssuesPage.ChapterInfo"));
            return string.Format(CultureInfo.InvariantCulture, _chapterInfoFormat, Chapter.Number, CreatedAt);
        }
    }

    private void FromDto(IssueChapterAutomaticDto dto)
    {
        CreatedAt = dto.CreatedAt;
        Problem = dto.Problem;
        _subscription?.Dispose();
        _subscription = MainWindowViewModel.Instance.ChapterSource.Chapters
            .Connect()
            .AutoRefresh()
            .Filter(o => o.Id == dto.ChapterId)
            .Subscribe(changes =>
            {
                foreach (Change<Chapter, long> change in changes)
                {
                    if (change.Reason is not (ChangeReason.Add or ChangeReason.Update)) continue;
                    Chapter = change.Current;
                    OnPropertyChanged(nameof(FormattedInfo));
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