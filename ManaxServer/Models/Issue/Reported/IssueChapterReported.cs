// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedMember.Global

using System.ComponentModel.DataAnnotations.Schema;
using ManaxLibrary.DTO.Issue.Reported;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ManaxServer.Models.Issue.Reported;

[Index(nameof(UserId), nameof(ChapterId), nameof(ProblemId), IsUnique = true)]
public class IssueChapterReported
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))] public User.User User { get; set; } = null!;
    public long ChapterId { get; set; }

    [ForeignKey(nameof(ChapterId))] public Chapter.Chapter Chapter { get; set; } = null!;
    public long ProblemId { get; set; }
    [ForeignKey(nameof(ProblemId))] public IssueChapterReportedType Problem { get; set; } = null!;

    public IssueChapterReportedDto ToDto()
    {
        return new IssueChapterReportedDto
        {
            Id = Id,
            CreatedAt = CreatedAt,
            UserId = UserId,
            ChapterId = ChapterId,
            ProblemId = Problem.Id
        };
    }

    public static IssueChapterReported Create(IssueChapterReportedCreateDto issueChapterReportedCreate, long userId)
    {
        return new IssueChapterReported
        {
            CreatedAt = DateTime.UtcNow,
            UserId = userId,
            ChapterId = issueChapterReportedCreate.ChapterId,
            ProblemId = issueChapterReportedCreate.ProblemId
        };
    }
}