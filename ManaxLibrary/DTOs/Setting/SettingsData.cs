namespace ManaxLibrary.DTOs.Setting;

public class SettingsData
{
    public uint MaxPosterWidth { get; private set; } = 800;
    public uint MinPosterWidth { get; private set; } = 720;
    public uint MaxChapterWidth { get; private set; } = 800;
    public uint MinChapterWidth { get; private set; } = 720;
    public uint MaxDescriptionLength { get; private set; } = 1000;
    public uint MinDescriptionLength { get; private set; } = 100;
    public uint ImageQuality { get; private set; } = 94;
    
    public string PosterName { get; private set; } = "poster";
    public uint PosterQuality { get; private set; } = 94;
    public ImageFormat PosterFormat { get; private set; } = ImageFormat.Webp;
    public ImageFormat ImageFormat { get; private set; } = ImageFormat.Webp;
    public ArchiveFormat ArchiveFormat { get; private set; } = ArchiveFormat.Cbz;
    
    public bool IsValid => GetIssues() == null;

    public string? GetIssues()
    {
        if (MaxPosterWidth < MinPosterWidth) { return "MaxPosterWidth cannot be less than MinPosterWidth."; }
        if (MaxChapterWidth < MinChapterWidth) { return "MaxChapterWidth cannot be less than MinChapterWidth."; }
        if (MaxDescriptionLength < MinDescriptionLength) { return "MaxDescriptionLength cannot be less than MinDescriptionLength."; }
        if (ImageQuality is < 1 or > 100) { return "ImageQuality must be between 1 and 100."; }
        if (PosterQuality is < 1 or > 100) { return "PosterQuality must be between 1 and 100."; }

        return null;
    }
}