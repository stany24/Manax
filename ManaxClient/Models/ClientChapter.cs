using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTO.Chapter;

namespace ManaxClient.Models;

public partial class ClientChapter : ObservableObject
{
    [ObservableProperty] private ChapterDto _info = null!;
    [ObservableProperty] private ObservableCollection<Bitmap> _pages = [];
}