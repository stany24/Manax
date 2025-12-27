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
    private readonly ConcurrentDictionary<string, byte[]> _cachedIconImages = new();
    [ObservableProperty] private Bitmap? _libraryIcon;
    [ObservableProperty] private Bitmap? _tagsIcon;
    [ObservableProperty] private Bitmap? _homeIcon;
    [ObservableProperty] private Bitmap? _usersIcon;
    [ObservableProperty] private Bitmap? _userIcon;
    [ObservableProperty] private Bitmap? _settingsIcon;
    [ObservableProperty] private Bitmap? _uploadIcon;
    [ObservableProperty] private Bitmap? _featuresIcon;
    [ObservableProperty] private Bitmap? _statsIcon;
    [ObservableProperty] private Bitmap? _logoutIcon;
    [ObservableProperty] private Bitmap? _ranksIcon;
    [ObservableProperty] private Bitmap? _issuesIcon;
    [ObservableProperty] private Bitmap? _folderIcon;
    [ObservableProperty] private Bitmap? _addIcon;
    [ObservableProperty] private Bitmap? _editIcon;
    [ObservableProperty] private Bitmap? _trashIcon;
    [ObservableProperty] private Bitmap? _ideaIcon;
    [ObservableProperty] private Bitmap? _moonIcon;
    [ObservableProperty] private Bitmap? _nextIcon;
    [ObservableProperty] private Bitmap? _previousIcon;
    [ObservableProperty] private Bitmap? _flagIcon;
    
    public IconManager()
    {
        LoadIconsFromDisk();
        WeakReferenceMessenger.Default.Register<ThemeMessage>(this, (_, data) =>
        {
            ColorizeIcons(data.Value);
        });
    }
    
    private void LoadIconsFromDisk()
    {
        Parallel.ForEach(typeof(IconManager).GetProperties(), propertyInfo =>
        {
            if (!propertyInfo.Name.EndsWith("Icon")) return;
            
            string iconName = propertyInfo.Name.Replace("Icon", "").ToLower();
            using MagickImage originalIcon = new(AssetLoader.Open(new Uri(
                $"avares://ManaxClient/Assets/Icons/{iconName}.webp")));
            byte[] imageBytes = originalIcon.ToByteArray(MagickFormat.Png);
            _cachedIconImages.TryAdd(iconName, imageBytes);
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
            using MagickImage iconCopy = new(_cachedIconImages[iconName]);
            Bitmap updatedIcon = UpdateIconColor(iconCopy, iconColor);
            propertyInfo.SetValue(this, updatedIcon);
        });
    }

    private static Bitmap UpdateIconColor(MagickImage image,MagickColor iconColor)
    {
        image.Colorize(iconColor, new Percentage(80));
        using MemoryStream ms = new();
        image.Write(ms, MagickFormat.Png);
        ms.Seek(0, SeekOrigin.Begin);
        Bitmap bitmap = new(ms);
        return bitmap;
    }
}