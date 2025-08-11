using ManaxLibrary.DTO.Setting;
using ManaxServer.Services;

namespace ManaxServer.Tasks;

public class PosterRenamingTask(string oldName,string newName,ImageFormat oldFormat, ImageFormat newFormat) : ITask
{
    private readonly string _oldName = oldName;
    private readonly ImageFormat _oldFormat = oldFormat;

    public void Execute()
    {
        ServicesManager.Renaming.RenamePosters(_oldName, newName, _oldFormat, newFormat);
    }

    public string GetName()
    {
        return "Poster renaming";
    }

    public TaskPriority GetPriority()
    {
        return TaskPriority.PosterRenaming;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not PosterRenamingTask serieCheckTask) { return false; }
        return serieCheckTask._oldName == _oldName && serieCheckTask._oldFormat == _oldFormat;
    }

    public override int GetHashCode()
    {
        return (_oldName, _newName: newName, _oldFormat, _newFormat: newFormat).GetHashCode();
    }
}