using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace ManaxServer.Services.Hash;

public class HashService : Service, IHashService
{
    // Format: Base64(Salt):Base64(Hash)
    private const int MemorySize = 64; // kB
    private const int Iterations = 4;
    private const int DegreeOfParallelism = 4; // threads
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 32; // 256 bits

    public string HashPassword(string password)
    {
        byte[] salt = GenerateSalt();
        byte[] hash = HashPasswordWithSalt(password, salt);


        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public bool VerifyPassword(string password, string storedHash)
    {
        string[] parts = storedHash.Split(':');
        if (parts.Length != 2) return false;

        try
        {
            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] expectedHash = Convert.FromBase64String(parts[1]);
            byte[] actualHash = HashPasswordWithSalt(password, salt);
            return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
        }
        catch
        {
            return false;
        }
    }

    private static byte[] GenerateSalt()
    {
        byte[] salt = new byte[SaltSize];
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }

    private static byte[] HashPasswordWithSalt(string password, byte[] salt)
    {
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

        using Argon2id argon2 = new(passwordBytes)
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySize,
            Iterations = Iterations
        };

        return argon2.GetBytes(HashSize);
    }
}