using ManaxApi.Models.Library;
using ManaxApi.Services;

namespace ManaxApi.Task;

public class LibraryScanTask(IServiceScopeFactory scopeFactory, Library library) : ITask
{
    public void Execute()
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        ScanService scanService = scope.ServiceProvider.GetRequiredService<ScanService>();
        scanService.ScanLibrary(library);
    }

    public string GetName()
    {
        return "Library Scan";
    }
}