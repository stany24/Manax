using System.Collections.Generic;
using System.Threading.Tasks;
using ManaxLibrary;
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
            Optional<List<AutomaticIssueSerieDTO>> allAutomaticSerieIssuesResponse = await ManaxApiIssueClient.GetAllAutomaticSerieIssuesAsync();
            if (allAutomaticSerieIssuesResponse.Failed)
            {
                InfoEmitted?.Invoke(this, allAutomaticSerieIssuesResponse.Error);
            }
            else
            {
                _allInternalSerieIssues = allAutomaticSerieIssuesResponse.GetValue();
            }
            
            Optional<List<AutomaticIssueChapterDTO>> allAutomaticChapterIssuesResponse = await ManaxApiIssueClient.GetAllAutomaticChapterIssuesAsync();
            if (allAutomaticChapterIssuesResponse.Failed)
            {
                InfoEmitted?.Invoke(this, allAutomaticChapterIssuesResponse.Error);
            }
            else
            {
                _allInternalChapterIssues = allAutomaticChapterIssuesResponse.GetValue();
            }
        });
    }
}