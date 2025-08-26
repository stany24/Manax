using System.Globalization;
using ImageMagick;
using ManaxLibrary.DTO.Setting;
using ManaxServer.Models;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Services.Renaming;

public class RenamingService : Service
{
    public void RenameChapters()
    {
    }

    public static void RenamePosters(ManaxContext manaxContext,string oldName, string newName, ImageFormat oldFormat, ImageFormat newFormat)
    {
        manaxContext.Series
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
                    image.Format = GetMagickFormat(newFormat);
                    image.Write(newPoster);
                    File.Delete(oldPoster);
                }
            });
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