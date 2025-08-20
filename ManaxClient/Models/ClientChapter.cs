using System.Collections.ObjectModel;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Read;

namespace ManaxClient.Models;

public partial class ClientChapter : ObservableObject
{
    [ObservableProperty] private ChapterDto _info = null!;
    [ObservableProperty] private ObservableCollection<Bitmap> _pages = [];
    [ObservableProperty] private ReadDto? _read;
}