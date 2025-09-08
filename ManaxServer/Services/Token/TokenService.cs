using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using ManaxServer.Models.User;
using Microsoft.IdentityModel.Tokens;

namespace ManaxServer.Services.Token;

public class TokenService : Service, ITokenService
{
    private const string Issuer = "ManaxServer";
    private const string Audience = "ManaxClient";
    private readonly string _secretKey;
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);
    private readonly List<string> _revokedTokens = [];

    public TokenService()
    {
        RandomNumberGenerator rng = RandomNumberGenerator.Create();
        byte[] bytes = new byte[48];
        rng.GetBytes(bytes);
        _secretKey = Convert.ToBase64String(bytes);
    }

    public string GenerateToken(User user)
    {
        JwtSecurityTokenHandler tokenHandler = new();
        byte[] key = Convert.FromBase64String(_secretKey);
        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            ]),
            Expires = DateTime.UtcNow.Add(TokenLifetime),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = Issuer,
            Audience = Audience
        };
        SecurityToken? token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public void RevokeToken(string? token)
    {
        if(token == null) return;
        _revokedTokens.Add(token);
    }

    public bool IsTokenRevoked(string token)
    {
        return _revokedTokens.Contains(token);
    }
}