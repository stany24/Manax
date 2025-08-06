using ManaxLibrary.DTO.Setting;
using ManaxServer.Services;

namespace ManaxServer.BackgroundTask;

public class PosterRenamingTask(string oldName,string newName,ImageFormat oldFormat, ImageFormat newFormat) : ITask
{
    private readonly string _oldName = oldName;
    private readonly string _newName = newName;
    private readonly ImageFormat _oldFormat = oldFormat;
    private readonly ImageFormat _newFormat = newFormat;
    public void Execute()
    {
        RenamingService.RenamePosters(_oldName, _newName, _oldFormat, _newFormat);
    }

    public string GetName()
    {
        return "Poster renaming";
    }

    public int GetPriority()
    {
        return (int)TaskPriority.PosterRenaming;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not PosterRenamingTask serieCheckTask) { return false; }
        return serieCheckTask._oldName == _oldName &&
               serieCheckTask._newName == _newName &&
               serieCheckTask._oldFormat == _oldFormat &&
               serieCheckTask._newFormat == _newFormat;
    }

    public override int GetHashCode()
    {
        return (_oldName, _newName, _oldFormat, _newFormat).GetHashCode();
    }
}