// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace ManaxApi.Models.User;

public class UserInfo
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}