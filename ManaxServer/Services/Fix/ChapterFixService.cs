using System.IO.Compression;
using System.Text.RegularExpressions;
using ImageMagick;
using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Setting;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Services.Issue;
using ManaxServer.Settings;

namespace ManaxServer.Services.Fix;

public partial class FixService(IServiceScopeFactory scopeFactory, IIssueService issueService) : Service, IFixService
{
    [GeneratedRegex("\\d{1,4}")]
    private partial Regex RegexNumber();
    
    private readonly string[] _chapterNumberPatterns =
    [
        "CH\\d{1,4}",
        "(?i)chapter[-_ ]\\d{1,4}",
        "(?i)episode[-_ ][-_ ]\\d{1,4}",
        "(?i)episode[-_ ]\\d{1,4}",
        "(?i)chap[-_ ]\\d{1,4}",
        "(?i)ch.[-_ ]*\\d{1,4}",
        "(?i)ep.[-_ ]*\\d{1,4}",
        "(?i)Flight[-_ ]\\d{1,4}",
        "\\d{1,4}"
    ];
    
    public void FixChapter(long chapterId)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        Chapter? chapter = manaxContext.Chapters.Find(chapterId);
        if (chapter == null) return;

        chapter.Pages = ZipFile.OpenRead(chapter.Path).Entries.Count;

        FixChapterDeep(chapter);
        manaxContext.SaveChangesAsync();
    }

    private void FixChapterDeep(Chapter chapter)
    {
        string copyName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

        try
        {
            ZipFile.ExtractToDirectory(chapter.Path, copyName);
            List<string> list = Directory.GetFiles(copyName, "*.*", SearchOption.AllDirectories).ToList();
            foreach (string file in list)
            {
                File.Move(file, Path.Combine(copyName, Path.GetFileName(file)));
            }
            foreach (string directory in Directory.GetDirectories(copyName))
            {
                Directory.Delete(directory);
            }
        }
        catch
        {
            if (Directory.Exists(copyName)) Directory.Delete(copyName, true);
            issueService.CreateChapterIssue(chapter.Id, AutomaticIssueChapterType.CouldNotOpen);
            return;
        }

        string[] files = Directory.GetFiles(copyName);
        Array.Sort(files);
        MagickImage?[] images = LoadImages(chapter.Id, files);
        bool modified1 = FixWidthOfChapter(chapter.Id, images);
        bool modified2 = FixChapterFilesFormat(images);
        bool modified3 = FixPagesNaming(images);
        bool modified4 = FixChapterName(chapter);
        if (modified1 || modified2 || modified3 || modified4)
        {
            File.Delete(chapter.Path);
            ZipFile.CreateFromDirectory(copyName, chapter.Path);
        }

        Directory.Delete(copyName, true);
    }

    private bool FixChapterName(Chapter chapter)
    {
        int? chapterNumber = GetChapterNumber(chapter.FileName);
        if (chapterNumber != null) return ChangeChapterName(chapter, (int)chapterNumber);
        issueService.CreateChapterIssue(chapter.Id, AutomaticIssueChapterType.ChapterNumberMissing);
        return false;
    }
    
    private static bool ChangeChapterName(Chapter chapter, int number)
    {
        string newName = Path.GetDirectoryName(chapter.Path) + Path.DirectorySeparatorChar +
                         $"CH{number:0000}."+SettingsManager.Data.ArchiveFormat.ToString().ToLower();
        if (newName == chapter.Path) {return false;}
        Directory.Move(chapter.Path, newName);
        chapter.Path = newName;
        chapter.FileName = Path.GetFileName(newName);
        return true;
    }
    
    private int? GetChapterNumber(string fullChapterPath)
    {
        string folderName = Path.GetFileName(fullChapterPath);
        foreach (string pattern in _chapterNumberPatterns)
        {
            Regex regex = new(pattern);
            Match match = regex.Match(folderName);
            if (match.Success) return GetNumber(match.Value);
        }

        return null;
    }
    
    private int GetNumber(string chapterName)
    {
        Regex regex = RegexNumber();
        Match match = regex.Match(chapterName);
        return Convert.ToInt32(match.Value);
    }

    private MagickImage?[] LoadImages(long chapterId, string[] files)
    {
        MagickImage?[] images = new MagickImage[files.Length];
        for (int i = 0; i < files.Length; i++)
            try
            {
                images[i] = new MagickImage(files[i]);
            }
            catch
            {
                images[i] = null;
                issueService.CreateChapterIssue(chapterId, AutomaticIssueChapterType.CouldNotOpen);
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
                issueService.RemoveChapterIssue(id, AutomaticIssueChapterType.CouldNotOpen);
                uint min = SettingsManager.Data.MinChapterWidth;
                uint max = SettingsManager.Data.MaxChapterWidth;
                issueService.ManageChapterIssue(id, AutomaticIssueChapterType.ImageTooSmall, image.Width < min);
                if (image.Width <= max) continue;
                image.Resize(max, image.Height * max / image.Width);
                image.Write(image.FileName!);
                modified = true;
            }
            catch
            {
                issueService.CreateChapterIssue(id, AutomaticIssueChapterType.CouldNotOpen);
            }

        return modified;
    }

    private static bool FixChapterFilesFormat(MagickImage?[] images)
    {
        bool modified = false;
        MagickFormat format = GetMagickFormat(SettingsManager.Data.ImageFormat);
        foreach (MagickImage? image in images)
        {
            if (image == null) continue;
            if (image.Format == format) continue;
            image.Format = format;
            string newPath = Path.Combine(Path.GetDirectoryName(image.FileName) ?? string.Empty,
                $"{Path.GetFileNameWithoutExtension(image.FileName)}.{format.ToString().ToLower()}");
            File.Delete(image.FileName!);
            image.Write(newPath);
            modified = true;
        }

        return modified;
    }

    private static MagickFormat GetMagickFormat(ImageFormat format)
    {
        return format switch
        {
            ImageFormat.Webp => MagickFormat.WebP,
            ImageFormat.Png => MagickFormat.Png,
            ImageFormat.Jpeg => MagickFormat.Jpeg,
            _ => MagickFormat.WebP
        };
    }
}