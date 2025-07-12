namespace ManaxApi.Settings;

public class SettingsData
{
    public uint MinPosterWidth { get; private set; } = 720;
    public uint MaxPosterWidth { get; private set; } = 800;
    public uint MinChapterWidth { get; private set; } = 720;
    public uint MaxChapterWidth { get; private set; } = 800;
    public uint MaxDescriptionLength { get; private set; } = 1000;
    public uint MinDescriptionLength { get; private set; } = 100;
    public uint ImageQuality { get; private set; } = 94;
    public uint PosterQuality { get; private set; } = 94;
    public ImageFormat PosterFormat { get; private set; } = ImageFormat.Webp;
    public ImageFormat ImageFormat { get; private set; } = ImageFormat.Webp;
    public ArchiveFormat ArchiveFormat { get; private set; } = ArchiveFormat.Cbz;
}