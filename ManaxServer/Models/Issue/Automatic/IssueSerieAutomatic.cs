// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedMember.Global

using System.ComponentModel.DataAnnotations.Schema;
using ManaxLibrary.DTO.Issue.Automatic;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ManaxServer.Models.Issue.Automatic;

[PrimaryKey(nameof(SerieId), nameof(Problem))]
public class AutomaticIssueSerie
{
    public DateTime CreatedAt { get; set; }
    public long SerieId { get; set; }

    [ForeignKey(nameof(SerieId))] public Serie.Serie Serie { get; set; } = null!;
    public IssueSerieAutomaticType Problem { get; set; }
}