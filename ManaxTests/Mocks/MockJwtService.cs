using ManaxServer.Models.User;
using ManaxServer.Services.Jwt;

namespace ManaxTests.Mocks;

public class MockJwtService : IJwtService
{
    public string GetSecretKey()
    {
        return "test-secret-key";
    }

    public string GenerateToken(User user)
    {
        return $"{user.Id}-jwt-token";
    }
}