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

public partial class FixService(
    IServiceScopeFactory scopeFactory,
    IIssueService issueService,
    INotificationService notificationService) : Service, IFixService
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

        ZipArchive archive = new(new MemoryStream(newChapter.Data));
        ZipArchiveEntry[] entries = archive.Entries.ToArray();
        Array.Sort(entries, (a, b) => string.Compare(b.FullName, a.FullName, StringComparison.Ordinal));
        MagickImage[] images = new MagickImage[archive.Entries.Count];

        for (int i = 0; i < entries.Length; i++)
            try
            {
                MagickImage magickImage = new(entries[i].Open());
                images[i] = magickImage;
            }
            catch (Exception)
            {
                notificationService.NotifyChapterUploadFailedAsync(newChapter.UploaderId, serie.Title,
                    newChapter.Number);
                return;
            }

        FixWidthOfChapter(chapter.Id, images);
        FixChapterFilesFormat(images);

        File.Delete(chapter.Path());
        using (FileStream fs = new(chapter.Path(), FileMode.Create, FileAccess.Write, FileShare.None))
        using (ZipArchive chapterFile = new(fs, ZipArchiveMode.Create))
        {
            for (int i = 0; i < images.Length; i++)
            {
                ZipArchiveEntry zipArchiveEntry = chapterFile.CreateEntry(i + ".webp");
                using Stream stream = zipArchiveEntry.Open();
                byte[] data = images[i].ToByteArray();
                stream.Write(data, 0, data.Length);
            }
        }

        chapter.PageNumber = Convert.ToUInt32(images.Length);
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

        ZipArchive archive = new(new MemoryStream(newChapter.Data));
        Chapter chapter = Chapter.FromDto(newChapter);
        chapter.PageNumber = Convert.ToUInt32(archive.Entries.Count);
        chapter.Serie = serie;

        ZipArchiveEntry[] entries = archive.Entries.ToArray();
        Array.Sort(entries, (a, b) => string.Compare(a.FullName, b.FullName, StringComparison.Ordinal));
        MagickImage[] images = new MagickImage[archive.Entries.Count];

        for (int i = 0; i < entries.Length; i++)
            try
            {
                MagickImage magickImage = new(entries[i].Open());
                images[i] = magickImage;
            }
            catch (Exception)
            {
                notificationService.NotifyChapterUploadFailedAsync(newChapter.UploaderId, serie.Title,
                    newChapter.Number);
                return;
            }

        FixWidthOfChapter(chapter.Id, images);
        FixChapterFilesFormat(images);

        using (FileStream fs = new(chapter.Path(), FileMode.Create, FileAccess.Write, FileShare.None))
        using (ZipArchive chapterFile = new(fs, ZipArchiveMode.Create))
        {
            for (int i = 0; i < images.Length; i++)
            {
                ZipArchiveEntry zipArchiveEntry = chapterFile.CreateEntry(i + ".webp");
                using Stream stream = zipArchiveEntry.Open();
                stream.Write(images[i].ToByteArray());
            }
        }

        serie.LastModification = DateTime.UtcNow;
        manaxContext.Chapters.Add(chapter);
        manaxContext.SaveChanges();
        notificationService.NotifyChapterAddedAsync(chapter.ToDto());
    }

    [GeneratedRegex("\\d{1,4}")]
    private partial Regex RegexNumber();

    private void FixWidthOfChapter(long id, MagickImage[] images)
    {
        foreach (MagickImage image in images)
        {
            uint min = SettingsManager.DataDto.MinChapterWidth;
            uint max = SettingsManager.DataDto.MaxChapterWidth;
            issueService.ManageChapterIssue(id, IssueChapterAutomaticType.ImageTooSmall, image.Width < min);
            if (image.Width <= max) continue;
            image.Resize(max, image.Height * max / image.Width);
        }
    }

    private static void FixChapterFilesFormat(MagickImage[] images)
    {
        MagickFormat format = SettingsManager.DataDto.ImageFormat.GetMagickFormat();
        foreach (MagickImage image in images)
        {
            if (image.Format == format) continue;
            image.Format = format;
            string newPath = Path.Combine(Path.GetDirectoryName(image.FileName) ?? string.Empty,
                $"{Path.GetFileNameWithoutExtension(image.FileName)}.{format.ToString().ToLower(CultureInfo.InvariantCulture)}");
            File.Delete(image.FileName!);
            image.Write(newPath);
        }
    }
}