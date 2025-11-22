using ManaxServer.Models.Chapter;
using ManaxServer.Services.Fix;

namespace ManaxServer.Tasks;

public class FixNewChapterBackGroundTask(IFixService fixService, NewChapter chapter) : IBackGroundTask
{
    private readonly NewChapter _chapter = chapter;

    public void Execute()
    {
        fixService.FixNewChapter(_chapter);
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
        if (obj is not FixNewChapterBackGroundTask fixChapterTask) return false;
        return fixChapterTask._chapter.TempPath == _chapter.TempPath;
    }

    public override int GetHashCode()
    {
        return _chapter.TempPath.GetHashCode();
    }
}