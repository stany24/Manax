using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
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
    [ObservableProperty] private ObservableCollection<IssueChapterAutomatic> _allAutomaticChapterIssues = [];
    [ObservableProperty] private ObservableCollection<IssueSerieAutomatic> _allAutomaticSerieIssues = [];
    [ObservableProperty] private ObservableCollection<IssueChapterReported> _allReportedChapterIssues = [];
    [ObservableProperty] private ObservableCollection<IssueSerieReported> _allReportedSerieIssues = [];

    [ObservableProperty] private bool _showAutomaticIssues = true;
    [ObservableProperty] private bool _showChapterIssues = true;

    public IssuesPageViewModel()
    {
    }

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

            InfoEmitted?.Invoke(this, request.GetValue() ? "Replacement successful" : "Failed to replace chapter");
        }
        catch (Exception e)
        {
            InfoEmitted?.Invoke(this, e.Message);
            Logger.LogError("Error replacing chapter", e);
        }
    }
}