using System.Globalization;
using System.IO.Compression;
using System.Text.RegularExpressions;
using ImageMagick;
using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Setting;
using ManaxLibrary.Logging;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Models.Serie;
using ManaxServer.Services.Issue;
using ManaxServer.Services.Notification;
using ManaxServer.Settings;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Services.Fix;

public partial class FixService(IServiceScopeFactory scopeFactory, IIssueService issueService,INotificationService notificationService) : Service, IFixService
{
    public void ReplaceChapter(long oldChapterId, NewChapter newChapter)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        
        Serie? serie = manaxContext.Series
            .Include(s => s.SavePoint)
            .FirstOrDefault(s => s.Id == newChapter.SerieId);
        Chapter? chapter = manaxContext.Chapters.Find(oldChapterId);
        
        if (serie == null || chapter == null)
        {
            Logger.LogFailure($"Serie with id {newChapter.SerieId} not found for chapter {newChapter.Number}");
            return;
        }
        
        bool success = FixChapterDeep(newChapter,chapter);
        if (!success)
        {
            notificationService.NotifyChapterUploadFailedAsync(newChapter.UploaderId,serie.Title, newChapter.Number);
            return;
        }
        
        chapter.PageNumber = ZipFile.OpenRead(chapter.Path()).Entries.Count;
        serie.LastModification = DateTime.UtcNow;
        chapter.LastModification = DateTime.UtcNow;
        manaxContext.SaveChanges();
        notificationService.NotifyChapterUpdatedAsync(chapter.ToDto());
    }

    public void FixNewChapter(NewChapter newChapter)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();

        Serie? serie = manaxContext.Series
            .Include(s => s.SavePoint)
            .FirstOrDefault(s => s.Id == newChapter.SerieId);
        
        if (serie == null)
        {
            Logger.LogFailure($"Serie with id {newChapter.SerieId} not found for chapter {newChapter.Number}");
            return;
        }
        
        Chapter chapter = new()
        {
            SerieId = newChapter.SerieId,
            Serie = serie,
            UploaderId = newChapter.UploaderId,
            Number = newChapter.Number,
            Creation = DateTime.UtcNow,
            LastModification = DateTime.UtcNow,
            PageNumber = ZipFile.OpenRead(newChapter.TempPath).Entries.Count
        };
        
        bool success = FixChapterDeep(newChapter,chapter);
        if (!success)
        {
            notificationService.NotifyChapterUploadFailedAsync(newChapter.UploaderId,serie.Title, newChapter.Number);
            return;
        }
        
        serie.LastModification = DateTime.UtcNow;
        manaxContext.Chapters.Add(chapter);
        manaxContext.SaveChanges();
        notificationService.NotifyChapterAddedAsync(chapter.ToDto());
    }

    [GeneratedRegex("\\d{1,4}")]
    private partial Regex RegexNumber();

    private bool FixChapterDeep(NewChapter newChapter, Chapter chapter)
    {
        string extractedPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            ZipFile.ExtractToDirectory(newChapter.TempPath, extractedPath);
            List<string> allExtractedFiles = Directory.GetFiles(extractedPath, "*.*", SearchOption.AllDirectories).ToList();
            foreach (string file in allExtractedFiles) File.Move(file, Path.Combine(extractedPath, Path.GetFileName(file)));
            foreach (string directory in Directory.GetDirectories(extractedPath)) Directory.Delete(directory);
        }
        catch(Exception e)
        {
            if (Directory.Exists(extractedPath)) Directory.Delete(extractedPath, true);
            Logger.LogError("Failed to extract chapter archive",e);
            return false;
        }

        string[] files = Directory.GetFiles(extractedPath);
        Array.Sort(files);
        MagickImage[]? images = LoadImages(files);
        if (images == null)
        {
            Directory.Delete(extractedPath, true);
            Logger.LogFailure($"Failed to load images for chapter {newChapter.Number} in serie {newChapter.SerieId}");
            return false;
        }
        
        bool modified = false;
        modified = modified || FixWidthOfChapter(chapter.Id, images);
        modified = modified || FixChapterFilesFormat(images);
        modified = modified || FixPagesNaming(images);
        
        if(File.Exists(chapter.Path())) {File.Delete(chapter.Path());}
        if (modified) { ZipFile.CreateFromDirectory(extractedPath, chapter.Path()); }
        else { File.Move(newChapter.TempPath, chapter.Path()); }
        
        File.Delete(newChapter.TempPath);
        Directory.Delete(extractedPath,true);

        foreach (MagickImage image in images) image.Dispose();
        return true;
    }

    private static MagickImage[]? LoadImages(string[] files)
    {
        MagickImage[] images = new MagickImage[files.Length];
        for (int i = 0; i < files.Length; i++)
            try
            {
                images[i] = new MagickImage(files[i]);
            }
            catch
            {
                return null;
            }

        return images;
    }

    private static bool FixPagesNaming(MagickImage?[] images)
    {
        bool modified = false;
        string? directory = null;
        foreach (MagickImage? image in images)
        {
            if (image == null) continue;
            directory = Path.GetDirectoryName(image.FileName);
            if (directory != null) break;
        }

        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] == null) continue;
            string newPath = directory + Path.DirectorySeparatorChar + $"P{i + 1:000}" +
                             Path.GetExtension(images[i]!.FileName);
            if (newPath == images[i]!.FileName) continue;
            File.Move(images[i]!.FileName!, newPath);
            images[i]?.Dispose();
            images[i] = new MagickImage(newPath);
            modified = true;
        }

        return modified;
    }

    private bool FixWidthOfChapter(long id, MagickImage?[] images)
    {
        bool modified = false;
        foreach (MagickImage? image in images)
            try
            {
                if (image == null) continue;
                issueService.RemoveChapterIssue(id, IssueChapterAutomaticType.CouldNotOpen);
                uint min = SettingsManager.Data.MinChapterWidth;
                uint max = SettingsManager.Data.MaxChapterWidth;
                issueService.ManageChapterIssue(id, IssueChapterAutomaticType.ImageTooSmall, image.Width < min);
                if (image.Width <= max) continue;
                image.Resize(max, image.Height * max / image.Width);
                image.Write(image.FileName!);
                modified = true;
            }
            catch
            {
                issueService.CreateChapterIssue(id, IssueChapterAutomaticType.CouldNotOpen);
            }

        return modified;
    }

    private static bool FixChapterFilesFormat(MagickImage?[] images)
    {
        bool modified = false;
        MagickFormat format = SettingsManager.Data.ImageFormat.GetMagickFormat();
        foreach (MagickImage? image in images)
        {
            if (image == null) continue;
            if (image.Format == format) continue;
            image.Format = format;
            string newPath = Path.Combine(Path.GetDirectoryName(image.FileName) ?? string.Empty,
                $"{Path.GetFileNameWithoutExtension(image.FileName)}.{format.ToString().ToLower(CultureInfo.InvariantCulture)}");
            File.Delete(image.FileName!);
            image.Write(newPath);
            modified = true;
        }

        return modified;
    }
}