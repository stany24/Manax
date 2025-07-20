using System.Net.Http.Headers;
using ManaxLibrary.Notifications;

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

    public static void SetToken(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        _ = ServerNotification.InitializeAsync(Client.BaseAddress,token);
    }
}