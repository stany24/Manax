using ManaxApi.Services;

namespace ManaxApi.BackgroundTask;

public class ChapterCheckTask(long chapterId) : ITask
{
    private readonly long _chapterId = chapterId;
    public void Execute()
    {
        CheckService.CheckChapter(_chapterId);
    }

    public string GetName()
    {
        return "Chapter Scan";
    }

    public int GetPriority()
    {
        return 10;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not  ChapterCheckTask chapterCheckTask) { return false; }
        return chapterCheckTask._chapterId == _chapterId;
    }

    public override int GetHashCode()
    {
        return _chapterId.GetHashCode();
    }
}