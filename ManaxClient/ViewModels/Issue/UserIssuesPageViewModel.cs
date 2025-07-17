using System.Collections.Generic;
using System.Threading.Tasks;
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
            List<ReportedIssueSerieDTO>? allUserSerieIssuesAsync = await ManaxApiIssueClient.GetAllReportedSerieIssuesAsync();
            if (allUserSerieIssuesAsync is not null)
            {
                _allUserSerieIssues = allUserSerieIssuesAsync;
            }
            
            // Récupérer les données des problèmes de chapitres
            List<ReportedIssueChapterDTO>? allUserChapterIssuesAsync = await ManaxApiIssueClient.GetAllReportedChapterIssuesAsync();
            if (allUserChapterIssuesAsync is not null)
            {
                _allUserChapterIssues = allUserChapterIssuesAsync;
            }
        });
    }
}