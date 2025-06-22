using ManaxApi.Services;

namespace ManaxApi.Task;

public class SerieScanTask(IServiceScopeFactory scopeFactory, long serieId) : ITask
{
    public void Execute()
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        ScanService scanService = scope.ServiceProvider.GetRequiredService<ScanService>();
        scanService.ScanSerie(serieId);
    }

    public string GetName()
    {
        return "Serie Scan";
    }
}