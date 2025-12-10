using System;
using System.Collections.Concurrent;
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

namespace ManaxClient.Manager;

public partial class IconManager:ObservableObject
{
    private readonly ConcurrentDictionary<string, MagickImage> _cachedIconImages = new();
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
        LoadIconsFromDisk();
        WeakReferenceMessenger.Default.Register<ThemeMessage>(this, (_, data) => { ColorizeIcons(data.Value); });
    }
    
    private void LoadIconsFromDisk()
    {
        Parallel.ForEach(typeof(IconManager).GetProperties(), propertyInfo =>
        {
            if (!propertyInfo.Name.EndsWith("Icon")) return;
            
            string iconName = propertyInfo.Name.Replace("Icon", "").ToLower();
            MagickImage originalIcon = new(AssetLoader.Open(new Uri(
                $"avares://ManaxClient/Assets/Icons/{iconName}.webp")));
            _cachedIconImages.TryAdd(iconName, originalIcon);
        });
    }
    
    private void ColorizeIcons(ThemeSettingsData theme)
    {
        Color accent = theme.AccentColor.ToRgb();
        MagickColor iconColor = new(accent.R, accent.G, accent.B);
        Parallel.ForEach(typeof(IconManager).GetProperties(), propertyInfo =>
        {
            if (!propertyInfo.Name.EndsWith("Icon")) return;
            
            string iconName = propertyInfo.Name.Replace("Icon", "").ToLower();
            MagickImage iconCopy = (MagickImage)_cachedIconImages[iconName].Clone();
            Bitmap updatedIcon = UpdateIconColor(iconCopy, iconColor);
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