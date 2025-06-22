// ReSharper disable InconsistentNaming
// ReSharper disable PropertyCanBeMadeInitOnly.Global
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTOs;

public class ReadDTO
{
    public long ChapterId { get; set; }
    public DateTime Date { get; set; }
    public ChapterDTO Chapter { get; set; }
    public UserDTO User { get; set; }
}

public class ReadCreateDTO
{
    [Required]
    public int ChapterId { get; set; }
}