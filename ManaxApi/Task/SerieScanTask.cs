using ManaxApi.Models.Chapter;
using ManaxApi.Models.Library;
using ManaxApi.Models.Serie;
using ManaxApi.Services;

namespace ManaxApi.Task;

public class SerieScanTask(Serie serie,LibraryContext libraryContext, SerieContext serieContext, ChapterContext chapterContext) : ITask
{
    public void Execute()
    {
        ScanService.ScanSerie(serie, libraryContext, serieContext, chapterContext);
    }

    public string GetName()
    {
        return "Serie Scan";
    }
}