using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTOs.Chapter;

namespace ManaxClient.Models;

public partial class ClientChapter : ObservableObject
{
    [ObservableProperty] private ChapterDTO _info = null!;
    [ObservableProperty] private ObservableCollection<Bitmap> _pages = [];
}