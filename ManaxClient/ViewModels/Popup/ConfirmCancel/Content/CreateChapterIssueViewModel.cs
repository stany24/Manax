using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Issue.Reported;

namespace ManaxClient.ViewModels.Popup.ConfirmCancel.Content;

public class CreateChapterIssueViewModel : ConfirmCancelContentViewModel
{
    private readonly long _chapterId;
    private IssueChapterReportedTypeDto? _selectedIssue;

    public CreateChapterIssueViewModel(long chapterId)
    {
        CanConfirm = false;
        _chapterId = chapterId;
        _ = LoadProblems();
    }

    public ObservableCollection<IssueChapterReportedTypeDto> Issues { get; } = [];

    public IssueChapterReportedTypeDto? SelectedIssue
    {
        get => _selectedIssue;
        set
        {
            SetProperty(ref _selectedIssue, value);
            CanConfirm = value != null;
        }
    }

    private async Task LoadProblems()
    {
        Optional<List<IssueChapterReportedTypeDto>> issuesTypes =
            await ManaxApiIssueClient.GetAllReportedChapterIssueTypesAsync();
        if (!issuesTypes.Failed)
            Dispatcher.UIThread.Post(() =>
            {
                foreach (IssueChapterReportedTypeDto issue in issuesTypes.GetValue())
                    Issues.Add(issue);
                if (Issues.Count <= 0) return;
                SelectedIssue = Issues[0];
                CanConfirm = true;
            });
    }

    public IssueChapterReportedCreateDto GetResult()
    {
        return new IssueChapterReportedCreateDto
        {
            ChapterId = _chapterId,
            ProblemId = SelectedIssue?.Id ?? 0
        };
    }
}