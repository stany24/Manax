using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ManaxApiCaller;

public static class ManaxApiCaller
{
    internal static readonly HttpClient Client = new() { BaseAddress = new Uri("http://localhost:5000/") };

    public static void SetHost(Uri host)
    {
        Client.BaseAddress = host;
    }

    public static void SetToken(string? token)
    {
        Client.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token) ? null : new AuthenticationHeaderValue("Bearer", token);
    }

    public static async Task<string?> LoginAsync(string username, string password)
    {
        HttpResponseMessage response = await Client.PostAsJsonAsync("api/user/login", new { username, password });
        if (!response.IsSuccessStatusCode) return null;
        string json = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("token").GetString();
    }
}