using ManaxApi.Models.Chapter;
using ManaxApi.Models.Library;
using ManaxApi.Models.Serie;
using ManaxApi.Services;

namespace ManaxApi.Task;

public class SerieScanTask(Serie serie,ManaxContext ManaxContext) : ITask
{
    public void Execute()
    {
        ScanService.ScanSerie(serie, ManaxContext);
    }

    public string GetName()
    {
        return "Serie Scan";
    }
}