using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Issue.Reported;

namespace ManaxClient.Controls.Popups.Issue;

public class CreateChapterIssuePopup(long chapterId): ConfirmCancelPopup
{
    private ComboBox _issueTypeComboBox = null!;
    
    protected override Grid GetFormGrid()
    {
        Canceled = true;
        _ = LoadProblems();
        Grid grid = new()
        {
            ColumnDefinitions = new ColumnDefinitions("*,*")
        };

        Label titleLabel = new()
        {
            Content = "Problème:",
            FontSize = 18,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            Margin = new Avalonia.Thickness(0, 0, 0, 10)
        };
        Grid.SetColumn(titleLabel, 0);
        
        _issueTypeComboBox = new ComboBox
        {
            DisplayMemberBinding = new Binding(nameof(ReportedIssueChapterTypeDto.Name))
        };
        Grid.SetColumn(_issueTypeComboBox, 1);
        
        grid.Children.Add(titleLabel);
        grid.Children.Add(_issueTypeComboBox);
        return grid;
    }

    private async Task LoadProblems()
    {
        ManaxLibrary.Optional<List<ReportedIssueChapterTypeDto>> issuesTypes =
            await ManaxApiIssueClient.GetAllReportedChapterIssueTypesAsync();
        if (!issuesTypes.Failed)
        {
            Dispatcher.UIThread.Post(() =>
            {
                foreach (ReportedIssueChapterTypeDto issue in issuesTypes.GetValue())
                {
                    _issueTypeComboBox.Items.Add(issue);
                }
            });
        }
    }

    protected override void OkButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (_issueTypeComboBox.SelectedItem is ReportedIssueChapterTypeDto)
        {
            Canceled = false;
            CloseRequested?.Invoke(this, System.EventArgs.Empty);
        }
    }

    public ReportedIssueChapterCreateDto GetResult()
    {
        return new ReportedIssueChapterCreateDto
        {
            ChapterId = chapterId,
            ProblemId = (_issueTypeComboBox.SelectedItem as ReportedIssueChapterTypeDto)!.Id
        };
    }
}