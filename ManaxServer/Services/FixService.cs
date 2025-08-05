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

public static partial class FixService
{
    private static IServiceScopeFactory _scopeFactory = null!;

    [GeneratedRegex("\\d{1,4}")]
    private static partial Regex RegexNumber();

    public static void Initialize(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public static void FixSerie(long serieId)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        Serie? serie = manaxContext.Series.Find(serieId);
        if (serie == null) return;
        manaxContext.SaveChanges();

        CheckMissingChapters(serie);
        FixDescription(serie);
    }

    private static void FixDescription(Serie serie)
    {
        uint max = SettingsManager.Data.MaxDescriptionLength;
        uint min = SettingsManager.Data.MinDescriptionLength;
        IssueManagerService.ManageSerieIssue(serie.Id, AutomaticIssueSerieType.DescriptionTooLong, serie.Description.Length > max);
        IssueManagerService.ManageSerieIssue(serie.Id, AutomaticIssueSerieType.DescriptionTooShort, serie.Description.Length < min);
    }

    public static void FixPoster(long serieId)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        Serie? serie = manaxContext.Series.Find(serieId);
        if (serie == null) { return; }

        string directory = serie.Path;
        string fileName = SettingsManager.Data.PosterName +"."+ SettingsManager.Data.ImageFormat.ToString().ToLower();
        string posterPath = Path.Combine(directory, fileName);
        IssueManagerService.ManageSerieIssue(serie.Id, AutomaticIssueSerieType.PosterMissing,!File.Exists(posterPath));
        if (!File.Exists(posterPath)) { return; }
        
        uint min = SettingsManager.Data.MinPosterWidth;
        uint max = SettingsManager.Data.MaxPosterWidth;
        try
        {
            using MagickImage poster = new(posterPath);
            IssueManagerService.RemoveSerieIssue(serieId, AutomaticIssueSerieType.PosterCouldNotOpen);
            IssueManagerService.ManageSerieIssue(serieId, AutomaticIssueSerieType.PosterTooSmall,poster.Width < min);

            if (poster.Width > max)
            {
                poster.Resize(max,poster.Height * max / poster.Width);
                poster.Write(posterPath);
            }
        }
        catch (Exception)
        {
            IssueManagerService.CreateSerieIssue(serieId, AutomaticIssueSerieType.PosterCouldNotOpen);
        }
    }

    private static void CheckMissingChapters(Serie serie)
    {
        string[] chapters = Directory.GetFiles(serie.Path);
        IssueManagerService.ManageSerieIssue(serie.Id, AutomaticIssueSerieType.MissingChapter, chapters.Length == 0);
        if (chapters.Length == 0) { return; }

        Array.Sort(chapters);
        Regex regex = RegexNumber();
        string last = Path.GetFileName(chapters[^1]);
        Match match = regex.Match(last);
        if (!match.Success) return;
        IssueManagerService.ManageSerieIssue(serie.Id, AutomaticIssueSerieType.MissingChapter, chapters.Length != Convert.ToInt32(match.Value));
    }

    public static void FixChapter(long chapterId)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        Chapter? chapter = manaxContext.Chapters.Find(chapterId);
        if (chapter == null) return;

        chapter.Pages = ZipFile.OpenRead(chapter.Path).Entries.Count;

        FixChapterDeep(chapter);
    }

    private static void FixChapterDeep(Chapter chapter)
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
        Array.Sort(files);
        MagickImage?[] images = LoadImages(chapter.Id,files);
        bool modified = CheckWidthOfChapter(chapter.Id, images);
        modified = modified || FixChapterFilesFormat(images);
        modified = modified || FixPagesNaming(images);
        if (modified)
        {
            File.Delete(chapter.Path);
            ZipFile.CreateFromDirectory(copyName, chapter.Path);
        }
        Directory.Delete(copyName, true);
    }

    private static MagickImage?[] LoadImages(long chapterId,string[] files)
    {
        MagickImage?[] images = new MagickImage[files.Length];
        for (int i = 0; i < files.Length; i++)
        {
            try
            {
                images[i] = new MagickImage(files[i]);
            }
            catch
            {
                images[i] = null;
                IssueManagerService.CreateChapterIssue(chapterId, AutomaticIssueChapterType.CouldNotOpen);
            }
        }

        return images;
    }

    private static bool FixPagesNaming(MagickImage?[] images)
    {
        bool modified = false;
        string? directory = null;
        foreach (MagickImage? image in images)
        {
            if (image == null) { continue; }
            directory = Path.GetDirectoryName(image.FileName);
            if (directory != null) { break; }
        }
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] == null) continue;
            string newPath = directory + Path.DirectorySeparatorChar + $"P{i + 1:000}" + Path.GetExtension(images[i]!.FileName);
            if (newPath == images[i]!.FileName) continue;
            File.Move(images[i]!.FileName!, newPath);
            images[i] = new MagickImage(newPath);
            modified = true;
        }

        return modified;
    }

    private static bool CheckWidthOfChapter(long id, MagickImage?[] images)
    {
        bool modified = false;
        foreach (MagickImage? image in images)
        {
            try
            {
                if (image == null) continue;
                IssueManagerService.RemoveChapterIssue(id, AutomaticIssueChapterType.CouldNotOpen);
                uint min = SettingsManager.Data.MinChapterWidth;
                uint max = SettingsManager.Data.MaxChapterWidth;
                IssueManagerService.ManageChapterIssue(id, AutomaticIssueChapterType.ImageTooSmall,image.Width < min);
                if (image.Width > max)
                {
                    image.Resize(max,image.Height * max / image.Width);
                    image.Write(image.FileName!);
                    modified = true;
                }
            }
            catch
            {
                IssueManagerService.CreateChapterIssue(id, AutomaticIssueChapterType.CouldNotOpen);
            }
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
            if(image.Format == format) continue;
            image.Format = format;
            string newPath = Path.Combine(Path.GetDirectoryName(image.FileName) ?? string.Empty, $"{Path.GetFileNameWithoutExtension(image.FileName)}.{format.ToString().ToLower()}");
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