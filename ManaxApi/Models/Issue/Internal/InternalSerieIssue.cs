// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedMember.Global

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ManaxApi.Models.Issue.Internal;

[PrimaryKey(nameof(SerieId), nameof(Problem))]
public class InternalSerieIssue
{
    public DateTime CreatedAt { get; set; }
    public long SerieId { get; set; }

    [ForeignKey(nameof(SerieId))] public Serie.Serie Serie { get; set; } = null!;
    public InternalSerieIssueTypeEnum Problem { get; set; }
}