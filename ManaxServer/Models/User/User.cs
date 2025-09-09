// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

using System.ComponentModel.DataAnnotations;
using ManaxLibrary.DTO.User;
using Microsoft.EntityFrameworkCore;

namespace ManaxServer.Models.User;

[Index(nameof(Username), IsUnique = true)]
public class User
{
    public long Id { get; set; }
    [MaxLength(50)] public string Username { get; set; } = string.Empty;
    [MaxLength(128)] public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }

    public DateTime Creation { get; set; }
    public DateTime LastLogin { get; set; }
    
    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}