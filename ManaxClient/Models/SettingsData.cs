using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTO.Setting;

namespace ManaxClient.Models;

public partial class SettingsData:ObservableObject
{
    [ObservableProperty] private bool _isValid;
    [ObservableProperty] private string _issue = string.Empty;
    [ObservableProperty] private uint _maxPosterWidth;
    [ObservableProperty] private uint _minPosterWidth;
    [ObservableProperty] private uint _maxChapterWidth;
    [ObservableProperty] private uint _minChapterWidth;
    [ObservableProperty] private uint _maxDescriptionLength;
    [ObservableProperty] private uint _minDescriptionLength;
    [ObservableProperty] private uint _imageQuality;
    [ObservableProperty] private uint _posterQuality;
    [ObservableProperty] private uint _bannerQuality;
    [ObservableProperty] private ImageFormat _posterFormat;
    [ObservableProperty] private ImageFormat _bannerFormat;
    [ObservableProperty] private ImageFormat _imageFormat;
    [ObservableProperty] private ArchiveFormat _archiveFormat;
    
    public static SettingsData FromDto(SettingsDataDto dto)
    {
        return new SettingsData
        {
            MaxPosterWidth = dto.MaxPosterWidth,
            MinPosterWidth = dto.MinPosterWidth,
            MaxChapterWidth = dto.MaxChapterWidth,
            MinChapterWidth = dto.MinChapterWidth,
            MaxDescriptionLength = dto.MaxDescriptionLength,
            MinDescriptionLength = dto.MinDescriptionLength,
            ImageQuality = dto.ImageQuality,
            PosterQuality = dto.PosterQuality,
            PosterFormat = dto.PosterFormat,
            BannerQuality = dto.BannerQuality,
            BannerFormat = dto.BannerFormat,
            ImageFormat = dto.ImageFormat,
            ArchiveFormat = dto.ArchiveFormat
        };
    }

    public SettingsDataDto ToDto()
    {
        return new SettingsDataDto
        {
            MaxPosterWidth = MaxPosterWidth,
            MinPosterWidth = MinPosterWidth,
            MaxChapterWidth = MaxChapterWidth,
            MinChapterWidth = MinChapterWidth,
            MaxDescriptionLength = MaxDescriptionLength,
            MinDescriptionLength = MinDescriptionLength,
            ImageQuality = ImageQuality,
            PosterQuality = PosterQuality,
            PosterFormat = PosterFormat,
            BannerQuality = BannerQuality,
            BannerFormat = BannerFormat,
            ImageFormat = ImageFormat,
            ArchiveFormat = ArchiveFormat
        };
    }
}