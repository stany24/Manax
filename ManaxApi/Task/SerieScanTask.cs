using ManaxApi.Services;

namespace ManaxApi.Task;

public class SerieScanTask : ITask
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly long _serieId;

    public SerieScanTask(IServiceScopeFactory scopeFactory, long serieId)
    {
        _scopeFactory = scopeFactory;
        _serieId = serieId;
    }

    public void Execute()
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ScanService scanService = scope.ServiceProvider.GetRequiredService<ScanService>();
        scanService.ScanSerie(_serieId);
    }

    public string GetName()
    {
        return "Serie Scan";
    }
}