using ManaxServer.Services;

namespace ManaxServer.Tasks;

public class FixSerieTask(long serieId) : ITask
{
    private readonly long _serieId = serieId;
    public void Execute()
    {
        ServicesManager.Fix.FixSerie(_serieId);
    }

    public string GetName()
    {
        return "Serie check";
    }

    public TaskPriority GetPriority()
    {
        return TaskPriority.SerieCheck;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not  FixSerieTask serieCheckTask) { return false; }
        return serieCheckTask._serieId == _serieId;
    }

    public override int GetHashCode()
    {
        return _serieId.GetHashCode();
    }
}