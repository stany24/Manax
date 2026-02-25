using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTO.User;

public class UserCreateDto
{
    [Required] public UserRole Role { get; init; } = UserRole.User;
    [Required] [MaxLength(50)] public string Username { get; init; } = string.Empty;
    [Required] [MaxLength(128)] public string Password { get; init; } = string.Empty;
}