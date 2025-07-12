using ImageMagick;

namespace ManaxApi.Settings;

public enum ImageFormat
{
    Jpeg = 0,
    Png = 1,
    Webp = 2
}

internal static class ImageFormatMethods
{

    public static MagickFormat GetMagickFormat(this ImageFormat format)
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