namespace ManaxLibrary.DTO.Serie;

public class SerieDto
{
    public long Id { get; init; }
    public List<long> PersonIds { get; init; } = [];
    public List<long> TagIds { get; init; } = [];
    public long? LibraryId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Status Status { get; init; }
    public DateTime Creation { get; init; }
    public DateTime LastModification { get; init; }
}