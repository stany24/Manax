using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Issue.Reported;

namespace ManaxClient.Models.Issue;

public partial class IssueSerieReported : ObservableObject
{
    [ObservableProperty] private long _id;
    [ObservableProperty] private DateTime _createdAt;
    [ObservableProperty] private long _userId;
    [ObservableProperty] private long _serieId;
    [ObservableProperty] private long _problemId;

    public IssueSerieReported(IssueSerieReportedDto dto)
    {
        FromDto(dto);
    }

    public void Close()
    {
        Task.Run(async () => { await ManaxApiIssueClient.CloseSerieIssueAsync(Id); });
    }

    private void FromDto(IssueSerieReportedDto dto)
    {
        Id = dto.Id;
        CreatedAt = dto.CreatedAt;
        UserId = dto.UserId;
        SerieId = dto.SerieId;
        ProblemId = dto.ProblemId;
    }
}