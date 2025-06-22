using ManaxApi.Services;

namespace ManaxApi.Task;

public class SerieScanTask(long serieId) : ITask
{
    public void Execute()
    {
        ScanService.ScanSerie(serieId);
    }

    public string GetName()
    {
        return "Serie Scan";
    }
}