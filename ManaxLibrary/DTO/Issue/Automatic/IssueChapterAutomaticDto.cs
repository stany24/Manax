namespace ManaxLibrary.DTO.Issue.Automatic;

public class IssueChapterAutomaticDto
{
    public DateTime CreatedAt { get; init; }
    public long ChapterId { get; init; }
    public IssueChapterAutomaticType Problem { get; init; }
}