using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTO.Issue.Reported;

public class IssueChapterReportedCreateDto
{
    [Required] public long ChapterId { get; init; }

    [Required] public long ProblemId { get; init; }
}