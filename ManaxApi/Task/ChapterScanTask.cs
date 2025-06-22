using ManaxApi.Services;

namespace ManaxApi.Task;

public class ChapterScanTask(IServiceScopeFactory scopeFactory, long chapterId) : ITask
{
    public void Execute()
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        ScanService scanService = scope.ServiceProvider.GetRequiredService<ScanService>();
        scanService.ScanChapter(chapterId);
    }

    public string GetName()
    {
        return "Chapter Scan";
    }
}