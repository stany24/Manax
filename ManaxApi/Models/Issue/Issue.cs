// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedMember.Global

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ManaxApi.Models.Issue;

public class Issue
{
    public long Id { get; set; }
    public long ChapterId { get; set; }
    [ForeignKey(nameof(ChapterId))]
    public Chapter.Chapter Chapter { get; set; } = null!;
    public long UserId { get; set; }
    [ForeignKey(nameof(UserId))]
    public User.User User { get; set; } = null!;
    [MaxLength(128)] public string Problem { get; set; }
}