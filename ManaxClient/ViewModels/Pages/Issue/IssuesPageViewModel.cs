using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData;
using DynamicData.Binding;
using ManaxClient.Models.Issue;
using ManaxClient.ViewModels.Pages.Serie;
using ManaxClient.ViewModels.Popup.ConfirmCancel;
using ManaxClient.ViewModels.Popup.ConfirmCancel.Content;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.Logging;

namespace ManaxClient.ViewModels.Pages.Issue;

public partial class IssuesPageViewModel : PageViewModel
{
    private readonly ReadOnlyObservableCollection<IssueChapterAutomatic> _issueChapterAutomatic;
    private readonly ReadOnlyObservableCollection<IssueChapterReported> _issueChapterReported;
    private readonly ReadOnlyObservableCollection<IssueSerieAutomatic> _issueSerieAutomatic;
    private readonly ReadOnlyObservableCollection<IssueSerieReported> _issueSerieReported;

    [ObservableProperty] private bool _showAutomaticIssues = true;
    [ObservableProperty] private bool _showChapterIssues = true;

    [ObservableProperty] private string _pageTitle = string.Empty;
    [ObservableProperty] private string _automaticText = string.Empty;
    [ObservableProperty] private string _reportedText = string.Empty;
    [ObservableProperty] private string _chaptersText = string.Empty;
    [ObservableProperty] private string _seriesText = string.Empty;
    [ObservableProperty] private string _automaticChaptersTitle = string.Empty;
    [ObservableProperty] private string _automaticSeriesTitle = string.Empty;
    [ObservableProperty] private string _reportedChaptersTitle = string.Empty;
    [ObservableProperty] private string _reportedSeriesTitle = string.Empty;
    [ObservableProperty] private string _noAutomaticChapterText = string.Empty;
    [ObservableProperty] private string _noAutomaticSeriesText = string.Empty;
    [ObservableProperty] private string _noReportedChapterText = string.Empty;
    [ObservableProperty] private string _noReportedSeriesText = string.Empty;
    [ObservableProperty] private string _replacementSuccessfulText = string.Empty;
    [ObservableProperty] private string _replacementFailedText = string.Empty;

    public IssuesPageViewModel()
    {
        SortExpressionComparer<IssueChapterAutomatic> comparer1 =
            SortExpressionComparer<IssueChapterAutomatic>.Descending(t => t.CreatedAt);
        SortExpressionComparer<IssueSerieAutomatic> comparer2 =
            SortExpressionComparer<IssueSerieAutomatic>.Descending(t => t.CreatedAt);
        SortExpressionComparer<IssueChapterReported> comparer3 =
            SortExpressionComparer<IssueChapterReported>.Descending(t => t.CreatedAt);
        SortExpressionComparer<IssueSerieReported> comparer4 =
            SortExpressionComparer<IssueSerieReported>.Descending(t => t.CreatedAt);
        
        IssueSource.IssueChapterAutomatic
            .Connect()
            .SortAndBind(out _issueChapterAutomatic, comparer1)
            .Subscribe();
        IssueSource.IssueSerieAutomatic
            .Connect()
            .SortAndBind(out _issueSerieAutomatic, comparer2)
            .Subscribe();
        IssueSource.IssueChapterReported
            .Connect()
            .SortAndBind(out _issueChapterReported, comparer3)
            .Subscribe();
        IssueSource.IssueSerieReported
            .Connect()
            .SortAndBind(out _issueSerieReported, comparer4)
            .Subscribe();
        
        InitializeLocalization();
    }

    private void InitializeLocalization()
    {
        Localize(() => PageTitle, "IssuesPage.Title");
        Localize(() => AutomaticText, "IssuesPage.Automatic");
        Localize(() => ReportedText, "IssuesPage.Reported");
        Localize(() => ChaptersText, "IssuesPage.Chapters");
        Localize(() => SeriesText, "IssuesPage.Series");
        Localize(() => AutomaticChaptersTitle, "IssuesPage.AutomaticChapters");
        Localize(() => AutomaticSeriesTitle, "IssuesPage.AutomaticSeries");
        Localize(() => ReportedChaptersTitle, "IssuesPage.ReportedChapters");
        Localize(() => ReportedSeriesTitle, "IssuesPage.ReportedSeries");
        Localize(() => NoAutomaticChapterText, "IssuesPage.NoAutomaticChapter");
        Localize(() => NoAutomaticSeriesText, "IssuesPage.NoAutomaticSeries");
        Localize(() => NoReportedChapterText, "IssuesPage.NoReportedChapter");
        Localize(() => NoReportedSeriesText, "IssuesPage.NoReportedSeries");
        Localize(() => ReplacementSuccessfulText, "IssuesPage.ReplacementSuccessful");
        Localize(() => ReplacementFailedText, "IssuesPage.ReplacementFailed");
    }

    public ReadOnlyObservableCollection<IssueChapterAutomatic> IssueChapterAutomatic => _issueChapterAutomatic;
    public ReadOnlyObservableCollection<IssueSerieAutomatic> IssueSerieAutomatic => _issueSerieAutomatic;
    public ReadOnlyObservableCollection<IssueChapterReported> IssueChapterReported => _issueChapterReported;
    public ReadOnlyObservableCollection<IssueSerieReported> IssueSerieReported => _issueSerieReported;

    public void OpenSeriePage(Models.Serie serie)
    {
        PageChangedRequested?.Invoke(this, new SeriePageViewModel(serie));
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
            Logger.LogError("Error downloading chapter", e);
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
                Logger.LogFailure("Replace chapter failed");
                return;
            }

            File.Delete(saveFile);
            Directory.Delete(saveFolder, true);

            string message = request.GetValue() ? ReplacementSuccessfulText : ReplacementFailedText;
            InfoEmitted?.Invoke(this, message);
        }
        catch (Exception e)
        {
            InfoEmitted?.Invoke(this, e.Message);
            Logger.LogError("Error replacing chapter", e);
        }
    }
}