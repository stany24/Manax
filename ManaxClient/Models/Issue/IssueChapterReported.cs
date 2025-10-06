using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Issue.Reported;

namespace ManaxClient.Models.Issue;

public partial class IssueChapterReported : ObservableObject
{
    [ObservableProperty] private long _id;
    [ObservableProperty] private DateTime _createdAt;
    [ObservableProperty] private long _userId;
    [ObservableProperty] private long _chapterId;
    [ObservableProperty] private long _problemId;

    public IssueChapterReported(IssueChapterReportedDto dto)
    {
        FromDto(dto);
    }
    
    public void Close()
    {
        Task.Run(async () => { await ManaxApiIssueClient.CloseChapterIssueAsync(Id); });
    }

    private void FromDto(IssueChapterReportedDto dto)
    {
        Id = dto.Id;
        CreatedAt = dto.CreatedAt;
        UserId = dto.UserId;
        ChapterId = dto.ChapterId;
        ProblemId = dto.ProblemId;
    }
}