namespace ManaxLibrary.DTO.Setting;

public class SettingsDataDto
{
    public uint MaxPosterWidth { get; init; } = 800;
    public uint MinPosterWidth { get; init; } = 720;
    public uint MaxChapterWidth { get; init; } = 800;
    public uint MinChapterWidth { get; init; } = 720;
    public uint MaxDescriptionLength { get; init; } = 1000;
    public uint MinDescriptionLength { get; init; } = 100;
    public uint ImageQuality { get; init; } = 94;
    public uint PosterQuality { get; init; } = 94;
    public ImageFormat PosterFormat { get; init; } = ImageFormat.Webp;
    public uint BannerQuality { get; init; } = 94;
    public ImageFormat BannerFormat { get; init; } = ImageFormat.Webp;
    public ImageFormat ImageFormat { get; init; } = ImageFormat.Webp;
    public ArchiveFormat ArchiveFormat { get; init; } = ArchiveFormat.Cbz;
    
    public string? Validate()
    {
        if (MaxPosterWidth < MinPosterWidth)
        {
            return "MaxPosterWidth cannot be less than MinPosterWidth.";
        }

        if (MaxChapterWidth < MinChapterWidth)
        {
            return "MaxChapterWidth cannot be less than MinChapterWidth.";
        }

        if (MaxDescriptionLength < MinDescriptionLength)
        {
            return "MaxDescriptionLength cannot be less than MinDescriptionLength.";
        }

        if (ImageQuality is < 1 or > 100)
        {
            return "ImageQuality must be between 1 and 100.";
        }

        if (PosterQuality is < 1 or > 100)
        {
            return "PosterQuality must be between 1 and 100.";
        }

        return null;
    }
}