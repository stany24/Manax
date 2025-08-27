using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Chapter;

namespace ManaxClient.Models.Issue;

public class ClientAutomaticIssueChapter : ObservableObject
{
    private AutomaticIssueChapterDto _issue;

    public AutomaticIssueChapterDto Issue
    {
        get => _issue;
        set
        {
            _issue = value;
            OnPropertyChanged();
        }
    }

    private ChapterDto? _chapter;
    public ChapterDto? Chapter
    {
        get => _chapter;
        set
        {
            _chapter = value;
            OnPropertyChanged();
        }
    }

    public ClientAutomaticIssueChapter(AutomaticIssueChapterDto issue)
    {
        _issue = issue;
        Task.Run(async () =>
        {
            Optional<ChapterDto> chapterInfo = await ManaxApiChapterClient.GetChapterAsync(issue.ChapterId);
            if (!chapterInfo.Failed) Chapter = chapterInfo.GetValue();
        });
    }
}
