using ManaxServer.Localization;
using ManaxServer.Services.Fix;

namespace ManaxServer.Tasks;

public class FixSerieTask(IFixService fixService, long serieId) : ITask
{
    private readonly long _serieId = serieId;

    public void Execute()
    {
        fixService.FixSerie(_serieId);
    }

    public string GetName()
    {
        return Localizer.GetString("TaskSerieFix");
    }

    public TaskPriority GetPriority()
    {
        return TaskPriority.SerieFix;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not FixSerieTask fixSerieTask) return false;
        return fixSerieTask._serieId == _serieId;
    }

    public override int GetHashCode()
    {
        return _serieId.GetHashCode();
    }
}