using System.Text;
using Org.BouncyCastle.Crypto.Digests;

namespace ManaxApi.Services;

public static class HashService
{
    public static string ComputeSha3_512(string input)
    {
        // Utilisation de SHA3-512 via BouncyCastle (nécessite l'ajout du package)
        Sha3Digest sha3 = new(512);
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        sha3.BlockUpdate(inputBytes, 0, inputBytes.Length);
        byte[] result = new byte[64];
        sha3.DoFinal(result, 0);
        return BitConverter.ToString(result).Replace("-", string.Empty).ToLower();
    }
}

