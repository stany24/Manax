using ManaxApi.Models.Chapter;
using ManaxApi.Models.Library;
using ManaxApi.Models.Serie;
using ManaxApi.Services;

namespace ManaxApi.Task;

public class LibraryScanTask(Library library,ManaxContext ManaxContext) : ITask
{
    public void Execute()
    {
        ScanService.ScanLibrary(library, ManaxContext);
    }

    public string GetName()
    {
        return "Library Scan";
    }
}