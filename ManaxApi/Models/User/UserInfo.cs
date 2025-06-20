namespace ManaxApi.Models.User;

public class UserInfo
{
    public string Username { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}