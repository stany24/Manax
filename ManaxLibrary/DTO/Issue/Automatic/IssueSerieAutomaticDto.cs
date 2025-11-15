namespace ManaxLibrary.DTO.Issue.Automatic;

public class IssueSerieAutomaticDto
{
    public DateTime CreatedAt { get; init; }
    public long SerieId { get; init; }

    public IssueSerieAutomaticType Problem { get; init; }
}