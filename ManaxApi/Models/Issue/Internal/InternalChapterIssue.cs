// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedMember.Global

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ManaxApi.Models.Issue.Internal;


[PrimaryKey(nameof(ChapterId), nameof(Problem))]
public class InternalChapterIssue
{
    public DateTime CreatedAt { get; set; }
    public long ChapterId { get; set; }

    [ForeignKey(nameof(ChapterId))] public Chapter.Chapter Chapter { get; set; } = null!;
    public InternalChapterIssueTypeEnum Problem { get; set; }
}