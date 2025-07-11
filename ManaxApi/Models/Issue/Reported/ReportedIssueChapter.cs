// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedMember.Global

using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ManaxApi.Models.Issue.Reported;

public class ReportedIssueChapter
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))] public Models.User.User User { get; set; } = null!;
    public long ChapterId { get; set; }

    [ForeignKey(nameof(ChapterId))] public Chapter.Chapter Chapter { get; set; } = null!;
    public long ProblemId { get; set; }
    [ForeignKey(nameof(ProblemId))] public ReportedIssueChapterType Problem { get; set; } = null!;
}