using System.Text;
using Org.BouncyCastle.Crypto.Digests;

namespace ManaxApi.Services;

public static class HashService
{
    public static string ComputeSha3_512(string input)
    {
        Sha3Digest sha3 = new(512);
        byte[] inputBytes = Encoding.UTF8.GetBytes(input);
        sha3.BlockUpdate(inputBytes, 0, inputBytes.Length);
        byte[] result = new byte[64];
        sha3.DoFinal(result, 0);
        return BitConverter.ToString(result).Replace("-", string.Empty).ToLower();
    }
}