using ManaxApi.Models.Chapter;
using ManaxApi.Services;

namespace ManaxApi.Task;

public class ChapterScanTask(Chapter chapter, ManaxContext manaxContext) : ITask
{
    public void Execute()
    {
        ScanService.ScanChapter(chapter, manaxContext);
    }

    public string GetName()
    {
        return "Chapter Scan";
    }
}