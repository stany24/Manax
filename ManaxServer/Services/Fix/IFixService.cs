namespace ManaxServer.Services.Fix;

public interface IFixService
{
    public void FixSerie(long serieId);
    public void FixPoster(long serieId);
    public void FixChapter(long chapterId);
}