using System.Net.Http.Headers;

namespace ManaxApiClient;

internal static class ManaxApiClient
{
    internal static HttpClient Client = new() { BaseAddress = new Uri("http://127.0.0.1:5246/") };

    public static void SetHost(Uri host)
    {
        Client = new HttpClient { BaseAddress = host };
    }

    public static void SetToken(string? token)
    {
        Client.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token) ? null : new AuthenticationHeaderValue("Bearer", token);
    }
}