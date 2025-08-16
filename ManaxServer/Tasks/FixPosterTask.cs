using ManaxServer.Localization;
using ManaxServer.Services.Fix;

namespace ManaxServer.Tasks;

public class FixPosterTask(IFixService fixService, long serieId) : ITask
{
    private readonly long _serieId = serieId;

    public void Execute()
    {
        fixService.FixPoster(_serieId);
    }

    public string GetName()
    {
        return Localizer.GetString("TaskPosterCheck");
    }

    public TaskPriority GetPriority()
    {
        return TaskPriority.PosterCheck;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not FixPosterTask serieCheckTask) return false;
        return serieCheckTask._serieId == _serieId;
    }

    public override int GetHashCode()
    {
        return _serieId.GetHashCode();
    }
}