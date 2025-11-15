
namespace ManaxLibrary.DTO.Issue.Reported;

public class IssueChapterReportedDto
{
    public long Id { get; init; }
    public DateTime CreatedAt { get; init; }
    public long UserId { get; init; }
    public long ChapterId { get; init; }
    public long ProblemId { get; init; }
}