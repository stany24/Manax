using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Issue.Automatic;

namespace ManaxClient.ViewModels.Issue;

public partial class AutomaticIssuesPageViewModel : PageViewModel
{
    [ObservableProperty] private List<AutomaticIssueChapterDto> _allInternalChapterIssues = [];
    [ObservableProperty] private List<AutomaticIssueSerieDto> _allInternalSerieIssues = [];
    
    public AutomaticIssuesPageViewModel()
    {
        LoadData();
    }
    
    private void LoadData()
    {
        Task.Run(async () =>
        {
            Optional<List<AutomaticIssueSerieDto>> allAutomaticSerieIssuesResponse = await ManaxApiIssueClient.GetAllAutomaticSerieIssuesAsync();
            if (allAutomaticSerieIssuesResponse.Failed)
            {
                InfoEmitted?.Invoke(this, allAutomaticSerieIssuesResponse.Error);
            }
            else
            {
                AllInternalSerieIssues = allAutomaticSerieIssuesResponse.GetValue();
            }
            
            Optional<List<AutomaticIssueChapterDto>> allAutomaticChapterIssuesResponse = await ManaxApiIssueClient.GetAllAutomaticChapterIssuesAsync();
            if (allAutomaticChapterIssuesResponse.Failed)
            {
                InfoEmitted?.Invoke(this, allAutomaticChapterIssuesResponse.Error);
            }
            else
            {
                AllInternalChapterIssues = allAutomaticChapterIssuesResponse.GetValue();
            }
        });
    }
}