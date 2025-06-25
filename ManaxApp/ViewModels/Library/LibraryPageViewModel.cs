
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTOs;

namespace ManaxApp.ViewModels.Library;

public partial class LibraryPageViewModel : PageViewModel
{
    [ObservableProperty] private LibraryDTO? _library;
    [ObservableProperty] private ObservableCollection<SerieDTO> _series = [];

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
                SerieDTO? serie = await ManaxApiSerieClient.GetSerieInfoAsync(serieId);
                if (serie == null) continue;
                Dispatcher.UIThread.Post(() => Series.Add(serie));
            }
        });
    }
}