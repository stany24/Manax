using ImageMagick;
using ManaxLibrary.DTO.Setting;
using ManaxServer.Models;

namespace ManaxServer.Services.Renaming;

public class RenamingService(IServiceScopeFactory scopeFactory) : Service, IRenamingService
{
    public void RenameChapters()
    {
        
    }

    public void RenamePosters(string oldName, string newName, ImageFormat oldFormat, ImageFormat newFormat)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        ManaxContext manaxContext = scope.ServiceProvider.GetRequiredService<ManaxContext>();
        manaxContext.Series
            .Select(serie =>  serie.Path)
            .ToList()
            .ForEach(path =>
            {
                string oldPoster = Path.Combine(path, $"{oldName}.{oldFormat.ToString().ToLower()}");
                string newPoster = Path.Combine(path, $"{newName}.{newFormat.ToString().ToLower()}");
                
                if (!File.Exists(oldPoster)) { return; }

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