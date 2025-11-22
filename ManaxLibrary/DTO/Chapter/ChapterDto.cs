namespace ManaxLibrary.DTO.Chapter;

public class ChapterDto
{
    public long Id { get; init; }
    public long SerieId { get; init; }
    public int Number { get; init; }
    public int PageNumber { get; init; }

    public DateTime Creation { get; init; }
    public DateTime LastModification { get; init; }
}