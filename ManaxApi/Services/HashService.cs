using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace ManaxApi.Services;

public static class HashService
{
    private const int MemorySize = 65536; // 64 MB
    private const int Iterations = 4; // Facteur de travail (nombre d'itérations)
    private const int DegreeOfParallelism = 4; // Nombre de threads à utiliser
    private const int SaltSize = 16; // Taille du sel en octets (128 bits)
    private const int HashSize = 32; // Taille du hash en octets (256 bits)

    private static byte[] GenerateSalt()
    {
        byte[] salt = new byte[SaltSize];
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return salt;
    }

    public static string HashPassword(string password)
    {
        byte[] salt = GenerateSalt();
        byte[] hash = HashPasswordWithSalt(password, salt);

        // Format: Base64(Salt):Base64(Hash)
        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }

    public static bool VerifyPassword(string password, string storedHash)
    {
        // Format Argon2id: Base64(Salt):Base64(Hash)
        string[] parts = storedHash.Split(':');
        if (parts.Length != 2)
            return false;

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