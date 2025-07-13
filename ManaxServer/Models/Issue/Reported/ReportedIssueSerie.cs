// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedMember.Global

using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ManaxServer.Models.Issue.Reported;

public class ReportedIssueSerie
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))] public Models.User.User User { get; set; } = null!;
    public long SerieId { get; set; }

    [ForeignKey(nameof(SerieId))] public Serie.Serie Serie { get; set; } = null!;
    public long ProblemId { get; set; }
    [ForeignKey(nameof(ProblemId))] public ReportedIssueSerieType Problem { get; set; } = null!;
}