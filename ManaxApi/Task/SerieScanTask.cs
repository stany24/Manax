using ManaxApi.Models.Chapter;
using ManaxApi.Models.Library;
using ManaxApi.Models.Serie;
using ManaxApi.Services;

namespace ManaxApi.Task;

public class SerieScanTask(Serie serie,ManaxContext manaxContext) : ITask
{
    public void Execute()
    {
        ScanService.ScanSerie(serie, manaxContext);
    }

    public string GetName()
    {
        return "Serie Scan";
    }
}