namespace ManaxLibrary.DTO.Issue.Reported;

public class IssueSerieReportedDto
{
    public long Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public long UserId { get; init; }
    public long SerieId { get; init; }
    public long ProblemId { get; init; }
}