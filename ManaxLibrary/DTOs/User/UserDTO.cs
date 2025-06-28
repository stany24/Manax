// ReSharper disable InconsistentNaming
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTOs.User;

public class UserDTO
{
    public long Id { get; set; }
    [MaxLength(50)] public string Username { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}