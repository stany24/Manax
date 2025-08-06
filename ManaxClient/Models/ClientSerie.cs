using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.DTO.Serie;

namespace ManaxClient.Models;

public partial class ClientSerie : ObservableObject
{
    [ObservableProperty] private SerieDto _info = null!;
    [ObservableProperty] private Bitmap? _poster;
}