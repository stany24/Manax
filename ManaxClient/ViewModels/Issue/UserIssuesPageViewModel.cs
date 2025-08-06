using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Issue.Reported;

namespace ManaxClient.ViewModels.Issue;

public partial class UserIssuesPageViewModel : PageViewModel
{
    [ObservableProperty] private List<ReportedIssueChapterDto> _allUserChapterIssues = [];
    [ObservableProperty] private List<ReportedIssueSerieDto> _allUserSerieIssues = [];
    
    public UserIssuesPageViewModel()
    {
        LoadData();
    }
    
    private void LoadData()
    {
        Task.Run(async () =>
        {
            Optional<List<ReportedIssueSerieDto>> allReportedSerieIssuesAsync = await ManaxApiIssueClient.GetAllReportedSerieIssuesAsync();
            if (allReportedSerieIssuesAsync.Failed)
            {
                InfoEmitted?.Invoke(this,allReportedSerieIssuesAsync.Error);
            }
            else
            {
                AllUserSerieIssues = allReportedSerieIssuesAsync.GetValue();
            }
            
            Optional<List<ReportedIssueChapterDto>> allReportedChapterIssuesAsync = await ManaxApiIssueClient.GetAllReportedChapterIssuesAsync();
            if (allReportedChapterIssuesAsync.Failed)
            {
                InfoEmitted?.Invoke(this,allReportedChapterIssuesAsync.Error);
            }
            else
            {
                AllUserChapterIssues = allReportedChapterIssuesAsync.GetValue();
            }
        });
    }
}