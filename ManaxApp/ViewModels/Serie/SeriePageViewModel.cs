using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using ManaxLibrary.ApiCaller;
using ManaxLibrary.DTOs;

namespace ManaxApp.ViewModels.Serie;

public partial class SeriePageViewModel : PageViewModel
{
    [ObservableProperty] private Bitmap? _poster;
    [ObservableProperty] private SerieDTO? _serie;
    [ObservableProperty] private ObservableCollection<ChapterDTO> _chapters = [];

    public SeriePageViewModel(long serieId)
    {
        ControlBarVisible = true;
        Task.Run(async () =>
        {
            SerieDTO? libraryAsync = await ManaxApiSerieClient.GetSerieInfoAsync(serieId);
            if (libraryAsync == null) return;
            Dispatcher.UIThread.Post(() => Serie= libraryAsync);
        });
        
        Task.Run(async () =>
        {
            byte[]? posterBytes = await ManaxApiSerieClient.GetSeriePosterAsync(serieId);
            if (posterBytes != null)
            {
                try { Poster = new Bitmap(new MemoryStream(posterBytes)); }
                catch { Poster = null; }
            }
            else { Poster = null; }
        });
        
        Task.Run(async () =>
        {
            List<long>? chaptersIds = await ManaxApiSerieClient.GetSerieChaptersAsync(serieId);
            
            if (chaptersIds == null) return;
            foreach (long chapterId in chaptersIds)
            {
                ChapterDTO? chapter = await ManaxApiChapterClient.GetChapterAsync(chapterId);
                if (chapter == null) continue;
                Dispatcher.UIThread.Post(() => Chapters.Add(chapter));
            }
        });
    }
}