using System.Net.Http.Json;
using ManaxLibrary.DTOs.Rank;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiRankClient
{
    public static async Task<List<RankDTO>?> GetRanksAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("/api/ranks");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<List<RankDTO>>();
    }

    public static async Task<bool> CreateRankAsync(RankDTO rank)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("/api/rank", rank);
        return response.IsSuccessStatusCode;
    }

    public static async Task<bool> UpdateRankAsync(long id, RankDTO rank)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PutAsJsonAsync($"/api/rank/{id}", rank);
        return response.IsSuccessStatusCode;
    }

    public static async Task<bool> DeleteRankAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.DeleteAsync($"/api/rank/{id}");
        return response.IsSuccessStatusCode;
    }

    public static async Task<bool> SetUserRankAsync(UserRankCreateDTO rank)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("/api/rank/set",rank);
        return response.IsSuccessStatusCode;
    }

    public static async Task<List<UserRankDTO>?> GetRankingAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("/api/ranking");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<List<UserRankDTO>>();
    }
    
}