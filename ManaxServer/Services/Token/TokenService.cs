using System.Security.Cryptography;
using ManaxServer.Models.User;
using ManaxServer.Services.Permission;

namespace ManaxServer.Services.Token;

public class TokenService(IPermissionService permissionService) : Service, ITokenService
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);
    private readonly Dictionary<string, TokenInfo> _activeBearerTokens = [];
    private readonly List<string> _revokedTokens = [];

    public string GenerateToken(User user)
    {
        byte[] tokenBytes = new byte[32];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(tokenBytes);
        }
        string bearerToken = Convert.ToBase64String(tokenBytes);
        
        List<ManaxLibrary.DTO.User.Permission> userPermissions = permissionService.GetUserPermissions(user.Id).ToList();
        
        DateTime expiry = DateTime.UtcNow.Add(TokenLifetime);
        _activeBearerTokens[bearerToken] = new TokenInfo
        {
            UserId = user.Id,
            Username = user.Username,
            Permissions = userPermissions,
            Expiry = expiry
        };
        
        return bearerToken;
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