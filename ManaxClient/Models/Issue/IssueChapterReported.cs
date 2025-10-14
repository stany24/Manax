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
    [ObservableProperty] private Chapter _chapter = null!;
    [ObservableProperty] private DateTime _createdAt;
    [ObservableProperty] private long _id;
    [ObservableProperty] private IssueChapterReportedType _problem = null!;
    private IDisposable? _subscriptionChapter;
    private IDisposable? _subscriptionProblem;
    private IDisposable? _subscriptionUser;
    [ObservableProperty] private User _user = null!;

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

        _subscriptionChapter?.Dispose();
        _subscriptionChapter = ChapterSource.Chapters
            .Connect()
            .AutoRefresh()
            .Filter(o => o.Id == dto.ChapterId)
            .Subscribe(changes =>
            {
                using IEnumerator<Change<Chapter, long>> enumerator = changes.GetEnumerator();
                if (enumerator.MoveNext()) Chapter = enumerator.Current.Current;
            });

        _subscriptionUser?.Dispose();
        _subscriptionUser = UserSource.Users
            .Connect()
            .AutoRefresh()
            .Filter(o => o.Id == dto.UserId)
            .Subscribe(changes =>
            {
                using IEnumerator<Change<User, long>> enumerator = changes.GetEnumerator();
                if (enumerator.MoveNext()) User = enumerator.Current.Current;
            });

        _subscriptionProblem?.Dispose();
        _subscriptionProblem = ProblemSource.ChapterProblems
            .Connect()
            .AutoRefresh()
            .Filter(o => o.Id == dto.ProblemId)
            .Subscribe(changes =>
            {
                using IEnumerator<Change<IssueChapterReportedType, long>> enumerator = changes.GetEnumerator();
                if (enumerator.MoveNext()) Problem = enumerator.Current.Current;
            });
    }
}