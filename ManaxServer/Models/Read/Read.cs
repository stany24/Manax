// ReSharper disable PropertyCanBeMadeInitOnly.Global

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Models.Read;

[PrimaryKey(nameof(ChapterId), nameof(UserId))]
public class Read
{
    public DateTime Date { get; set; }

    public long ChapterId { get; set; }

    [ForeignKey(nameof(ChapterId))]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public Chapter.Chapter Chapter { get; set; }

    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    [DeleteBehavior(DeleteBehavior.NoAction)]
    public User.User User { get; set; }
}