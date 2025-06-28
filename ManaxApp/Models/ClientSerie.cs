using Avalonia.Media.Imaging;
using ManaxLibrary.DTOs;
using ManaxLibrary.DTOs.Serie;

namespace ManaxApp.Models;

public class ClientSerie
{
    public SerieDTO Info {get; set;}
    public Bitmap? Poster { get; set; }
}