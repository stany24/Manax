using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTO.Issue.Reported;

public class IssueSerieReportedCreateDto
{
    [Required] public long SerieId { get; init; }

    [Required] public long ProblemId { get; init; }
}