// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedMember.Global

using System.ComponentModel.DataAnnotations.Schema;
using ManaxApi.Models.Chapter;
using ManaxApi.Models.Issue.User;
using ManaxApi.Models.User;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

public class UserChapterIssue
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))] public User User { get; set; } = null!;
    public long ChapterId { get; set; }

    [ForeignKey(nameof(ChapterId))] public Chapter Chapter { get; set; } = null!;
    public long ProblemId { get; set; }
    [ForeignKey(nameof(ProblemId))] public UserChapterIssueType Problem { get; set; } = null!;
}