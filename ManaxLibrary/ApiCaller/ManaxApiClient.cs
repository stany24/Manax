using System.Net.Http.Headers;
using ManaxLibrary.Notifications;

namespace ManaxLibrary.ApiCaller;

internal static class ManaxApiClient
{
    internal static HttpClient Client = new()
    {
        BaseAddress = new Uri("http://127.0.0.1:5246/"),
        Timeout = TimeSpan.FromSeconds(5)
    };

    public static void SetHost(Uri host)
    {
        HttpClientHandler handler = new();
        Client = new HttpClient(handler)
        {
            BaseAddress = host,
            Timeout = TimeSpan.FromSeconds(5)
        };
    }

    public static void SetToken(string token)
    {
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (Client.BaseAddress == null) return;
        _ = ServerNotification.InitializeAsync(Client.BaseAddress, token);
    }

    public static void ResetToken()
    {
        Client.DefaultRequestHeaders.Authorization = null;
    }

    internal static async Task<Optional<T>> ExecuteWithErrorHandlingAsync<T>(Func<Task<Optional<T>>> apiCall)
    {
        try
        {
            return await apiCall();
        }
        catch (TaskCanceledException)
        {
            return new Optional<T>("Timeout");
        }
        catch (Exception e)
        {
            return new Optional<T>("Exception: " + e.Message);
        }
    }
}