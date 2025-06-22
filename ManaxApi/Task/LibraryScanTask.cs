using ManaxApi.Models.Library;
using ManaxApi.Services;

namespace ManaxApi.Task;

public class LibraryScanTask(Library library) : ITask
{
    public void Execute()
    {
        ScanService.ScanLibrary(library);
    }

    public string GetName()
    {
        return "Library Scan";
    }
}