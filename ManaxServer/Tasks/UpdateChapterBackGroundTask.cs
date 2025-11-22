using ManaxServer.Services.Fix;

namespace ManaxServer.Tasks;

public class UpdateChapterBackGroundTask(IFixService fixService, long chapterId) : IBackGroundTask
{
    private readonly long _chapterId = chapterId;

    public void Execute()
    {
        fixService.UpdateChapter(_chapterId);
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
        if (obj is not UpdateChapterBackGroundTask fixChapterTask) return false;
        return fixChapterTask._chapterId == _chapterId;
    }

    public override int GetHashCode()
    {
        return _chapterId.GetHashCode();
    }
}