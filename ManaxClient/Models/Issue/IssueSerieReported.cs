using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using ManaxClient.Models.Sources;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Issue.Reported;

namespace ManaxClient.Models.Issue;

public partial class IssueSerieReported : ObservableObject
{
    [ObservableProperty] private DateTime _createdAt;
    [ObservableProperty] private long _id;
    [ObservableProperty] private IssueChapterReportedType _problem = null!;
    [ObservableProperty] private Serie _serie = null!;
    private IDisposable? _subscriptionProblem;
    private IDisposable? _subscriptionSerie;
    private IDisposable? _subscriptionUser;
    [ObservableProperty] private User _user = null!;

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

        _subscriptionSerie?.Dispose();
        _subscriptionSerie = SerieSource.Series
            .Connect()
            .AutoRefresh(o => o)
            .Filter(o => o.Id == dto.SerieId)
            .Subscribe(changes =>
            {
                using IEnumerator<Change<Serie, long>> enumerator = changes.GetEnumerator();
                if (enumerator.MoveNext()) Serie = enumerator.Current.Current;
            });

        _subscriptionUser?.Dispose();
        _subscriptionUser = UserSource.Users
            .Connect()
            .AutoRefresh(o => o)
            .Filter(o => o.Id == dto.UserId)
            .Subscribe(changes =>
            {
                using IEnumerator<Change<User, long>> enumerator = changes.GetEnumerator();
                if (enumerator.MoveNext()) User = enumerator.Current.Current;
            });

        _subscriptionProblem?.Dispose();
        _subscriptionProblem = ProblemSource.ChapterProblems
            .Connect()
            .AutoRefresh(o => o)
            .Filter(o => o.Id == dto.ProblemId)
            .Subscribe(changes =>
            {
                using IEnumerator<Change<IssueChapterReportedType, long>> enumerator = changes.GetEnumerator();
                if (enumerator.MoveNext()) Problem = enumerator.Current.Current;
            });
    }
}