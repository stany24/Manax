using System.Net.Http.Json;
using ManaxApi.Models.Serie;

namespace ManaxApiClient;

public static class ManaxApiSerieClient
{
    public static async Task<List<long>?> GetSeriesIdsAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/series");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<List<long>>();
    }

    public static async Task<SerieInfo?> GetSerieInfoAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/serie/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<SerieInfo>();
    }

    public static async Task<List<long>?> GetSerieChaptersAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/series/{id}/chapters");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<List<long>>();
    }

    public static async Task<Serie?> PostSerieAsync(Serie serie)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("api/serie", serie);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<Serie>();
    }

    public static async Task<bool> PutSerieAsync(long id, Serie serie)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PutAsJsonAsync($"api/serie/{id}", serie);
        return response.IsSuccessStatusCode;
    }

    public static async Task<bool> DeleteSerieAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.DeleteAsync($"api/serie/{id}");
        return response.IsSuccessStatusCode;
    }
}