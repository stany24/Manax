using System.Globalization;
using ImageMagick;
using ManaxLibrary.DTO.Setting;
using ManaxServer.Models;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Services.Renaming;

public class RenamingService(IServiceScopeFactory scopeFactory) : Service, IRenamingService
{
    public void RenameChapters()
    {
    }

    public void RenamePosters(string oldName, string newName, ImageFormat oldFormat, ImageFormat newFormat)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        ManaxContext context = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        context.Series
            .Include(s => s.SavePoint)
            .ToList()
            .ForEach(serie =>
            {
                string oldPoster = Path.Combine(serie.SavePath,
                    $"{oldName}.{oldFormat.ToString().ToLower(CultureInfo.InvariantCulture)}");
                string newPoster = Path.Combine(serie.SavePath,
                    $"{newName}.{newFormat.ToString().ToLower(CultureInfo.InvariantCulture)}");

                if (!File.Exists(oldPoster)) return;

                if (newFormat == oldFormat)
                {
                    File.Move(oldPoster, newPoster);
                }
                else
                {
                    MagickImage image = new(oldPoster);
                    image.Format = newFormat.GetMagickFormat();
                    image.Write(newPoster);
                    File.Delete(oldPoster);
                }
            });
    }
}