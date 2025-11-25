using System.Globalization;
using System.Text.RegularExpressions;
using ManaxLibrary.DTO.Issue.Automatic;
using ManaxServer.Models;
using ManaxServer.Models.Serie;
using ManaxServer.Settings;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Services.Fix;

public partial class FixService
{
    public void FixSerie(long serieId)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        Serie? serie = manaxContext.Series
            .Include(s => s.SavePoint)
            .FirstOrDefault(s => s.Id == serieId);
        if (serie == null) return;

        string[] chapterPaths =
            manaxContext.Chapters.Where(c => c.SerieId == serie.Id).Select(c => c.Path()).ToArray();

        CheckMissingChapters(chapterPaths, serieId);
        CheckDescription(serie);
    }

    private void CheckDescription(Serie serie)
    {
        uint max = SettingsManager.DataDto.MaxDescriptionLength;
        uint min = SettingsManager.DataDto.MinDescriptionLength;
        issueService.ManageSerieIssue(serie.Id, IssueSerieAutomaticType.DescriptionTooLong,
            serie.Description.Length > max);
        issueService.ManageSerieIssue(serie.Id, IssueSerieAutomaticType.DescriptionTooShort,
            serie.Description.Length < min);
    }

    private void CheckMissingChapters(string[] chapterPaths, long serieId)
    {
        issueService.ManageSerieIssue(serieId, IssueSerieAutomaticType.MissingChapter, chapterPaths.Length == 0);
        if (chapterPaths.Length == 0) return;

        Array.Sort(chapterPaths);
        Regex regex = RegexNumber();
        string last = Path.GetFileName(chapterPaths[^1]);
        Match match = regex.Match(last);
        if (!match.Success) return;
        issueService.ManageSerieIssue(serieId, IssueSerieAutomaticType.MissingChapter,
            chapterPaths.Length != Convert.ToInt32(match.Value, CultureInfo.InvariantCulture));
    }
}