using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.DTO.User;

namespace ManaxClient.Models.Issue;

public class ClientReportedIssueChapter : ObservableObject
{
    private ChapterDto? _chapter;
    private ReportedIssueChapterDto _issue;

    private ReportedIssueChapterTypeDto? _problem;

    private UserDto? _user;

    public ClientReportedIssueChapter(ReportedIssueChapterDto issue)
    {
        _issue = issue;
        Task.Run(async () =>
        {
            Optional<ChapterDto> chapterInfo = await ManaxApiChapterClient.GetChapterAsync(issue.ChapterId);
            if (!chapterInfo.Failed) Chapter = chapterInfo.GetValue();
            Optional<UserDto> user = await ManaxApiUserClient.GetUserAsync(issue.UserId);
            if (!user.Failed) User = user.GetValue();
            Optional<List<ReportedIssueChapterTypeDto>> problem =
                await ManaxApiIssueClient.GetAllReportedChapterIssueTypesAsync();
            if (!problem.Failed) Problem = problem.GetValue().Find(p => p.Id == issue.Id);
        });
    }

    public ReportedIssueChapterDto Issue
    {
        get => _issue;
        set
        {
            _issue = value;
            OnPropertyChanged();
        }
    }

    public ReportedIssueChapterTypeDto? Problem
    {
        get => _problem;
        set
        {
            _problem = value;
            OnPropertyChanged();
        }
    }

    public UserDto? User
    {
        get => _user;
        set
        {
            _user = value;
            OnPropertyChanged();
        }
    }

    public ChapterDto? Chapter
    {
        get => _chapter;
        set
        {
            _chapter = value;
            OnPropertyChanged();
        }
    }

    public void Close()
    {
        Task.Run(async () => { await ManaxApiIssueClient.CloseChapterIssueAsync(Issue.Id); });
    }
}