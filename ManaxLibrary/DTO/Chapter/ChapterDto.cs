namespace ManaxLibrary.DTO.Chapter;

public class ChapterDto
{
    public long Id { get; init; }
    public long SerieId { get; init; }
    public uint Number { get; init; }
    public uint PageNumber { get; init; }
    public DateTime Creation { get; init; }
    public DateTime LastModification { get; init; }
}