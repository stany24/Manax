using ManaxServer.Localization;
using ManaxServer.Services.Fix;

namespace ManaxServer.Tasks;

public class FixPosterBackGroundTask(IFixService fixService, long serieId) : IBackGroundTask
{
    private readonly long _serieId = serieId;

    public void Execute()
    {
        fixService.FixPoster(_serieId);
    }

    public string GetName()
    {
        return Localizer.GetString("TaskPosterFix");
    }

    public TaskPriority GetPriority()
    {
        return TaskPriority.PosterFix;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not FixPosterBackGroundTask fixPosterTask) return false;
        return fixPosterTask._serieId == _serieId;
    }

    public override int GetHashCode()
    {
        return _serieId.GetHashCode();
    }
}