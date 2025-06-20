// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel.DataAnnotations;

namespace ManaxApi.Models.User;

public class User
{
    public long Id { get; set; }
    [MaxLength(50)] public string Username { get; set; } = string.Empty;
    [MaxLength(128)] public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}