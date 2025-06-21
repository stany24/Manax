using ManaxApi.Models.Chapter;
using ManaxApi.Models.Library;
using ManaxApi.Models.Serie;
using ManaxApi.Services;

namespace ManaxApi.Task;

public class ChapterScanTask(Chapter chapter,LibraryContext libraryContext, SerieContext serieContext, ChapterContext chapterContext) : ITask
{
    public void Execute()
    {
        ScanService.ScanChapter(chapter, libraryContext, serieContext, chapterContext);
    }

    public string GetName()
    {
        return "Chapter Scan";
    }
}