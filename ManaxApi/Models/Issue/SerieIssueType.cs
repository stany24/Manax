namespace ManaxApi.Models.Issue;

public class SerieIssueType
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
}

public enum SerieIssueTypeEnum
{
    PosterMissing,
    PosterDuplicate,
    PosterWrongFormat,
}