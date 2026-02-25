namespace ManaxLibrary.DTO.Read;

public class ReadDto
{
    public long ChapterId { get; init; }
    public uint Page { get; init; }
    public DateTime Date { get; init; }
    public long UserId { get; init; }
}