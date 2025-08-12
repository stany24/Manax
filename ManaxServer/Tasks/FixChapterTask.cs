using ManaxServer.Services.Fix;

namespace ManaxServer.Tasks;

public class FixChapterTask(IFixService fixService, long chapterId) : ITask
{
    private readonly long _chapterId = chapterId;

    public void Execute()
    {
        fixService.FixChapter(_chapterId);
    }

    public string GetName()
    {
        return "Chapter check";
    }

    public TaskPriority GetPriority()
    {
        return TaskPriority.ChapterCheck;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not FixChapterTask chapterCheckTask) { return false; }
        return chapterCheckTask._chapterId == _chapterId;
    }

    public override int GetHashCode()
    {
        return _chapterId.GetHashCode();
    }
}