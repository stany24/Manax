using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using ManaxClient.Models.Sources;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Issue.Reported;

namespace ManaxClient.Models.Issue;

public partial class IssueChapterReported : ObservableObject
{
    private IDisposable _subscriptionChapter = null!;
    private IDisposable _subscriptionProblem = null!;
    private IDisposable _subscriptionUser = null!;
    [ObservableProperty] private long _id;
    [ObservableProperty] private DateTime _createdAt;
    [ObservableProperty] private User _user = null!;
    [ObservableProperty] private Chapter _chapter = null!;
    [ObservableProperty] private IssueChapterReportedType _problem = null!;

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
        
        _subscriptionChapter.Dispose();
        _subscriptionChapter = ChapterSource.Chapters
            .Connect()
            .AutoRefresh(o => o)
            .Filter(o => o.Id == dto.ChapterId)
            .Subscribe(changes =>
            {
                using IEnumerator<Change<Chapter, long>> enumerator = changes.GetEnumerator();
                Chapter = enumerator.Current.Current;
            });
        
        _subscriptionUser.Dispose();
        _subscriptionUser = UserSource.Users
            .Connect()
            .AutoRefresh(o => o)
            .Filter(o => o.Id == dto.UserId)
            .Subscribe(changes =>
            {
                using IEnumerator<Change<User, long>> enumerator = changes.GetEnumerator();
                User = enumerator.Current.Current;
            });
        
        _subscriptionProblem.Dispose();
        _subscriptionProblem = ProblemSource.ChapterProblems
            .Connect()
            .AutoRefresh(o => o)
            .Filter(o => o.Id == dto.ProblemId)
            .Subscribe(changes =>
            {
                using IEnumerator<Change<IssueChapterReportedType, long>> enumerator = changes.GetEnumerator();
                Problem = enumerator.Current.Current;
            });
    }
}