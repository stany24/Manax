namespace ManaxLibrary.DTO.Tag;

public class TagDto
{
    public long Id { get; init; }
    public string Name { get; init; } = null!;
    public int ColorArgb { get; init; }
    public List<long> SerieIds { get; init; } = [];
}