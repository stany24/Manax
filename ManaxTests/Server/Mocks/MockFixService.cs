using ManaxServer.Models.Chapter;
using ManaxServer.Services.Fix;

namespace ManaxTests.Server.Mocks;

public class MockFixService : IFixService
{
    public void FixSerie(long serieId)
    {
    }

    public void FixPoster(long serieId)
    {
    }

    public void FixNewChapter(NewChapter newChapter)
    {
    }

    public void ReplaceChapter(long oldChapterId, NewChapter newChapter)
    {
        throw new NotImplementedException();
    }
}