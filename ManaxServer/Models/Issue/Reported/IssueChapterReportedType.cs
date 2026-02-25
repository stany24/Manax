// ReSharper disable PropertyCanBeMadeInitOnly.Global

using System.ComponentModel.DataAnnotations;
using ManaxLibrary.DTO.Issue.Reported;

namespace ManaxServer.Models.Issue.Reported;

public class IssueChapterReportedType
{
    public long Id { get; set; }
    [MaxLength(128)] public string Name { get; set; } = null!;

    public IssueChapterReportedTypeDto ToDto()
    {
        return new IssueChapterReportedTypeDto
        {
            Id = Id,
            Name = Name
        };
    }
}