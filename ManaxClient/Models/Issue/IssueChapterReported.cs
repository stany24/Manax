using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using DynamicData;
using ManaxClient.Event;
using ManaxClient.Models.Server.Data;
using ManaxClient.ViewModels;
using ManaxLibrary;
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
        Task.Run(async () =>
        {
            Optional<bool> response = await ManaxApiIssueClient.CloseChapterIssueAsync(Id);
            if (response.Failed) WeakReferenceMessenger.Default.Send(new NotificationMessage(response.Error));
        });
    }

    private void FromDto(IssueChapterReportedDto dto)
    {
        Id = dto.Id;
        CreatedAt = dto.CreatedAt;

        _subscriptionChapter?.Dispose();
        _subscriptionChapter = MainWindowViewModel.Instance.ChapterSource.Chapters
            .Connect()
            .AutoRefresh()
            .Filter(o => o.Id == dto.ChapterId)
            .Subscribe(changes =>
            {
                using IEnumerator<Change<Chapter, long>> enumerator = changes.GetEnumerator();
                if (enumerator.MoveNext()) Chapter = enumerator.Current.Current;
            });

        _subscriptionUser?.Dispose();
        _subscriptionUser = MainWindowViewModel.Instance.UserSource.Users
            .Connect()
            .AutoRefresh()
            .Filter(o => o.Id == dto.UserId)
            .Subscribe(changes =>
            {
                using IEnumerator<Change<User, long>> enumerator = changes.GetEnumerator();
                if (enumerator.MoveNext()) User = enumerator.Current.Current;
            });

        _subscriptionProblem?.Dispose();
        _subscriptionProblem = MainWindowViewModel.Instance.ProblemSource.ChapterProblems
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