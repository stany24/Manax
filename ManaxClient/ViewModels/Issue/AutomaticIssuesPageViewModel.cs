using System.Collections.Generic;
using System.Threading.Tasks;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTOs.Issue.Automatic;

namespace ManaxClient.ViewModels.Issue;

public partial class AutomaticIssuesPageViewModel : PageViewModel
{
    // Collections
    private List<AutomaticIssueChapterDTO> _allInternalChapterIssues = [];
    private List<AutomaticIssueSerieDTO> _allInternalSerieIssues = [];
    public AutomaticIssuesPageViewModel()
    {
        LoadData();
    }
    
    private void LoadData()
    {
        Task.Run(async () =>
        {
            List<AutomaticIssueSerieDTO>? allInternalSerieIssuesAsync = await ManaxApiIssueClient.GetAllAutomaticSerieIssuesAsync();
            if (allInternalSerieIssuesAsync is not null)
            {
                _allInternalSerieIssues = allInternalSerieIssuesAsync;
            }
            
            List<AutomaticIssueChapterDTO>? allInternalChapterIssuesAsync = await ManaxApiIssueClient.GetAllAutomaticChapterIssuesAsync();
            if (allInternalChapterIssuesAsync is not null)
            {
                _allInternalChapterIssues = allInternalChapterIssuesAsync;
            }
        });
    }
}