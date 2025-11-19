using ManaxLibrary.DTO.User;
using ManaxServer.Models.User;
using ManaxServer.Services.Token;

namespace ManaxTests.Server.Mocks;

public class MockTokenService : ITokenService
{
    private readonly Dictionary<string, TokenInfo> _activeBearerTokens = [];
    private readonly List<string> _revokedTokens = [];

    public string GenerateToken(User user)
    {
        return $"{user.Id}-jwt-token";
    }

    public void RevokeToken(string? token)
    {
        if (token == null) return;
        _revokedTokens.Add(token);

        _activeBearerTokens.Remove(token);
    }

    public bool IsTokenRevoked(string token)
    {
        return _revokedTokens.Contains(token);
    }

    public bool IsTokenValid(string token, out long userId)
    {
        userId = 0;

        if (string.IsNullOrEmpty(token) || _revokedTokens.Contains(token))
            return false;

        if (!_activeBearerTokens.TryGetValue(token, out TokenInfo? tokenInfo))
            return false;

        if (DateTime.UtcNow > tokenInfo.Expiry)
        {
            _activeBearerTokens.Remove(token);
            return false;
        }

        userId = tokenInfo.UserId;
        return true;
    }

    public bool TokenHasPermission(string token, Permission permission)
    {
        if (string.IsNullOrEmpty(token) || _revokedTokens.Contains(token))
            return false;

        if (!_activeBearerTokens.TryGetValue(token, out TokenInfo? tokenInfo))
            return false;

        if (DateTime.UtcNow <= tokenInfo.Expiry) return tokenInfo.Permissions.Contains(permission);
        _activeBearerTokens.Remove(token);
        return false;
    }

    public TokenInfo? GetTokenInfo(string token)
    {
        if (string.IsNullOrEmpty(token) || _revokedTokens.Contains(token))
            return null;

        if (!_activeBearerTokens.TryGetValue(token, out TokenInfo? tokenInfo))
            return null;

        if (DateTime.UtcNow <= tokenInfo.Expiry) return tokenInfo;
        _activeBearerTokens.Remove(token);
        return null;
    }

    public void AddToken(string token, TokenInfo tokenInfo)
    {
        _activeBearerTokens[token] = tokenInfo;
    }
}