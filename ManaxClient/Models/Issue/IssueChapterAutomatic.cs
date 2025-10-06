using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using ManaxClient.Models.Sources;
using ManaxLibrary.DTO.Issue.Automatic;

namespace ManaxClient.Models.Issue;

public partial class IssueChapterAutomatic : ObservableObject
{
    private IDisposable _subscription = null!;
    [ObservableProperty] private DateTime _createdAt;
    [ObservableProperty] private Chapter _chapter = null!;
    [ObservableProperty] private IssueChapterAutomaticType _problem;

    public IssueChapterAutomatic(IssueChapterAutomaticDto dto)
    {
        FromDto(dto);
    }

    private void FromDto(IssueChapterAutomaticDto dto)
    {
        CreatedAt = dto.CreatedAt;
        Problem = dto.Problem;
        _subscription.Dispose();
        _subscription = ChapterSource.Chapters
            .Connect()
            .AutoRefresh(o => o)
            .Filter(o => o.Id == dto.ChapterId)
            .Subscribe(changes =>
            {
                using IEnumerator<Change<Chapter, long>> enumerator = changes.GetEnumerator();
                Chapter = enumerator.Current.Current;
            });
    }
}