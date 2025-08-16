namespace ManaxServer.Services.Hash;

public interface IHashService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string storedHash);
}