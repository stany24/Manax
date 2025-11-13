// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedMember.Global

using System.ComponentModel.DataAnnotations.Schema;
using ManaxLibrary.DTO.Issue.Reported;
using Microsoft.EntityFrameworkCore;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ManaxServer.Models.Issue.Reported;

[Index(nameof(UserId), nameof(SerieId), nameof(ProblemId), IsUnique = true)]
public class IssueSerieReported
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public long UserId { get; set; }

    [ForeignKey(nameof(UserId))] public User.User User { get; set; } = null!;
    public long SerieId { get; set; }

    [ForeignKey(nameof(SerieId))] public Serie.Serie Serie { get; set; } = null!;
    public long ProblemId { get; set; }
    [ForeignKey(nameof(ProblemId))] public IssueSerieReportedType Problem { get; set; } = null!;
    
    public IssueSerieReportedDto ToDto()
    {
        return new IssueSerieReportedDto
        {
            Id = Id,
            CreatedAt = CreatedAt,
            UserId = UserId,
            SerieId = SerieId,
            ProblemId = Problem.Id
        };
    }

    public static IssueSerieReported Create(IssueSerieReportedCreateDto issueSerieReportedCreate, long currentUserId)
    {
        return new IssueSerieReported
        {
            CreatedAt = DateTime.UtcNow,
            UserId = currentUserId,
            SerieId = issueSerieReportedCreate.SerieId,
            ProblemId = issueSerieReportedCreate.ProblemId
        };
    }
}