using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ManaxClient.Models.Upload;

public partial class ImageFile: ObservableObject
{
    [ObservableProperty] private string _path;
    [ObservableProperty] private string _fileName;
    [ObservableProperty] private Bitmap? _preview;

    public ImageFile(string path)
    {
        Path = path;
        FileName = System.IO.Path.GetFileName(path);
        try
        {
            Task.Run(() =>
            {
                using FileStream stream = File.OpenRead(path);
                Preview = new Bitmap(stream);
            });
        }
        catch (Exception)
        {
            // ignored
        }
    }
}