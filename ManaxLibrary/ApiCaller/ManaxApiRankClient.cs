using System.Net.Http.Json;
using ManaxLibrary.DTOs.Rank;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiRankClient
{
    public static async Task<Optional<List<RankDTO>>> GetRanksAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("/api/ranks");
        if (!response.IsSuccessStatusCode) return new Optional<List<RankDTO>>(response);
        List<RankDTO>? ranks = await response.Content.ReadFromJsonAsync<List<RankDTO>>();
        return ranks == null
            ? new Optional<List<RankDTO>>("Failed to read ranks from response.")
            : new Optional<List<RankDTO>>(ranks);
    }

    public static async Task<Optional<bool>> CreateRankAsync(RankCreateDTO rank)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("/api/rank", rank);
        return response.IsSuccessStatusCode
            ? new Optional<bool>(true)
            : new Optional<bool>(response);
    }

    public static async Task<Optional<bool>> UpdateRankAsync(RankDTO rank)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PutAsJsonAsync($"/api/rank/{rank.Id}", rank);
        return response.IsSuccessStatusCode
            ? new Optional<bool>(true)
            : new Optional<bool>(response);
    }

    public static async Task<Optional<bool>> DeleteRankAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.DeleteAsync($"/api/rank/{id}");
        return response.IsSuccessStatusCode
            ? new Optional<bool>(true)
            : new Optional<bool>(response);
    }

    public static async Task<Optional<bool>> SetUserRankAsync(UserRankCreateDTO rank)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("/api/rank/set", rank);
        return response.IsSuccessStatusCode
            ? new Optional<bool>(true)
            : new Optional<bool>(response);
    }

    public static async Task<Optional<List<UserRankDTO>>> GetRankingAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("/api/ranking");
        if (!response.IsSuccessStatusCode) return new Optional<List<UserRankDTO>>(response);
        List<UserRankDTO>? ranking = await response.Content.ReadFromJsonAsync<List<UserRankDTO>>();
        return ranking == null
            ? new Optional<List<UserRankDTO>>("Failed to read user ranking from response.")
            : new Optional<List<UserRankDTO>>(ranking);
    }
}