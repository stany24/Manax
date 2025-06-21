// ReSharper disable InconsistentNaming
// ReSharper disable PropertyCanBeMadeInitOnly.Global
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using System.ComponentModel.DataAnnotations;
using ManaxApi.Models.Chapter;
using ManaxApi.Models.User;

namespace ManaxApi.DTOs;

public class ReadDTO
{
    public long ChapterId { get; set; }
    public DateTime Date { get; set; }
    public Chapter Chapter { get; set; }
    public User User { get; set; }
}

public class ReadCreateDTO
{
    [Required]
    public int ChapterId { get; set; }
}