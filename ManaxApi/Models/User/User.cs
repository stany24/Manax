namespace ManaxApi.Models.User;

public class User
{
    public long Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public UserRole Role { get; set; }
}