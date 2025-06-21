using ManaxApi.Services;

namespace ManaxApi.Task;

public class ChapterScanTask : ITask
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly long _chapterId;

    public ChapterScanTask(IServiceScopeFactory scopeFactory, long chapterId)
    {
        _scopeFactory = scopeFactory;
        _chapterId = chapterId;
    }

    public void Execute()
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ScanService scanService = scope.ServiceProvider.GetRequiredService<ScanService>();
        scanService.ScanChapter(_chapterId);
    }

    public string GetName()
    {
        return "Chapter Scan";
    }
}