using ManaxServer.Models.Chapter;
using ManaxServer.Services.Fix;

namespace ManaxServer.Tasks;

public class ReplaceChapterBackGroundTask(IFixService fixService, long oldChapter, NewChapter newChapter)
    : IBackGroundTask
{
    private readonly long _oldChapter = oldChapter;

    public void Execute()
    {
        fixService.ReplaceChapter(_oldChapter, newChapter);
    }

    public string GetName()
    {
        return "Chapter fix";
    }

    public TaskPriority GetPriority()
    {
        return TaskPriority.ChapterFix;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not ReplaceChapterBackGroundTask fixChapterTask) return false;
        return fixChapterTask._oldChapter == _oldChapter;
    }

    public override int GetHashCode()
    {
        return _oldChapter.GetHashCode();
    }
}