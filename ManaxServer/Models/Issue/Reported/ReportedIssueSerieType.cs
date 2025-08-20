// ReSharper disable PropertyCanBeMadeInitOnly.Global

using System.ComponentModel.DataAnnotations;

namespace ManaxServer.Models.Issue.Reported;

public class ReportedIssueSerieType
{
    public long Id { get; set; }
    [MaxLength(128)] public string Name { get; set; } = null!;
}