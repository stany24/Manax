using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTO.Serie;
using ManaxLibrary.Logging;

namespace ManaxClient.Models;

public partial class ClientSerie : ObservableObject
{
    [ObservableProperty] private SerieDto _info;
    [ObservableProperty] private Bitmap? _poster;
    public event EventHandler<string>? InfoEmitted;

    public ClientSerie(SerieDto info)
    {
        _info = info;
        Task.Run(async () =>
        {
            Optional<byte[]> seriePosterAsync = await ManaxApiSerieClient.GetSeriePosterAsync(Info.Id);
            if (seriePosterAsync.Failed)
                InfoEmitted?.Invoke(this, seriePosterAsync.Error);
            else
                try
                {
                    Poster = new Bitmap(new MemoryStream(seriePosterAsync.GetValue()));
                }
                catch (Exception e)
                {
                    Logger.LogError("Failed to convert byte[] to image", e, Environment.StackTrace);
                    InfoEmitted?.Invoke(this, "Invalid image received for serie " + Info.Id);
                    Poster = null;
                }
        });
    }
}