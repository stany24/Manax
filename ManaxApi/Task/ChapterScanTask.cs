using ManaxApi.Services;

namespace ManaxApi.Task;

public class ChapterScanTask(long chapterId) : ITask
{
    public void Execute()
    {
        ScanService.ScanChapter(chapterId);
    }

    public string GetName()
    {
        return "Chapter Scan";
    }
}