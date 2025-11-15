using System.ComponentModel.DataAnnotations;

namespace ManaxLibrary.DTO.User;

public class UserDto
{
    public long Id { get; init; }
    [MaxLength(50)] public string Username { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public DateTime Creation { get; init; }
    public DateTime LastLogin { get; init; }
}