using System.Collections.Generic;
using System.Threading.Tasks;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTOs.Issue.User;

namespace ManaxApp.ViewModels.Issue;

public partial class UserIssuesPageViewModel : PageViewModel
{
    private List<UserChapterIssueDTO> _allUserChapterIssues = [];
    private List<UserSerieIssueDTO> _allUserSerieIssues = [];
    
    public UserIssuesPageViewModel()
    {
        ControlBarVisible = true;
        LoadData();
    }
    
    private void LoadData()
    {
        Task.Run(async () =>
        {
            List<UserSerieIssueDTO>? allUserSerieIssuesAsync = await ManaxApiIssueClient.GetAllUserSerieIssuesAsync();
            if (allUserSerieIssuesAsync is not null)
            {
                _allUserSerieIssues = allUserSerieIssuesAsync;
            }
            
            // Récupérer les données des problèmes de chapitres
            List<UserChapterIssueDTO>? allUserChapterIssuesAsync = await ManaxApiIssueClient.GetAllUserChapterIssuesAsync();
            if (allUserChapterIssuesAsync is not null)
            {
                _allUserChapterIssues = allUserChapterIssuesAsync;
            }
        });
    }
}