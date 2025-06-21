using ManaxApi.Models.Chapter;
using ManaxApi.Models.Library;
using ManaxApi.Models.Serie;
using ManaxApi.Services;

namespace ManaxApi.Task;

public class LibraryScanTask(Library library,LibraryContext libraryContext, SerieContext serieContext, ChapterContext chapterContext) : ITask
{
    public void Execute()
    {
        ScanService.ScanLibrary(library, libraryContext, serieContext, chapterContext);
    }

    public string GetName()
    {
        return "Library Scan";
    }
}