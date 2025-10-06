// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ManaxLibrary.DTO.Issue.Automatic;

public class IssueChapterAutomaticDto
{
    public DateTime CreatedAt { get; set; }
    public long ChapterId { get; set; }
    public IssueChapterAutomaticType Problem { get; set; }
}