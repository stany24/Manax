using System.Collections.Generic;
using System.Threading.Tasks;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTOs.Issue.Internal;

namespace ManaxApp.ViewModels.Issue;

public partial class AutomaticIssuesPageViewModel : PageViewModel
{
    // Collections
    private List<InternalChapterIssueDTO> _allInternalChapterIssues = [];
    private List<InternalSerieIssueDTO> _allInternalSerieIssues = [];
    public AutomaticIssuesPageViewModel()
    {
        ControlBarVisible = true;
        LoadData();
    }
    
    private void LoadData()
    {
        Task.Run(async () =>
        {
            List<InternalSerieIssueDTO>? allInternalSerieIssuesAsync = await ManaxApiIssueClient.GetAllInternalSerieIssuesAsync();
            if (allInternalSerieIssuesAsync is not null)
            {
                _allInternalSerieIssues = allInternalSerieIssuesAsync;
            }
            
            List<InternalChapterIssueDTO>? allInternalChapterIssuesAsync = await ManaxApiIssueClient.GetAllInternalChapterIssuesAsync();
            if (allInternalChapterIssuesAsync is not null)
            {
                _allInternalChapterIssues = allInternalChapterIssuesAsync;
            }
        });
    }
}