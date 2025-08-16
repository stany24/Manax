using ManaxLibrary.DTO.Setting;
using ManaxServer.Localization;
using ManaxServer.Services.Renaming;

namespace ManaxServer.Tasks;

public class PosterRenamingTask(
    IRenamingService renamingService,
    string oldName,
    string newName,
    ImageFormat oldFormat,
    ImageFormat newFormat)
    : ITask
{
    private readonly ImageFormat _oldFormat = oldFormat;
    private readonly string _oldName = oldName;

    public void Execute()
    {
        renamingService.RenamePosters(_oldName, newName, _oldFormat, newFormat);
    }

    public string GetName()
    {
        return Localizer.GetString("TaskPosterRenaming");
    }

    public TaskPriority GetPriority()
    {
        return TaskPriority.PosterRenaming;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PosterRenamingTask task) return false;
        return task._oldName == _oldName && task._oldFormat == _oldFormat;
    }

    public override int GetHashCode()
    {
        return (_oldName, _newName: newName, _oldFormat, _newFormat: newFormat).GetHashCode();
    }
}