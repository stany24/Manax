using System.Globalization;
using ImageMagick;
using ManaxLibrary.DTO.Issue.Automatic;
using ManaxServer.Models;
using ManaxServer.Models.Serie;
using ManaxServer.Settings;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Services.Fix;

public partial class FixService
{
    public void FixPoster(long serieId)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        Serie? serie = manaxContext.Series
            .Include(s => s.SavePoint)
            .FirstOrDefault(s => s.Id == serieId);
        if (serie == null) return;

        string directory = serie.SavePath;
        string fileName = SettingsManager.Data.PosterName + "." +
                          SettingsManager.Data.ImageFormat.ToString().ToLower(CultureInfo.InvariantCulture);
        string posterPath = Path.Combine(directory, fileName);
        issueService.ManageSerieIssue(serie.Id, AutomaticIssueSerieType.PosterMissing, !File.Exists(posterPath));
        if (!File.Exists(posterPath)) return;

        uint min = SettingsManager.Data.MinPosterWidth;
        uint max = SettingsManager.Data.MaxPosterWidth;
        try
        {
            using MagickImage poster = new(posterPath);
            issueService.RemoveSerieIssue(serieId, AutomaticIssueSerieType.PosterCouldNotOpen);
            issueService.ManageSerieIssue(serieId, AutomaticIssueSerieType.PosterTooSmall, poster.Width < min);

            if (poster.Width <= max) return;
            poster.Resize(max, poster.Height * max / poster.Width);
            poster.Write(posterPath);
        }
        catch (Exception)
        {
            issueService.CreateSerieIssue(serieId, AutomaticIssueSerieType.PosterCouldNotOpen);
        }
    }
}