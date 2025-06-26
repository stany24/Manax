using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTOs;

namespace ManaxApp.ViewModels.Library;

public partial class LibraryPageViewModel : PageViewModel
{
    [ObservableProperty] private LibraryDTO? _library;
    [ObservableProperty] private ObservableCollection<ClientSerie> _series = [];

    public LibraryPageViewModel(long libraryId)
    {
        ControlBarVisible = true;
        Task.Run(async () =>
        {
            LibraryDTO? libraryAsync = await ManaxApiLibraryClient.GetLibraryAsync(libraryId);
            if (libraryAsync == null) return;
            Dispatcher.UIThread.Post(() => Library = libraryAsync);
        });
        Task.Run(async () =>
        {
            List<long>? seriesIds = await ManaxApiLibraryClient.GetLibrarySeriesAsync(libraryId);
            
            if (seriesIds == null) return;
            foreach (long serieId in seriesIds)
            {
                SerieDTO? info = await ManaxApiSerieClient.GetSerieInfoAsync(serieId);
                if (info == null) continue;
                byte[]? posterBytes = await ManaxApiSerieClient.GetSeriePosterAsync(serieId);
                ClientSerie serie = new() { Info = info };
                if (posterBytes != null)
                {
                    try { serie.Poster = new Bitmap(new MemoryStream(posterBytes)); }
                    catch { serie.Poster = null; }
                }
                else { serie.Poster = null; }
                Dispatcher.UIThread.Post(() => Series.Add(serie));
            }
        });
    }
}