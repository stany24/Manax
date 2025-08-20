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
        manaxContext.SaveChanges();

        CheckMissingChapters(serie);
        CheckDescription(serie);
    }

    private void CheckDescription(Serie serie)
    {
        uint max = SettingsManager.Data.MaxDescriptionLength;
        uint min = SettingsManager.Data.MinDescriptionLength;
        issueService.ManageSerieIssue(serie.Id, AutomaticIssueSerieType.DescriptionTooLong,
            serie.Description.Length > max);
        issueService.ManageSerieIssue(serie.Id, AutomaticIssueSerieType.DescriptionTooShort,
            serie.Description.Length < min);
    }

    private void CheckMissingChapters(Serie serie)
    {
        string[] chapters = Directory.GetFiles(serie.SavePath);
        issueService.ManageSerieIssue(serie.Id, AutomaticIssueSerieType.MissingChapter, chapters.Length == 0);
        if (chapters.Length == 0) return;

        Array.Sort(chapters);
        Regex regex = RegexNumber();
        string last = Path.GetFileName(chapters[^1]);
        Match match = regex.Match(last);
        if (!match.Success) return;
        issueService.ManageSerieIssue(serie.Id, AutomaticIssueSerieType.MissingChapter,
            chapters.Length != Convert.ToInt32(match.Value, CultureInfo.InvariantCulture));
    }
}