using ManaxServer.Models.User;

namespace ManaxServer.Services.Token;

public interface ITokenService
{
    string GenerateToken(User user);
    void RevokeToken(string? token);
    bool IsTokenRevoked(string token);
    bool IsTokenValid(string token, out long userId);
    bool TokenHasPermission(string token, ManaxLibrary.DTO.User.Permission permission);
    TokenInfo? GetTokenInfo(string token);
}