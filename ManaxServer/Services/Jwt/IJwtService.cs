using ManaxServer.Models.User;

namespace ManaxServer.Services.Jwt;

public interface IJwtService
{
    string GetSecretKey();
    string GenerateToken(User user);
}