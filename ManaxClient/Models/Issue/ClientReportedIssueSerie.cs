using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.DTO.User;

namespace ManaxClient.Models.Issue;

public class ClientReportedIssueSerie : ObservableObject
{
    private ReportedIssueSerieDto _issue;

    private ReportedIssueSerieTypeDto? _problem;
    private SerieDto? _serie;

    private User? _user;

    public ClientReportedIssueSerie(ReportedIssueSerieDto issue)
    {
        _issue = issue;
        Task.Run(async () =>
        {
            Optional<SerieDto> info = await ManaxApiSerieClient.GetSerieInfoAsync(issue.SerieId);
            if (!info.Failed) Serie = info.GetValue();
            Optional<UserDto> user = await ManaxApiUserClient.GetUserAsync(issue.UserId);
            if (!user.Failed) User = new User(user.GetValue());
            Optional<List<ReportedIssueSerieTypeDto>> problem =
                await ManaxApiIssueClient.GetAllReportedSerieIssueTypesAsync();
            if (!problem.Failed) Problem = problem.GetValue().Find(p => p.Id == issue.Id);
        });
    }

    public ReportedIssueSerieDto Issue
    {
        get => _issue;
        set
        {
            _issue = value;
            OnPropertyChanged();
        }
    }

    public ReportedIssueSerieTypeDto? Problem
    {
        get => _problem;
        set
        {
            _problem = value;
            OnPropertyChanged();
        }
    }

    public User? User
    {
        get => _user;
        set
        {
            _user = value;
            OnPropertyChanged();
        }
    }

    public SerieDto? Serie
    {
        get => _serie;
        set
        {
            _serie = value;
            OnPropertyChanged();
        }
    }

    public void Close()
    {
        Task.Run(async () => { await ManaxApiIssueClient.CloseSerieIssueAsync(Issue.Id); });
    }
}