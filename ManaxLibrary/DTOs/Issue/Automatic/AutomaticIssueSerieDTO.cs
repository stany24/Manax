// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedMember.Global

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace ManaxLibrary.DTOs.Issue.Automatic;
public class AutomaticIssueSerieDTO
{
    public DateTime CreatedAt { get; set; }
    public long SerieId { get; set; }

    public AutomaticIssueSerieType Problem { get; set; }
}