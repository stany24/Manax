// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedMember.Global

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ManaxLibrary.DTOs.Issue.Internal;
public class InternalSerieIssueDTO
{
    public DateTime CreatedAt { get; set; }
    public long SerieId { get; set; }

    public InternalSerieIssueType Problem { get; set; }
}