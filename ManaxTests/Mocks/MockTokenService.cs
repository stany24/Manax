using ManaxLibrary.DTO.User;
using ManaxServer.Models.User;
using ManaxServer.Services.Token;

namespace ManaxTests.Mocks;

public class MockTokenService : ITokenService
{
    private readonly List<string> _revokedTokens = [];
    private readonly Dictionary<string, TokenInfo> _activeBearerTokens = [];
    
    public string GenerateToken(User user)
    {
        return $"{user.Id}-jwt-token";
    }

    public void RevokeToken(string? token)
    {
        if(token == null) return;
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

    public bool TokenHasPermission(string token, ManaxLibrary.DTO.User.Permission permission)
    {
        if (string.IsNullOrEmpty(token) || _revokedTokens.Contains(token))
            return false;
            
        if (!_activeBearerTokens.TryGetValue(token, out TokenInfo? tokenInfo))
            return false;
            
        if (DateTime.UtcNow > tokenInfo.Expiry)
        {
            _activeBearerTokens.Remove(token);
            return false;
        }
        
        return tokenInfo.Permissions.Contains(permission);
    }

    public TokenInfo? GetTokenInfo(string token)
    {
        if (string.IsNullOrEmpty(token) || _revokedTokens.Contains(token))
            return null;
            
        if (!_activeBearerTokens.TryGetValue(token, out TokenInfo? tokenInfo))
            return null;
            
        if (DateTime.UtcNow > tokenInfo.Expiry)
        {
            _activeBearerTokens.Remove(token);
            return null;
        }
        
        return tokenInfo;
    }
}