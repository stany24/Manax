// ReSharper disable InconsistentNaming
// ReSharper disable PropertyCanBeMadeInitOnly.Global
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

using System.ComponentModel.DataAnnotations;
using ManaxApi.Models.User;

namespace ManaxApi.DTOs;

public class UserDTO
{
    public long Id { get; set; }
    [MaxLength(50)] public string Username { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}

public class UserCreateDTO
{
    [Required] [MaxLength(50)] public string Username { get; set; } = string.Empty;
    [Required] [MaxLength(128)] public string Password { get; set; } = string.Empty;
}

public class UserLoginDTO
{
    [Required]
    public string Username { get; set; }
        
    [Required]
    public string Password { get; set; }
}
    
public class UserUpdateDTO
{
    [Required]
    public string Username { get; set; }
        
    [Required]
    public string Password { get; set; }
}