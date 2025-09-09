namespace ManaxServer.Services.Token;

public class TokenInfo
{
    public long UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public List<ManaxLibrary.DTO.User.Permission> Permissions { get; set; } = [];
    public DateTime Expiry { get; set; }
}