// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedMember.Global

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Models.Issue;

[PrimaryKey(nameof(ChapterId), nameof(ProblemId))]
public class ChapterIssue
{
    public DateTime CreatedAt { get; set; }
    public long ChapterId { get; set; }
    public long ProblemId { get; set; }
}