using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace ManaxApi.Models.Rank;

[PrimaryKey(nameof(UserId), nameof(SerieId))]
public class UserRank
{
    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))] public User.User User { get; set; } = null!;

    public long SerieId { get; set; }

    [ForeignKey(nameof(SerieId))] public Serie.Serie Serie { get; set; } = null!;

    public long RankId { get; set; }

    [ForeignKey(nameof(RankId))] public Rank Rank { get; set; } = null!;
}