using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxClient.Models.Issue;
using ManaxClient.ViewModels.Pages.Serie;
using ManaxClient.ViewModels.Popup.ConfirmCancel;
using ManaxClient.ViewModels.Popup.ConfirmCancel.Content;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Issue.Reported;
using ManaxLibrary.Logging;
using ManaxLibrary.Notifications;

namespace ManaxClient.ViewModels.Pages.Issue;

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
        ClientReportedIssueChapter? issue =
            AllReportedChapterIssues.FirstOrDefault(i => i.Issue.Id == issueId);
        if (issue == null) return;
        AllReportedChapterIssues.Remove(issue);
    }

    private void OnReportedSerieIssueCreated(ReportedIssueSerieDto issue)
    {
        AllReportedSerieIssues.Add(new ClientReportedIssueSerie(issue));
    }

    private void OnReportedSerieIssueDeleted(long issueId)
    {
        ClientReportedIssueSerie? issue =
            AllReportedSerieIssues.FirstOrDefault(i => i.Issue.Id == issueId);
        if (issue == null) return;
        AllReportedSerieIssues.Remove(issue);
    }

    public void OpenSeriePage(long id)
    {
        PageChangedRequested?.Invoke(this, new SeriePageViewModel(id));
    }

    public void OnChapterIssueClicked(ChapterDto chapter)
    {
        string serieFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Manax",
            chapter.SerieId.ToString(CultureInfo.InvariantCulture));
        if (!Directory.Exists(serieFolder)) Directory.CreateDirectory(serieFolder);
        string saveFile = Path.Combine(serieFolder, chapter.FileName);
        string saveFolder = Path.Combine(serieFolder, Path.GetFileNameWithoutExtension(chapter.FileName));

        ReplaceChapterViewModel content = new(saveFolder);
        ConfirmCancelViewModel viewModel = new(content);
        Controls.Popups.Popup popup = new(viewModel);
        popup.Closed += (_, _) => { PopupClosed(viewModel, saveFile, saveFolder, chapter); };
        PopupRequested?.Invoke(this, popup);

        DownloadChapter(saveFile, saveFolder, chapter, content);
    }

    private async void DownloadChapter(string saveFile, string saveFolder, ChapterDto chapter,
        ReplaceChapterViewModel content)
    {
        try
        {
            Optional<byte[]> chapterPagesAsync = await ManaxApiChapterClient.GetChapterPagesAsync(chapter.Id);
            if (chapterPagesAsync.Failed)
            {
                InfoEmitted?.Invoke(this, chapterPagesAsync.Error);
                return;
            }

            FileStream fileStream = File.Create(saveFile);
            await fileStream.WriteAsync(chapterPagesAsync.GetValue());
            await fileStream.DisposeAsync();
            fileStream.Close();
            ZipFile.ExtractToDirectory(saveFile, saveFolder);
            File.Delete(saveFile);
            content.CanConfirm = true;
        }
        catch (Exception e)
        {
            InfoEmitted?.Invoke(this, e.Message);
            Logger.LogError("Error downloading chapter", e, Environment.StackTrace);
        }
    }

    private async void PopupClosed(ConfirmCancelViewModel viewModel, string saveFile, string saveFolder,
        ChapterDto chapter)
    {
        try
        {
            if (viewModel.Canceled()) return;

            if (File.Exists(saveFile)) File.Delete(saveFile);
            ZipFile.CreateFromDirectory(saveFolder, saveFile);
            byte[] data = await File.ReadAllBytesAsync(saveFile);

            Optional<bool> request =
                await ManaxApiUploadClient.ReplaceChapterAsync(new ByteArrayContent(data), chapter.FileName,
                    chapter.SerieId);
            if (request.Failed)
            {
                InfoEmitted?.Invoke(this, request.Error);
                Logger.LogFailure("Replace chapter failed", Environment.StackTrace);
                return;
            }

            File.Delete(saveFile);
            Directory.Delete(saveFolder, true);

            InfoEmitted?.Invoke(this, request.GetValue() ? "Replacement successful" : "Failed to replace chapter");
        }
        catch (Exception e)
        {
            InfoEmitted?.Invoke(this, e.Message);
            Logger.LogError("Error replacing chapter", e, Environment.StackTrace);
        }
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