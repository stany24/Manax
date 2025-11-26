using ManaxLibrary.DTO.Stats;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiStatsClient
{
    public static async Task<Optional<UserStats>> GetUserStats()
    {
        return await ManaxApiClient.GetAsync<UserStats>("api/stats/self");
    }

    public static async Task<Optional<ServerStats>> GetServerStats()
    {
        return await ManaxApiClient.GetAsync<ServerStats>("api/stats/server");
    }
}