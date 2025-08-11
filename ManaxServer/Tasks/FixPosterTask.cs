using ManaxServer.Services;

namespace ManaxServer.Tasks;

public class FixPosterTask(long serieId) : ITask
{
    private readonly long _serieId = serieId;
    public void Execute()
    {
        ServicesManager.Fix.FixPoster(_serieId);
    }

    public string GetName()
    {
        return "Poster check";
    }

    public TaskPriority GetPriority()
    {
        return TaskPriority.PosterCheck;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not FixPosterTask serieCheckTask) { return false; }
        return serieCheckTask._serieId == _serieId;
    }

    public override int GetHashCode()
    {
        return _serieId.GetHashCode();
    }
}