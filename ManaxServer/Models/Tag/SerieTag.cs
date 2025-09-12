using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace ManaxServer.Models.Tag;

[PrimaryKey(nameof(TagId), nameof(SerieId))]
public class SerieTag
{
    public long TagId { get; set; }

    [ForeignKey(nameof(TagId))] public Tag Tag { get; set; } = null!;

    public long SerieId { get; set; }

    [ForeignKey(nameof(SerieId))] public Serie.Serie Serie { get; set; } = null!;
}