using ManaxServer.Models.Chapter;

namespace ManaxServer.Services.Fix;

public interface IFixService
{
    public void FixSerie(long serieId);
    public void FixPoster(long serieId);
    public void FixNewChapter(NewChapter newChapter);
    public void ReplaceChapter(long oldChapterId, NewChapter newChapter);
}