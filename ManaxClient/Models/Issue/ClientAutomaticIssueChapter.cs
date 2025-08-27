using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Issue.Automatic;

namespace ManaxClient.Models.Issue;

public class ClientAutomaticIssueChapter : ObservableObject
{
    private ChapterDto? _chapter;
    private AutomaticIssueChapterDto _issue;

    public ClientAutomaticIssueChapter(AutomaticIssueChapterDto issue)
    {
        _issue = issue;
        Task.Run(async () =>
        {
            Optional<ChapterDto> chapterInfo = await ManaxApiChapterClient.GetChapterAsync(issue.ChapterId);
            if (!chapterInfo.Failed) Chapter = chapterInfo.GetValue();
        });
    }

    public AutomaticIssueChapterDto Issue
    {
        get => _issue;
        set
        {
            _issue = value;
            OnPropertyChanged();
        }
    }

    public ChapterDto? Chapter
    {
        get => _chapter;
        set
        {
            _chapter = value;
            OnPropertyChanged();
        }
    }
}