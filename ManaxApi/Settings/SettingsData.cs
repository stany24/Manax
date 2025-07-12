namespace ManaxApi.Settings;

public class SettingsData
{
    public int MinPosterWidth { get; private set; } = 720;
    public int MaxPosterWidth { get; private set; } = 800;
    public int MinChapterWidth { get; private set; } = 720;
    public int MaxChapterWidth { get; private set; } = 800;
    public ImageFormat ImageFormat { get; private set; } = ImageFormat.Webp;
    public ArchiveFormat ArchiveFormat { get; private set; } = ArchiveFormat.Cbz;
}