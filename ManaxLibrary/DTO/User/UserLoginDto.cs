using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTO.User;

public class UserLoginDto
{
    [Required] public string Username { get; init; } = string.Empty;
    [Required] public string Password { get; init; } = string.Empty;
}