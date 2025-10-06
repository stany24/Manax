using System;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTO.Issue.Automatic;

namespace ManaxClient.Models.Issue;

public partial class IssueChapterAutomatic : ObservableObject
{
    [ObservableProperty] private DateTime _createdAt;
    [ObservableProperty] private long _chapterId;
    [ObservableProperty] private IssueChapterAutomaticType _problem;

    public IssueChapterAutomatic(IssueChapterAutomaticDto dto)
    {
        FromDto(dto);
    }

    private void FromDto(IssueChapterAutomaticDto dto)
    {
        CreatedAt = dto.CreatedAt;
        ChapterId = dto.ChapterId;
        Problem = dto.Problem;
    }
}