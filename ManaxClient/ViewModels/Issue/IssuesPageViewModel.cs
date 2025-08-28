using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Controls.Popups.Issue;
using ManaxClient.Models.Issue;
using ManaxClient.ViewModels.Serie;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.Notifications;

namespace ManaxClient.ViewModels.Issue;

public partial class IssuesPageViewModel : PageViewModel
{
    [ObservableProperty] private ObservableCollection<ClientAutomaticIssueChapter> _allAutomaticChapterIssues = [];
    [ObservableProperty] private ObservableCollection<ClientAutomaticIssueSerie> _allAutomaticSerieIssues = [];
    [ObservableProperty] private ObservableCollection<ClientReportedIssueChapter> _allReportedChapterIssues = [];
    [ObservableProperty] private ObservableCollection<ClientReportedIssueSerie> _allReportedSerieIssues = [];

    [ObservableProperty] private bool _showAutomaticIssues = true;
    [ObservableProperty] private bool _showChapterIssues = true;

    public IssuesPageViewModel()
    {
        ServerNotification.OnReportedChapterIssueCreated += OnReportedChapterIssueCreated;
        ServerNotification.OnReportedChapterIssueDeleted += OnReportedChapterIssueDeleted;
        ServerNotification.OnReportedSerieIssueCreated += OnReportedSerieIssueCreated;
        ServerNotification.OnReportedSerieIssueDeleted += OnReportedSerieIssueDeleted;
        LoadData();
    }

    private void OnReportedChapterIssueCreated(ReportedIssueChapterDto issue)
    {
        AllReportedChapterIssues.Add(new ClientReportedIssueChapter(issue));
    }

    private void OnReportedChapterIssueDeleted(long issueId)
    {
        ClientReportedIssueChapter? issue = AllReportedChapterIssues.FirstOrDefault(i => i.Issue.Id == issueId);
        if (issue == null) return;
        AllReportedChapterIssues.Remove(issue);
    }

    private void OnReportedSerieIssueCreated(ReportedIssueSerieDto issue)
    {
        AllReportedSerieIssues.Add(new ClientReportedIssueSerie(issue));
    }

    private void OnReportedSerieIssueDeleted(long issueId)
    {
        ClientReportedIssueSerie? issue = AllReportedSerieIssues.FirstOrDefault(i => i.Issue.Id == issueId);
        if (issue == null) return;
        AllReportedSerieIssues.Remove(issue);
    }

    public void OpenSeriePage(long id)
    {
        PageChangedRequested?.Invoke(this, new SeriePageViewModel(id));
    }

    public async void OnChapterIssueClicked(ChapterDto chapter)
    {
        ReplaceChapterPopup popup = new(chapter);
        PopupRequested?.Invoke(this, popup);
        popup.CloseRequested += (_, _) =>
        {
            popup.Close();
        };
        
        Optional<byte[]> chapterPagesAsync = await ManaxApiChapterClient.GetChapterPagesAsync(chapter.Id);
        if (chapterPagesAsync.Failed)
        {
            InfoEmitted?.Invoke(this, chapterPagesAsync.Error);
            return;
        }
        string saveFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Manax",chapter.SerieId.ToString());
        if (!Directory.Exists(saveFolder)) { Directory.CreateDirectory(saveFolder);}

        string saveFile = Path.Combine(saveFolder, chapter.FileName);
        FileStream fileStream = File.Create(saveFile);
        await fileStream.WriteAsync(chapterPagesAsync.GetValue());
    }

    private void LoadData()
    {
        Task.Run(async () =>
        {
            Optional<List<AutomaticIssueSerieDto>> allAutomaticSerieIssuesResponse =
                await ManaxApiIssueClient.GetAllAutomaticSerieIssuesAsync();
            if (allAutomaticSerieIssuesResponse.Failed)
                InfoEmitted?.Invoke(this, allAutomaticSerieIssuesResponse.Error);
            else
                AllAutomaticSerieIssues =
                    new ObservableCollection<ClientAutomaticIssueSerie>(allAutomaticSerieIssuesResponse.GetValue()
                        .Select(s => new ClientAutomaticIssueSerie(s)));

            Optional<List<AutomaticIssueChapterDto>> allAutomaticChapterIssuesResponse =
                await ManaxApiIssueClient.GetAllAutomaticChapterIssuesAsync();
            if (allAutomaticChapterIssuesResponse.Failed)
                InfoEmitted?.Invoke(this, allAutomaticChapterIssuesResponse.Error);
            else
                AllAutomaticChapterIssues =
                    new ObservableCollection<ClientAutomaticIssueChapter>(allAutomaticChapterIssuesResponse.GetValue()
                        .Select(c => new ClientAutomaticIssueChapter(c)));

            Optional<List<ReportedIssueChapterDto>> allReportedChapterIssuesResponse =
                await ManaxApiIssueClient.GetAllReportedChapterIssuesAsync();
            if (allReportedChapterIssuesResponse.Failed)
                InfoEmitted?.Invoke(this, allReportedChapterIssuesResponse.Error);
            else
                AllReportedChapterIssues =
                    new ObservableCollection<ClientReportedIssueChapter>(allReportedChapterIssuesResponse.GetValue()
                        .Select(c => new ClientReportedIssueChapter(c)));

            Optional<List<ReportedIssueSerieDto>> allReportedSerieIssuesResponse =
                await ManaxApiIssueClient.GetAllReportedSerieIssuesAsync();
            if (allReportedSerieIssuesResponse.Failed)
                InfoEmitted?.Invoke(this, allReportedSerieIssuesResponse.Error);
            else

                AllReportedSerieIssues =
                    new ObservableCollection<ClientReportedIssueSerie>(allReportedSerieIssuesResponse.GetValue()
                        .Select(s => new ClientReportedIssueSerie(s)));
        });
    }
}