using ManaxServer.Models.User;
using ManaxServer.Services.Token;

namespace ManaxTests.Mocks;

public class MockTokenService : ITokenService
{
    private readonly List<string> _revokedTokens = [];
    public string GenerateToken(User user)
    {
        return $"{user.Id}-jwt-token";
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