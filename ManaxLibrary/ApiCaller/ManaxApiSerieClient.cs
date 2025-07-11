using System.Net.Http.Json;
using ManaxLibrary.DTOs.Serie;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiSerieClient
{
    public static async Task<List<long>?> GetSeriesIdsAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/series");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<List<long>>();
    }

    public static async Task<SerieDTO?> GetSerieInfoAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/serie/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<SerieDTO>();
    }

    public static async Task<List<long>?> GetSerieChaptersAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/series/{id}/chapters");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<List<long>>();
    }

    public static async Task<long?> PostSerieAsync(SerieCreateDTO serieCreate)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("/api/serie", serieCreate);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<long>();
    }

    public static async Task<bool> PutSerieAsync(long id, SerieUpdateDTO serieUpdate)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PutAsJsonAsync($"api/serie/{id}", serieUpdate);
        return response.IsSuccessStatusCode;
    }

    public static async Task<bool> DeleteSerieAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.DeleteAsync($"api/serie/{id}");
        return response.IsSuccessStatusCode;
    }

    public static async Task<byte[]?> GetSeriePosterAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/serie/{id}/poster");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadAsByteArrayAsync();
    }
}