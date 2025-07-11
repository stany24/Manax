using System.IO.Compression;
using System.Text.RegularExpressions;
using ImageMagick;
using ManaxApi.Models;
using ManaxApi.Models.Chapter;
using ManaxApi.Models.Serie;
using ManaxLibrary.DTOs.Issue.Automatic;

namespace ManaxApi.Services;

public static partial class CheckService
{
    private static IServiceScopeFactory _scopeFactory = null!;

    [GeneratedRegex("\\d{1,4}")]
    private static partial Regex RegexNumber();

    public static void Initialize(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public static void CheckSerie(long serieId)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        Serie? serie = manaxContext.Series.Find(serieId);
        if (serie == null) return;
        manaxContext.AutomaticIssuesSerie.RemoveRange(manaxContext.AutomaticIssuesSerie.Where(i => i.SerieId == serieId));
        manaxContext.SaveChanges();

        CheckPoster(serie);
        CheckMissingChapters(serie);
        CheckDescription(serie);
    }

    private static void CheckDescription(Serie serie)
    {
        switch (serie.Description.Length)
        {
            case < 100:
                IssueManagerService.CreateSerieIssue(serie.Id, AutomaticIssueSerieType.DescriptionTooShort);
                break;
            case > 1000:
                IssueManagerService.CreateSerieIssue(serie.Id, AutomaticIssueSerieType.DescriptionTooLong);
                break;
        }
    }

    private static void CheckPoster(Serie serie)
    {
        string[] files = Directory.GetFiles(serie.Path);
        List<string> posters = files
            .Where(f => Path.GetFileName(f).StartsWith("poster.", StringComparison.CurrentCultureIgnoreCase)).ToList();
        switch (posters.Count)
        {
            case 0:
                IssueManagerService.CreateSerieIssue(serie.Id, AutomaticIssueSerieType.PosterMissing);
                break;
            case 1:
                string poster = posters.First();
                if (!Path.GetExtension(poster).Equals(".webp", StringComparison.CurrentCultureIgnoreCase))
                    IssueManagerService.CreateSerieIssue(serie.Id, AutomaticIssueSerieType.PosterWrongFormat);
                break;
            case > 1:
                IssueManagerService.CreateSerieIssue(serie.Id, AutomaticIssueSerieType.PosterDuplicate);
                break;
        }
    }

    private static void CheckMissingChapters(Serie serie)
    {
        string[] chapters = Directory.GetFiles(serie.Path);
        if (chapters.Length == 0)
        {
            IssueManagerService.CreateSerieIssue(serie.Id, AutomaticIssueSerieType.MissingChapter);
            return;
        }

        Array.Sort(chapters);
        Regex regex = RegexNumber();
        string last = Path.GetFileName(chapters[^1]);
        Match match = regex.Match(last);
        if (!match.Success) return;
        if (Convert.ToInt32(match.Value) == chapters.Length) return;
        IssueManagerService.CreateSerieIssue(serie.Id, AutomaticIssueSerieType.MissingChapter);
    }

    public static void CheckChapter(long chapterId)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        Chapter? chapter = manaxContext.Chapters.Find(chapterId);
        if (chapter == null) return;

        chapter.Pages = ZipFile.OpenRead(chapter.Path).Entries.Count;

        manaxContext.AutomaticIssuesChapter.RemoveRange(manaxContext.AutomaticIssuesChapter.Where(i => i.ChapterId == chapterId));
        manaxContext.SaveChanges();

        CheckChapterDeep(chapter);
    }

    private static void CheckChapterDeep(Chapter chapter)
    {
        string copyName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try
        {
            ZipFile.ExtractToDirectory(chapter.Path, copyName);
        }
        catch
        {
            if (Directory.Exists(copyName)) Directory.Delete(copyName, true);
            IssueManagerService.CreateChapterIssue(chapter.Id, AutomaticIssueChapterType.CouldNotOpen);
            return;
        }

        string[] files = Directory.GetFiles(copyName);
        CheckWidthOfChapter(chapter.Id, files);
        CheckNames(chapter.Id, files);
        CheckChapterFilesAreWebp(chapter.Id, files);
        CheckMissingPages(chapter.Id, files);
        Directory.Delete(copyName, true);
    }

    private static void CheckNames(long id, string[] chapterFiles)
    {
        if (chapterFiles.Any(file => !Path.GetFileName(file).StartsWith('P')))
            IssueManagerService.CreateChapterIssue(id, AutomaticIssueChapterType.BadPageNaming);
    }

    private static void CheckMissingPages(long id, string[] chapterFiles)
    {
        Array.Sort(chapterFiles);
        Regex regex = RegexNumber();
        string last = Path.GetFileName(chapterFiles[^1]);
        Match match = regex.Match(last);
        if (!match.Success) return;
        if (Convert.ToInt32(match.Value) == chapterFiles.Length) return;
        IssueManagerService.CreateChapterIssue(id, AutomaticIssueChapterType.MissingPage);
    }

    private static void CheckWidthOfChapter(long id, string[] images)
    {
        foreach (string image in images) CheckSizeOfImage(id, image);
    }

    private static void CheckSizeOfImage(long id, string file)
    {
        try
        {
            using MagickImage image = new(file);
            int width = (int)image.Width;
            switch (width)
            {
                case > 800:
                    IssueManagerService.CreateChapterIssue(id, AutomaticIssueChapterType.ImageTooBig);
                    break;
                case < 720:
                    IssueManagerService.CreateChapterIssue(id, AutomaticIssueChapterType.ImageTooSmall);
                    break;
            }
        }
        catch
        {
            IssueManagerService.CreateChapterIssue(id, AutomaticIssueChapterType.CouldNotOpen);
        }
    }

    private static void CheckChapterFilesAreWebp(long id, string[] chapterFiles)
    {
        if (chapterFiles.Any(file => !Path.GetFileName(file).EndsWith(".webp")))
            IssueManagerService.CreateChapterIssue(id, AutomaticIssueChapterType.NotAllWebp);
    }
}