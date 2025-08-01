using System.IO.Compression;
using System.Text.RegularExpressions;
using ImageMagick;
using ManaxLibrary.DTOs.Issue.Automatic;
using ManaxLibrary.DTOs.Setting;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Models.Serie;
using ManaxServer.Settings;

namespace ManaxServer.Services;

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

        CheckMissingChapters(serie);
        CheckDescription(serie);
    }

    private static void CheckDescription(Serie serie)
    {
        uint max = SettingsManager.Data.MaxDescriptionLength;
        uint min = SettingsManager.Data.MinDescriptionLength;
        if (serie.Description.Length > max) { IssueManagerService.CreateSerieIssue(serie.Id, AutomaticIssueSerieType.DescriptionTooLong); }
        if (serie.Description.Length < min) { IssueManagerService.CreateSerieIssue(serie.Id, AutomaticIssueSerieType.DescriptionTooShort); }
    }

    public static void CheckPoster(long serieId)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        Serie? serie = manaxContext.Series.Find(serieId);
        if (serie == null) { return; }

        string directory = serie.Path;
        string fileName = SettingsManager.Data.PosterName +"."+ SettingsManager.Data.ImageFormat.ToString().ToLower();
        string poster = Path.Combine(directory, fileName);
        if (!File.Exists(poster))
        {
            IssueManagerService.CreateSerieIssue(serie.Id, AutomaticIssueSerieType.PosterMissing);
            return;
        }
        CheckPoster(poster,serie.Id);
    }

    private static void CheckPoster(string posterPath,long serieId)
    {
        string format = "."+SettingsManager.Data.ImageFormat.ToString().ToLower();
        uint min = SettingsManager.Data.MinPosterWidth;
        uint max = SettingsManager.Data.MaxPosterWidth;
        if (!Path.GetExtension(posterPath).Equals(format, StringComparison.CurrentCultureIgnoreCase))
            IssueManagerService.CreateSerieIssue(serieId, AutomaticIssueSerieType.PosterWrongFormat);
        try
        {
            MagickImage poster = new(posterPath);
            if (poster.Width < min) { IssueManagerService.CreateSerieIssue(serieId, AutomaticIssueSerieType.PosterWrongFormat); }
            if (poster.Width > max) { IssueManagerService.CreateSerieIssue(serieId, AutomaticIssueSerieType.PosterWrongFormat); }
        }
        catch (Exception)
        {
            IssueManagerService.CreateSerieIssue(serieId, AutomaticIssueSerieType.PosterCouldNotOpen);
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
        CheckChapterFilesAreRightFormat(chapter.Id, files);
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
            uint min = SettingsManager.Data.MinChapterWidth;
            uint max = SettingsManager.Data.MaxChapterWidth;
            if (image.Width < min) {IssueManagerService.CreateChapterIssue(id, AutomaticIssueChapterType.ImageTooSmall); }
            if (image.Width > max) {IssueManagerService.CreateChapterIssue(id, AutomaticIssueChapterType.ImageTooBig); }
        }
        catch
        {
            IssueManagerService.CreateChapterIssue(id, AutomaticIssueChapterType.CouldNotOpen);
        }
    }

    private static void CheckChapterFilesAreRightFormat(long id, string[] chapterFiles)
    {
        ImageFormat format = SettingsManager.Data.ImageFormat;
        if (chapterFiles.Any(file => !Path.GetFileName(file).EndsWith("."+format.ToString().ToLower())))
            IssueManagerService.CreateChapterIssue(id, AutomaticIssueChapterType.NotAllWebp);
    }
}