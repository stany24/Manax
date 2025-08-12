using System.IO.Compression;
using System.Text.RegularExpressions;
using ImageMagick;
using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Setting;
using ManaxServer.Models;
using ManaxServer.Models.Chapter;
using ManaxServer.Models.Serie;
using ManaxServer.Services.Issue;
using ManaxServer.Settings;

namespace ManaxServer.Services.Fix;

public partial class FixService(IServiceScopeFactory scopeFactory, IIssueService issueService) : Service, IFixService
{
    [GeneratedRegex("\\d{1,4}")]
    private partial Regex RegexNumber();

    public void FixSerie(long serieId)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        Serie? serie = manaxContext.Series.Find(serieId);
        if (serie == null) return;
        manaxContext.SaveChanges();

        CheckMissingChapters(serie);
        FixDescription(serie);
    }

    private void FixDescription(Serie serie)
    {
        uint max = SettingsManager.Data.MaxDescriptionLength;
        uint min = SettingsManager.Data.MinDescriptionLength;
        issueService.ManageSerieIssue(serie.Id, AutomaticIssueSerieType.DescriptionTooLong, serie.Description.Length > max);
        issueService.ManageSerieIssue(serie.Id, AutomaticIssueSerieType.DescriptionTooShort, serie.Description.Length < min);
    }

    public void FixPoster(long serieId)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        Serie? serie = manaxContext.Series.Find(serieId);
        if (serie == null) { return; }

        string directory = serie.Path;
        string fileName = SettingsManager.Data.PosterName +"."+ SettingsManager.Data.ImageFormat.ToString().ToLower();
        string posterPath = Path.Combine(directory, fileName);
        issueService.ManageSerieIssue(serie.Id, AutomaticIssueSerieType.PosterMissing,!File.Exists(posterPath));
        if (!File.Exists(posterPath)) { return; }
        
        uint min = SettingsManager.Data.MinPosterWidth;
        uint max = SettingsManager.Data.MaxPosterWidth;
        try
        {
            using MagickImage poster = new(posterPath);
            issueService.RemoveSerieIssue(serieId, AutomaticIssueSerieType.PosterCouldNotOpen);
            issueService.ManageSerieIssue(serieId, AutomaticIssueSerieType.PosterTooSmall,poster.Width < min);

            if (poster.Width <= max) return;
            poster.Resize(max,poster.Height * max / poster.Width);
            poster.Write(posterPath);
        }
        catch (Exception)
        {
            issueService.CreateSerieIssue(serieId, AutomaticIssueSerieType.PosterCouldNotOpen);
        }
    }

    private void CheckMissingChapters(Serie serie)
    {
        string[] chapters = Directory.GetFiles(serie.Path);
        issueService.ManageSerieIssue(serie.Id, AutomaticIssueSerieType.MissingChapter, chapters.Length == 0);
        if (chapters.Length == 0) { return; }

        Array.Sort(chapters);
        Regex regex = RegexNumber();
        string last = Path.GetFileName(chapters[^1]);
        Match match = regex.Match(last);
        if (!match.Success) return;
        issueService.ManageSerieIssue(serie.Id, AutomaticIssueSerieType.MissingChapter, chapters.Length != Convert.ToInt32(match.Value));
    }

    public void FixChapter(long chapterId)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        Chapter? chapter = manaxContext.Chapters.Find(chapterId);
        if (chapter == null) return;

        chapter.Pages = ZipFile.OpenRead(chapter.Path).Entries.Count;

        FixChapterDeep(chapter);
    }

    private void FixChapterDeep(Chapter chapter)
    {
        string copyName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        
        try
        {
            ZipFile.ExtractToDirectory(chapter.Path, copyName);
        }
        catch
        {
            if (Directory.Exists(copyName)) Directory.Delete(copyName, true);
            issueService.CreateChapterIssue(chapter.Id, AutomaticIssueChapterType.CouldNotOpen);
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

    private MagickImage?[] LoadImages(long chapterId,string[] files)
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
                issueService.CreateChapterIssue(chapterId, AutomaticIssueChapterType.CouldNotOpen);
            }
        }

        return images;
    }

    private bool FixPagesNaming(MagickImage?[] images)
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

    private bool CheckWidthOfChapter(long id, MagickImage?[] images)
    {
        bool modified = false;
        foreach (MagickImage? image in images)
        {
            try
            {
                if (image == null) continue;
                issueService.RemoveChapterIssue(id, AutomaticIssueChapterType.CouldNotOpen);
                uint min = SettingsManager.Data.MinChapterWidth;
                uint max = SettingsManager.Data.MaxChapterWidth;
                issueService.ManageChapterIssue(id, AutomaticIssueChapterType.ImageTooSmall,image.Width < min);
                if (image.Width <= max) continue;
                image.Resize(max,image.Height * max / image.Width);
                image.Write(image.FileName!);
                modified = true;
            }
            catch
            {
                issueService.CreateChapterIssue(id, AutomaticIssueChapterType.CouldNotOpen);
            }
        }
        return modified;
    }

    private bool FixChapterFilesFormat(MagickImage?[] images)
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
    
    private MagickFormat GetMagickFormat(ImageFormat format)
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