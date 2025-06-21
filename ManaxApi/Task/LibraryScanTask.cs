using ManaxApi.Models.Chapter;
using ManaxApi.Models.Library;
using ManaxApi.Services;

namespace ManaxApi.Task;

public class LibraryScanTask(Library library,ManaxContext manaxContext) : ITask
{
    public void Execute()
    {
        ScanService.ScanLibrary(library, manaxContext);
    }

    public string GetName()
    {
        return "Library Scan";
    }
}