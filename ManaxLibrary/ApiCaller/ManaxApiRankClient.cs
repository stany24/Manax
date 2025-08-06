using System.Net.Http.Json;
using ManaxLibrary.DTO.Rank;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiRankClient
{
    public static async Task<Optional<List<RankDto>>> GetRanksAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("/api/ranks");
        if (!response.IsSuccessStatusCode) return new Optional<List<RankDto>>(response);
        List<RankDto>? ranks = await response.Content.ReadFromJsonAsync<List<RankDto>>();
        return ranks == null
            ? new Optional<List<RankDto>>("Failed to read ranks from response.")
            : new Optional<List<RankDto>>(ranks);
    }

    public static async Task<Optional<bool>> CreateRankAsync(RankCreateDto rank)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("/api/rank", rank);
        return response.IsSuccessStatusCode
            ? new Optional<bool>(true)
            : new Optional<bool>(response);
    }

    public static async Task<Optional<bool>> UpdateRankAsync(RankDto rank)
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

    public static async Task<Optional<bool>> SetUserRankAsync(UserRankCreateDto rank)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("/api/rank/set", rank);
        return response.IsSuccessStatusCode
            ? new Optional<bool>(true)
            : new Optional<bool>(response);
    }

    public static async Task<Optional<List<UserRankDto>>> GetRankingAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("/api/ranking");
        if (!response.IsSuccessStatusCode) return new Optional<List<UserRankDto>>(response);
        List<UserRankDto>? ranking = await response.Content.ReadFromJsonAsync<List<UserRankDto>>();
        return ranking == null
            ? new Optional<List<UserRankDto>>("Failed to read user ranking from response.")
            : new Optional<List<UserRankDto>>(ranking);
    }
}