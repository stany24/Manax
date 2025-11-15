namespace ManaxLibrary.DTO.Chapter;

public class ChapterDto
{
    public long Id { get; init; }
    public long SerieId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public int Number { get; init; }
    public int PageNumber { get; init; }

    public DateTime Creation { get; init; }
    public DateTime LastModification { get; init; }
}