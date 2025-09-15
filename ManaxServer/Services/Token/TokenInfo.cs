namespace ManaxServer.Services.Token;

public class TokenInfo
{
    public long UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public List<ManaxLibrary.DTO.User.Permission> Permissions { get; init; } = [];
    public DateTime Expiry { get; init; }
}