using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using ManaxServer.Models.User;
using Microsoft.IdentityModel.Tokens;

namespace ManaxServer.Services.Jwt;

public class JwtService : Service, IJwtService
{
    private const string Issuer = "ManaxServer";
    private const string Audience = "ManaxClient";
    private string? _secretKey;

    public string GetSecretKey()
    {
        if (!string.IsNullOrEmpty(_secretKey))
            return _secretKey;

        RandomNumberGenerator rng = RandomNumberGenerator.Create();
        byte[] bytes = new byte[48];
        rng.GetBytes(bytes);
        _secretKey = Convert.ToBase64String(bytes);
        return _secretKey;
    }

    public string GenerateToken(User user)
    {
        string secret = GetSecretKey();
        JwtSecurityTokenHandler tokenHandler = new();
        byte[] key = Convert.FromBase64String(secret);
        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            ]),
            Expires = DateTime.UtcNow.AddHours(12),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = Issuer,
            Audience = Audience
        };
        SecurityToken? token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}