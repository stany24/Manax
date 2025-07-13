// ReSharper disable PropertyCanBeMadeInitOnly.Global

using System.ComponentModel.DataAnnotations;

namespace ManaxServer.Models.User;

public class LoginAttempt
{
    public long Id { get; set; }
    [MaxLength(15)] public string Type { get; set; } = string.Empty;
    [MaxLength(45)] public string Origin { get; set; } = string.Empty;
    [MaxLength(50)] public string Username { get; set; } = string.Empty;
    public bool Success { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}