using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using ImageMagick;
using ManaxClient.Event;
using ManaxClient.Models.Theme;

namespace ManaxClient.Assets;

public partial class IconManager:ObservableObject
{
    [ObservableProperty] private Bitmap? _libraryIcon;
    [ObservableProperty] private Bitmap? _tagsIcon;
    [ObservableProperty] private Bitmap? _homeIcon;
    [ObservableProperty] private Bitmap? _usersIcon;
    [ObservableProperty] private Bitmap? _settingsIcon;
    [ObservableProperty] private Bitmap? _uploadIcon;
    [ObservableProperty] private Bitmap? _featuresIcon;
    [ObservableProperty] private Bitmap? _statsIcon;
    [ObservableProperty] private Bitmap? _logoutIcon;
    [ObservableProperty] private Bitmap? _ranksIcon;
    [ObservableProperty] private Bitmap? _issuesIcon;
    
    public IconManager()
    {
        WeakReferenceMessenger.Default.Register<ThemeMessage>(this, (_, data) => { LoadIcons(data.Value); });
    }
    
    private void LoadIcons(ThemeSettingsData theme)
    {
        Color primaryColor = theme.PrimaryColor.ToRgb();
        MagickColor iconColor = new(primaryColor.R, primaryColor.G, primaryColor.B);
        Parallel.ForEach(typeof(IconManager).GetProperties(), propertyInfo =>
        {
            if (!propertyInfo.Name.EndsWith("Icon")) return;
            MagickImage icon =
                new(AssetLoader.Open(new Uri(
                    $"avares://ManaxClient/Assets/Icons/{propertyInfo.Name.Replace("Icon", "").ToLower()}.webp")));
            Bitmap updatedIcon = UpdateIconColor(icon,iconColor);
            propertyInfo.SetValue(this, updatedIcon);
        });
    }

    private static Bitmap UpdateIconColor(MagickImage image,MagickColor iconColor)
    {
        image.Colorize(iconColor, new Percentage(100));
        using MemoryStream ms = new();
        image.Write(ms, MagickFormat.Png);
        ms.Seek(0, SeekOrigin.Begin);
        Bitmap bitmap = new(ms);
        return bitmap;
    }
}