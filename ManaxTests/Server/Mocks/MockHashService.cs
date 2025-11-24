using ManaxServer.Services.Hash;

namespace ManaxTests.Server.Mocks;

public class MockHashService : IHashService
{
    private const string HashedPassword = "hashed";
    private List<string> HashPasswordCalls { get; } = [];
    private List<(string password, string storedHash)> VerifyPasswordCalls { get; } = [];

    public string HashPassword(string password)
    {
        HashPasswordCalls.Add(password);
        return password + HashedPassword;
    }

    public bool VerifyPassword(string password, string storedHash)
    {
        VerifyPasswordCalls.Add((password, storedHash));
        return storedHash == password + HashedPassword;
    }

    public void VerifyHashPasswordCalled(string expectedPassword)
    {
        if (!HashPasswordCalls.Contains(expectedPassword))
            throw new Exception();
    }

    public void VerifyHashPasswordNotCalled()
    {
        if (HashPasswordCalls.Count != 0)
            throw new Exception();
    }

    public void VerifyVerifyPasswordCalled(string expectedPassword)
    {
        if (VerifyPasswordCalls.All(call => call.password != expectedPassword))
            throw new Exception();
    }

    public void Reset()
    {
        HashPasswordCalls.Clear();
        VerifyPasswordCalls.Clear();
    }
}