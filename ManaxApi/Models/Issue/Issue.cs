// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedMember.Global
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using System.ComponentModel.DataAnnotations;

namespace ManaxApi.Models.Issue;

public class Issue
{
    public long Id { get; set; }
    public List<Chapter.Chapter> Chapters { get; set; } = [];
    public User.User User { get; set; }
    [MaxLength(128)] public string Problem { get; set; }
}