using System.Net.Http.Json;
using ManaxLibrary.DTO.Stats;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiStatsClient
{

    public static async Task<Optional<UserStats>> GetUserStats()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/stats");
        if (!response.IsSuccessStatusCode) return new Optional<UserStats>(response);
        UserStats? library = await response.Content.ReadFromJsonAsync<UserStats>();
        return library == null
            ? new Optional<UserStats>($"Failed to read user stats from response.")
            : new Optional<UserStats>(library);
    }
    
    public static async Task<Optional<ServerStats>> GetServerStats()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/server-stats");
        if (!response.IsSuccessStatusCode) return new Optional<ServerStats>(response);
        ServerStats? library = await response.Content.ReadFromJsonAsync<ServerStats>();
        return library == null
            ? new Optional<ServerStats>($"Failed to read server stats from response.")
            : new Optional<ServerStats>(library);
    }
}