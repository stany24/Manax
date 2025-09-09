using ManaxServer.Localization;
using ManaxServer.Services.Fix;

namespace ManaxServer.Tasks;

public class FixChapterBackGroundTask(IFixService fixService, long chapterId) : IBackGroundTask
{
    private readonly long _chapterId = chapterId;

    public void Execute()
    {
        fixService.FixChapter(_chapterId);
    }

    public string GetName()
    {
        return Localizer.TaskChapterFix();
    }

    public TaskPriority GetPriority()
    {
        return TaskPriority.ChapterFix;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not FixChapterBackGroundTask fixChapterTask) return false;
        return fixChapterTask._chapterId == _chapterId;
    }

    public override int GetHashCode()
    {
        return _chapterId.GetHashCode();
    }
}