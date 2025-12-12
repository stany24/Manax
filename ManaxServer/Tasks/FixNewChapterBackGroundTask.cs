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
        return fixChapterTask._chapter.Number == _chapter.Number 
               && fixChapterTask._chapter.SerieId == _chapter.SerieId 
               && fixChapterTask._chapter.UploaderId == _chapter.UploaderId;
    }

    public override int GetHashCode()
    {
        return _chapter.Number.GetHashCode();
    }
}