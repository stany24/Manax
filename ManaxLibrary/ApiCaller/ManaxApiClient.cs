using System.Net.Http.Headers;

namespace ManaxLibrary.ApiCaller;

internal static class ManaxApiClient
{
    internal static HttpClient Client = new() { BaseAddress = new Uri("http://127.0.0.1:5246/") };

    public static void SetHost(Uri host)
    {
        HttpClientHandler handler = new();
        Client = new HttpClient(handler)
        {
            BaseAddress = host,
            Timeout = TimeSpan.FromMinutes(30)
        };
    }

    public static void SetToken(string? token)
    {
        Client.DefaultRequestHeaders.Authorization =
            string.IsNullOrEmpty(token) ? null : new AuthenticationHeaderValue("Bearer", token);
    }
}