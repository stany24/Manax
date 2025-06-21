using ManaxApi.Models.Library;
using ManaxApi.Services;

namespace ManaxApi.Task;

public class LibraryScanTask : ITask
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Library _library;

    public LibraryScanTask(IServiceScopeFactory scopeFactory, Library library)
    {
        _scopeFactory = scopeFactory;
        _library = library;
    }

    public void Execute()
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        ScanService scanService = scope.ServiceProvider.GetRequiredService<ScanService>();
        scanService.ScanLibrary(_library);
    }

    public string GetName()
    {
        return "Library Scan";
    }
}