using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using ManaxApi.Models.User;
using Microsoft.IdentityModel.Tokens;

namespace ManaxApi.Services;

public static class JwtService
{
    private static string? _secretKey;
    private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true };

    public static string GetSecretKey(IConfiguration config)
    {
        if (!string.IsNullOrEmpty(_secretKey))
            return _secretKey;
        // Tente de lire la clé depuis la configuration
        string? key = config["SecretKey"];
        if (!string.IsNullOrEmpty(key))
        {
            _secretKey = key;
            return _secretKey;
        }

        // Générer une clé aléatoire
        RandomNumberGenerator rng = RandomNumberGenerator.Create();
        byte[] bytes = new byte[48];
        rng.GetBytes(bytes);
        _secretKey = Convert.ToBase64String(bytes);
        // Ajouter la clé au fichier appsettings.json
        string configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        string json = File.ReadAllText(configPath);
        Dictionary<string, object> dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json) ??
                                          new Dictionary<string, object>();
        dict["SecretKey"] = _secretKey;
        string newJson = JsonSerializer.Serialize(dict, JsonSerializerOptions);
        File.WriteAllText(configPath, newJson);
        return _secretKey;
    }

    public static string GenerateToken(User user, IConfiguration config)
    {
        string secret = GetSecretKey(config);
        JwtSecurityTokenHandler tokenHandler = new();
        byte[] key = Convert.FromBase64String(secret);
        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            ]),
            Expires = DateTime.UtcNow.AddHours(12),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        SecurityToken? token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public static ClaimsPrincipal? ValidateToken(string token, IConfiguration config)
    {
        string secret = GetSecretKey(config);
        JwtSecurityTokenHandler tokenHandler = new();
        byte[] key = Convert.FromBase64String(secret);
        try
        {
            ClaimsPrincipal? principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out _);
            return principal;
        }
        catch
        {
            return null;
        }
    }
}