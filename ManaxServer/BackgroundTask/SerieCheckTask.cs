using ManaxServer.Services;

namespace ManaxServer.BackgroundTask;

public class SerieCheckTask(long serieId) : ITask
{
    private readonly long _serieId = serieId;
    public void Execute()
    {
        FixService.FixSerie(_serieId);
    }

    public string GetName()
    {
        return "Serie check";
    }

    public int GetPriority()
    {
        return (int)TaskPriority.SerieCheck;
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