using ManaxServer.Models.User;

namespace ManaxServer.Services.Token;

public interface ITokenService
{
    string GenerateToken(User user);
    void RevokeToken(string? token);
    bool IsTokenRevoked(string token);
}