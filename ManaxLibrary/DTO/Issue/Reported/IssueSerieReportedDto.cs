// ReSharper disable InconsistentNaming
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ManaxLibrary.DTO.Issue.Reported;

public class IssueSerieReportedDto
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public long UserId { get; set; }
    public long SerieId { get; set; }
    public long ProblemId { get; set; }
}