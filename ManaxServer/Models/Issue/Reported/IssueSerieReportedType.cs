// ReSharper disable PropertyCanBeMadeInitOnly.Global

using System.ComponentModel.DataAnnotations;
using ManaxLibrary.DTO.Issue.Reported;

namespace ManaxServer.Models.Issue.Reported;

public class IssueSerieReportedType
{
    public long Id { get; set; }
    [MaxLength(128)] public string Name { get; set; } = null!;

    public IssueSerieReportedTypeDto ToDto()
    {
        return new IssueSerieReportedTypeDto
        {
            Id = Id,
            Name = Name
        };
    }
}