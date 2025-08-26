using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Issue.Reported;

namespace ManaxClient.ViewModels.Issue;

public partial class IssuesPageViewModel : PageViewModel
{
    [ObservableProperty] private List<AutomaticIssueChapterDto> _allInternalChapterIssues = [];
    [ObservableProperty] private List<AutomaticIssueSerieDto> _allInternalSerieIssues = [];
    [ObservableProperty] private List<ReportedIssueChapterDto> _allReportedChapterIssues = [];
    [ObservableProperty] private List<ReportedIssueSerieDto> _allReportedSerieIssues = [];
    
    [ObservableProperty] private bool _showAutomaticIssues = true;
    [ObservableProperty] private bool _showChapterIssues = true;

    public IssuesPageViewModel()
    {
        LoadData();
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
                AllInternalSerieIssues = allAutomaticSerieIssuesResponse.GetValue();

            Optional<List<AutomaticIssueChapterDto>> allAutomaticChapterIssuesResponse =
                await ManaxApiIssueClient.GetAllAutomaticChapterIssuesAsync();
            if (allAutomaticChapterIssuesResponse.Failed)
                InfoEmitted?.Invoke(this, allAutomaticChapterIssuesResponse.Error);
            else
                AllInternalChapterIssues = allAutomaticChapterIssuesResponse.GetValue();

            Optional<List<ReportedIssueChapterDto>> allReportedChapterIssuesResponse =
                await ManaxApiIssueClient.GetAllReportedChapterIssuesAsync();
            if (allReportedChapterIssuesResponse.Failed)
                InfoEmitted?.Invoke(this, allReportedChapterIssuesResponse.Error);
            else
                AllReportedChapterIssues = allReportedChapterIssuesResponse.GetValue();

            Optional<List<ReportedIssueSerieDto>> allReportedSerieIssuesResponse =
                await ManaxApiIssueClient.GetAllReportedSerieIssuesAsync();
            if (allReportedSerieIssuesResponse.Failed)
                InfoEmitted?.Invoke(this, allReportedSerieIssuesResponse.Error);
            else
                AllReportedSerieIssues = allReportedSerieIssuesResponse.GetValue();
        });
    }

    public async Task CloseReportedChapterIssueAsync(ReportedIssueChapterDto issue)
    {
        Optional<bool> result = await ManaxApiIssueClient.CloseChapterIssueAsync(issue.Id);
        if (result.Failed)
            InfoEmitted?.Invoke(this, result.Error);
        else
            AllReportedChapterIssues = AllReportedChapterIssues.Where(i => i.Id != issue.Id).ToList();
    }

    public async Task CloseReportedSerieIssueAsync(ReportedIssueSerieDto issue)
    {
        Optional<bool> result = await ManaxApiIssueClient.CloseSerieIssueAsync(issue.Id);
        if (result.Failed)
            InfoEmitted?.Invoke(this, result.Error);
        else
            AllReportedSerieIssues = AllReportedSerieIssues.Where(i => i.Id != issue.Id).ToList();
    }
}