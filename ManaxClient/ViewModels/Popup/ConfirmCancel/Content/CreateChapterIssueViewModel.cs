using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Issue.Reported;

namespace ManaxClient.ViewModels.Popup.ConfirmCancel.Content;

public class CreateChapterIssueViewModel: ConfirmCancelContentViewModel
{
    public ObservableCollection<ReportedIssueChapterTypeDto> Issues { get; } = [];
    private readonly long _chapterId;
    private ReportedIssueChapterTypeDto? _selectedIssue;
    public ReportedIssueChapterTypeDto? SelectedIssue
    {
        get => _selectedIssue;
        set
        {
            SetProperty(ref _selectedIssue, value);
            CanConfirm = value != null;
        }
    }

    public CreateChapterIssueViewModel(long chapterId)
    {
        CanConfirm = false;
        _chapterId = chapterId;
        _ = LoadProblems();
    }

    private async Task LoadProblems()
    {
        ManaxLibrary.Optional<List<ReportedIssueChapterTypeDto>> issuesTypes =
            await ManaxApiIssueClient.GetAllReportedChapterIssueTypesAsync();
        if (!issuesTypes.Failed)
            Dispatcher.UIThread.Post(() =>
            {
                foreach (ReportedIssueChapterTypeDto issue in issuesTypes.GetValue())
                    Issues.Add(issue);
                if (Issues.Count > 0)
                {
                    SelectedIssue = Issues[0];
                    CanConfirm = true;
                }
            });
    }

    public ReportedIssueChapterCreateDto GetResult()
    {
        return new ReportedIssueChapterCreateDto
        {
            ChapterId = _chapterId,
            ProblemId = SelectedIssue?.Id ?? 0
        };
    }
}