// ReSharper disable InconsistentNaming
// ReSharper disable PropertyCanBeMadeInitOnly.Global
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using System.ComponentModel.DataAnnotations;
using ManaxApi.Models.User;

namespace ManaxApi.DTOs;

public class IssueDTO
{
    public long Id { get; set; }
    public ChapterDTO Chapter { get; set; }
    public User User { get; set; }
    [MaxLength(128)] public string Problem { get; set; }
}

public class IssueCreateDTO
{
    [Required] public ChapterDTO Chapter { get; set; }
        
    [Required] [MaxLength(128)] public string Problem { get; set; }
}