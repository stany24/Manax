using ManaxApi.Models.Chapter;
using ManaxApi.Models.Library;
using ManaxApi.Models.Serie;
using ManaxApi.Task;
using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Services;

public class ScanService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ScanService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void ScanLibrary(Library library)
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
                    FolderName = folderName,
                    Title = folderName,
                    Description = string.Empty,
                    Path = directory
                };
                manaxContext.Series.Add(serie);
                manaxContext.SaveChanges();
            }
            
            TaskManagerService.AddTask(new SerieScanTask(_scopeFactory, serie.Id));
        }
    }

    public void ScanSerie(long serieId)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        
        Serie? serie = manaxContext.Series.Find(serieId);
        if (serie == null) return;
        
        Console.WriteLine("Scanning serie: " + serie.Title);
        string[] files = Directory.GetFiles(serie.Path);

        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            Chapter? chapter = manaxContext.Series
                .Include(s => s.Chapters)
                .FirstOrDefault(s => s.FolderName == serie.FolderName)?
                .Chapters.FirstOrDefault(s => s.FileName == fileName);

            if (chapter == null)
            {
                chapter = new Chapter
                {
                    FileName = Path.GetFileName(file),
                    Path = file,
                    Pages = 0,
                    Number = 0
                };
                manaxContext.Chapters.Add(chapter);
                serie.Chapters.Add(chapter);
                manaxContext.SaveChanges();
            }
            
            TaskManagerService.AddTask(new ChapterScanTask(_scopeFactory, chapter.Id));
        }
    }

    public void ScanChapter(long chapterId)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        
        Chapter? chapter = manaxContext.Chapters.Find(chapterId);
        if (chapter == null) return;
        
        Console.WriteLine("Scanning chapter: " + chapter.FileName);
        // Logique de scan de chapitre ici
    }
}