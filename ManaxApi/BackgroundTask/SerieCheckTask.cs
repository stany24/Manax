using ManaxApi.Services;

namespace ManaxApi.BackgroundTask;

public class SerieCheckTask(long serieId) : ITask
{
    private readonly long _serieId = serieId;
    public void Execute()
    {
        CheckService.CheckSerie(_serieId);
    }

    public string GetName()
    {
        return "Serie Scan";
    }

    public int GetPriority()
    {
        return 5;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not  SerieCheckTask serieCheckTask) { return false; }
        return serieCheckTask._serieId == _serieId;
    }

    public override int GetHashCode()
    {
        return _serieId.GetHashCode();
    }
}