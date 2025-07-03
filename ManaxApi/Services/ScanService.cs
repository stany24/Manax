using System.IO.Compression;
using System.Text.RegularExpressions;
using ImageMagick;
using ManaxApi.Models;
using ManaxApi.Models.Chapter;
using ManaxApi.Models.Issue;
using ManaxApi.Models.Library;
using ManaxApi.Models.Serie;
using ManaxApi.Task;

namespace ManaxApi.Services;

public static partial class ScanService
{
    private static IServiceScopeFactory _scopeFactory = null!;

    [GeneratedRegex("\\d{1,4}")]
    private static partial Regex RegexNumber();

    public static void Initialize(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public static void ScanLibrary(Library library)
    {
        Console.WriteLine("Scanning library: " + library.Name);

        using IServiceScope scope = _scopeFactory.CreateScope();
        ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();

        string[] directories = Directory.GetDirectories(library.Path);

        foreach (string directory in directories)
        {
            string folderName = Path.GetFileName(directory);
            Serie? serie = manaxContext.Series.FirstOrDefault(s => s.FolderName == folderName);

            if (serie == null)
            {
                serie = new Serie
                {
                    LibraryId = library.Id,
                    FolderName = folderName,
                    Title = folderName,
                    Description = string.Empty,
                    Path = directory
                };
                manaxContext.Series.Add(serie);
                manaxContext.SaveChanges();
            }

            _ = TaskManagerService.AddTaskAsync(new SerieScanTask(serie.Id));
        }
    }

    public static void ScanSerie(long serieId)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();

        Serie? serie = manaxContext.Series.Find(serieId);
        if (serie == null) return;

        Console.WriteLine("Scanning serie: " + serie.Title);
        string[] files = Directory.GetFiles(serie.Path);

        string[] chapters = files.Where(f => f.ToLower().EndsWith(".cbz")).ToArray();

        foreach (string file in chapters)
        {
            string fileName = Path.GetFileName(file);
            Chapter? chapter =
                manaxContext.Chapters.FirstOrDefault(s => s.FileName == fileName && s.SerieId == serie.Id);

            if (chapter == null)
            {
                chapter = new Chapter
                {
                    SerieId = serie.Id,
                    FileName = Path.GetFileName(file),
                    Path = file,
                    Pages = 0,
                    Number = 0
                };
                manaxContext.Chapters.Add(chapter);
                manaxContext.SaveChanges();
            }

            _ = TaskManagerService.AddTaskAsync(new ChapterScanTask(chapter.Id));
        }

        CheckSerie(serieId);
    }

    private static void CheckSerie(long serieId)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        Serie? serie = manaxContext.Series.Find(serieId);
        if (serie == null) return;
        manaxContext.SerieIssues.RemoveRange(manaxContext.SerieIssues.Where(i => i.SerieId == serieId));
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
                IssueManagerService.CreateSerieIssue(serie.Id, SerieIssueTypeEnum.DescriptionTooShort);
                break;
            case > 1000:
                IssueManagerService.CreateSerieIssue(serie.Id, SerieIssueTypeEnum.DescriptionTooLong);
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
                IssueManagerService.CreateSerieIssue(serie.Id, SerieIssueTypeEnum.PosterMissing);
                break;
            case 1:
                string poster = posters.First();
                if (!Path.GetExtension(poster).Equals(".webp", StringComparison.CurrentCultureIgnoreCase))
                    IssueManagerService.CreateSerieIssue(serie.Id, SerieIssueTypeEnum.PosterWrongFormat);
                break;
            case > 1:
                IssueManagerService.CreateSerieIssue(serie.Id, SerieIssueTypeEnum.PosterDuplicate);
                break;
        }
    }

    private static void CheckMissingChapters(Serie serie)
    {
        string[] chapters = Directory.GetFiles(serie.Path);
        if (chapters.Length == 0)
        {
            IssueManagerService.CreateSerieIssue(serie.Id, SerieIssueTypeEnum.MissingChapter);
            return;
        }

        Array.Sort(chapters);
        Regex regex = RegexNumber();
        string last = Path.GetFileName(chapters[^1]);
        Match match = regex.Match(last);
        if (!match.Success) return;
        if (Convert.ToInt32(match.Value) == chapters.Length) return;
        IssueManagerService.CreateSerieIssue(serie.Id, SerieIssueTypeEnum.MissingChapter);
    }

    public static void ScanChapter(long chapterId)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();

        Chapter? chapter = manaxContext.Chapters.Find(chapterId);
        if (chapter == null) return;

        Console.WriteLine("Scanning chapter: " + chapter.FileName);

        CheckChapter(chapterId);
    }

    private static void CheckChapter(long chapterId)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        Chapter? chapter = manaxContext.Chapters.Find(chapterId);
        if (chapter == null) return;

        chapter.Pages = ZipFile.OpenRead(chapter.Path).Entries.Count;

        manaxContext.ChapterIssues.RemoveRange(manaxContext.ChapterIssues.Where(i => i.ChapterId == chapterId));
        manaxContext.SaveChanges();

        CheckChapterDeep(chapter);
    }

    private static void CheckChapterDeep(Chapter chapter)
    {
        string copyName = Path.Combine(Directory.GetCurrentDirectory(), Path.GetRandomFileName());
        try
        {
            ZipFile.ExtractToDirectory(chapter.Path, copyName);
        }
        catch
        {
            if (Directory.Exists(copyName)) Directory.Delete(copyName, true);
            IssueManagerService.CreateChapterIssue(chapter.Id, ChapterIssueTypeEnum.CouldNotOpen);
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
            IssueManagerService.CreateChapterIssue(id, ChapterIssueTypeEnum.BadPageNaming);
    }

    private static void CheckMissingPages(long id, string[] chapterFiles)
    {
        Array.Sort(chapterFiles);
        Regex regex = RegexNumber();
        string last = Path.GetFileName(chapterFiles[^1]);
        Match match = regex.Match(last);
        if (!match.Success) return;
        if (Convert.ToInt32(match.Value) == chapterFiles.Length) return;
        IssueManagerService.CreateChapterIssue(id, ChapterIssueTypeEnum.MissingPage);
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
                    IssueManagerService.CreateChapterIssue(id, ChapterIssueTypeEnum.ImageTooBig);
                    break;
                case < 720:
                    IssueManagerService.CreateChapterIssue(id, ChapterIssueTypeEnum.ImageTooSmall);
                    break;
            }
        }
        catch
        {
            IssueManagerService.CreateChapterIssue(id, ChapterIssueTypeEnum.CouldNotOpen);
        }
    }

    private static void CheckChapterFilesAreWebp(long id, string[] chapterFiles)
    {
        if (chapterFiles.Any(file => !Path.GetFileName(file).EndsWith(".webp")))
            IssueManagerService.CreateChapterIssue(id, ChapterIssueTypeEnum.CouldNotOpen);
    }
}