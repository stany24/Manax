using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.Notifications;

namespace ManaxClient.ViewModels.Issue;

public partial class IssuesPageViewModel : PageViewModel
{
    [ObservableProperty] private ObservableCollection<AutomaticIssueChapterDto> _allAutomaticChapterIssues = [];
    [ObservableProperty] private ObservableCollection<AutomaticIssueSerieDto> _allAutomaticSerieIssues = [];
    [ObservableProperty] private ObservableCollection<ReportedIssueChapterDto> _allReportedChapterIssues = [];
    [ObservableProperty] private ObservableCollection<ReportedIssueSerieDto> _allReportedSerieIssues = [];
    
    [ObservableProperty] private bool _showAutomaticIssues = true;
    [ObservableProperty] private bool _showChapterIssues = true;

    public IssuesPageViewModel()
    {
        ServerNotification.OnReportedChapterIssueCreated += OnReportedChapterIssueCreated;
        ServerNotification.OnReportedChapterIssueDeleted += OnReportedChapterIssueDeleted;
        ServerNotification.OnReportedSerieIssueCreated += OnReportedSerieIssueCreated;
        ServerNotification.OnReportedSerieIssueDeleted += OnReportedSerieIssueDeleted;
        LoadData();
    }

    private void OnReportedChapterIssueCreated(ReportedIssueChapterDto issue)
    {
        AllReportedChapterIssues.Add(issue);
    }

    private void OnReportedChapterIssueDeleted(long issueId)
    {
        ReportedIssueChapterDto? issue = AllReportedChapterIssues.FirstOrDefault(i => i.Id == issueId);
        if (issue == null){return;}
        AllReportedChapterIssues.Remove(issue);
    }

    private void OnReportedSerieIssueCreated(ReportedIssueSerieDto issue)
    {
        AllReportedSerieIssues.Add(issue);
    }

    private void OnReportedSerieIssueDeleted(long issueId)
    {
        ReportedIssueSerieDto? issue = AllReportedSerieIssues.FirstOrDefault(i => i.Id == issueId);
        if (issue == null){return;}
        AllReportedSerieIssues.Remove(issue);
    }

    private void LoadData()
    {
        Task.Run(async () =>
        {
            Optional<List<AutomaticIssueSerieDto>> allAutomaticSerieIssuesResponse =
                await ManaxApiIssueClient.GetAllAutomaticSerieIssuesAsync();
            if (allAutomaticSerieIssuesResponse.Failed)
                InfoEmitted?.Invoke(this, allAutomaticSerieIssuesResponse.Error);
            else
                AllAutomaticSerieIssues = new ObservableCollection<AutomaticIssueSerieDto>(allAutomaticSerieIssuesResponse.GetValue());

            Optional<List<AutomaticIssueChapterDto>> allAutomaticChapterIssuesResponse =
                await ManaxApiIssueClient.GetAllAutomaticChapterIssuesAsync();
            if (allAutomaticChapterIssuesResponse.Failed)
                InfoEmitted?.Invoke(this, allAutomaticChapterIssuesResponse.Error);
            else
                AllAutomaticChapterIssues = new ObservableCollection<AutomaticIssueChapterDto>(allAutomaticChapterIssuesResponse.GetValue());

            Optional<List<ReportedIssueChapterDto>> allReportedChapterIssuesResponse =
                await ManaxApiIssueClient.GetAllReportedChapterIssuesAsync();
            if (allReportedChapterIssuesResponse.Failed)
                InfoEmitted?.Invoke(this, allReportedChapterIssuesResponse.Error);
            else
                AllReportedChapterIssues = new ObservableCollection<ReportedIssueChapterDto>(allReportedChapterIssuesResponse.GetValue());

            Optional<List<ReportedIssueSerieDto>> allReportedSerieIssuesResponse =
                await ManaxApiIssueClient.GetAllReportedSerieIssuesAsync();
            if (allReportedSerieIssuesResponse.Failed)
                InfoEmitted?.Invoke(this, allReportedSerieIssuesResponse.Error);
            else
                AllReportedSerieIssues = new ObservableCollection<ReportedIssueSerieDto>(allReportedSerieIssuesResponse.GetValue());
        });
    }
}