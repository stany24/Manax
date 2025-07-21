using System.Collections.Generic;
using System.Threading.Tasks;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTOs.Issue.Reported;

namespace ManaxClient.ViewModels.Issue;

public partial class UserIssuesPageViewModel : PageViewModel
{
    private List<ReportedIssueChapterDTO> _allUserChapterIssues = [];
    private List<ReportedIssueSerieDTO> _allUserSerieIssues = [];
    
    public UserIssuesPageViewModel()
    {
        LoadData();
    }
    
    private void LoadData()
    {
        Task.Run(async () =>
        {
            Optional<List<ReportedIssueSerieDTO>> allReportedSerieIssuesAsync = await ManaxApiIssueClient.GetAllReportedSerieIssuesAsync();
            if (allReportedSerieIssuesAsync.Failed)
            {
                InfoEmitted?.Invoke(this,allReportedSerieIssuesAsync.Error);
            }
            else
            {
                _allUserSerieIssues = allReportedSerieIssuesAsync.GetValue();
            }
            
            Optional<List<ReportedIssueChapterDTO>> allReportedChapterIssuesAsync = await ManaxApiIssueClient.GetAllReportedChapterIssuesAsync();
            if (allReportedChapterIssuesAsync.Failed)
            {
                InfoEmitted?.Invoke(this,allReportedChapterIssuesAsync.Error);
            }
            else
            {
                _allUserChapterIssues = allReportedChapterIssuesAsync.GetValue();
            }
        });
    }
}