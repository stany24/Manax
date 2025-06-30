using System.IO.Compression;
using ImageMagick;
using ManaxApi.Models;
using ManaxApi.Models.Chapter;
using ManaxApi.Models.Issue;
using ManaxApi.Models.Library;
using ManaxApi.Models.Serie;
using ManaxApi.Task;

namespace ManaxApi.Services;

public static class ScanService
{
    private static IServiceScopeFactory _scopeFactory = null!;
    
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
            Chapter? chapter = manaxContext.Chapters.FirstOrDefault(s => s.FileName == fileName && s.SerieId == serie.Id);

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
        
        string[] files = Directory.GetFiles(serie.Path);
        List<string> posters = files.Where(f => Path.GetFileName(f).StartsWith("poster.", StringComparison.CurrentCultureIgnoreCase)).ToList();
        switch (posters.Count)
        {
            case 0:
                IssueManagerService.CreateSerieIssue(serie.Id, SerieIssueTypeEnum.PosterMissing);
                break;
            case 1:
                string poster = posters.First();
                if (!Path.GetExtension(poster).Equals(".webp", StringComparison.CurrentCultureIgnoreCase))
                {
                    IssueManagerService.CreateSerieIssue(serie.Id, SerieIssueTypeEnum.PosterWrongFormat);
                }
                break;
            case > 1:
                IssueManagerService.CreateSerieIssue(serie.Id, SerieIssueTypeEnum.PosterDuplicate);
                break;
        }
        manaxContext.SaveChanges();
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
        
        manaxContext.ChapterIssues.RemoveRange(manaxContext.ChapterIssues.Where(i => i.ChapterId == chapterId));
        
        if (chapter == null) return;
        ZipArchive archive = ZipFile.OpenRead(chapter.Path);
        
        bool notAllWebp = archive.Entries.Any(t => !t.FullName.EndsWith(".webp", StringComparison.CurrentCultureIgnoreCase));
        if (notAllWebp)
        {
            IssueManagerService.CreateChapterIssue(chapter.Id, ChapterIssueTypeEnum.NotAllWebp);
        }
        
        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            string tempFileName = Path.GetTempFileName();
            entry.ExtractToFile(tempFileName, true);
            MagickImage image = new(tempFileName);
            
            switch (image.Width)
            {
                case < 720:
                    IssueManagerService.CreateChapterIssue(chapter.Id, ChapterIssueTypeEnum.ImageTooSmall);
                    break;
                case > 800:
                    IssueManagerService.CreateChapterIssue(chapter.Id, ChapterIssueTypeEnum.ImageTooBig);
                    break;
            }

            image.Dispose();
            File.Delete(tempFileName);
        }
        manaxContext.SaveChanges();
    }
}