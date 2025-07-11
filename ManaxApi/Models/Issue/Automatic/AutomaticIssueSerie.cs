// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedMember.Global

using System.ComponentModel.DataAnnotations.Schema;
using ManaxLibrary.DTOs.Issue.Automatic;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ManaxApi.Models.Issue.Automatic;

[PrimaryKey(nameof(SerieId), nameof(Problem))]
public class AutomaticIssueSerie
{
    public DateTime CreatedAt { get; set; }
    public long SerieId { get; set; }

    [ForeignKey(nameof(SerieId))] public Serie.Serie Serie { get; set; } = null!;
    public AutomaticIssueSerieType Problem { get; set; }
}