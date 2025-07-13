using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTOs.Serie;

namespace ManaxClient.Models;

public partial class ClientSerie : ObservableObject
{
    [ObservableProperty] private SerieDTO _info = null!;
    [ObservableProperty] private Bitmap? _poster;
}