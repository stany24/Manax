using System.Net.Http.Json;
using ManaxApi.Models.Chapter;

namespace ManaxApiClient;

public static class ManaxApiChapterClient
{
    public static async Task<List<long>?> GetChapterIdsAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/chapters");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<List<long>>();
    }

    public static async Task<Chapter?> GetChapterAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/chapter/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<Chapter>();
    }

    public static async Task<byte[]?> GetChapterPageAsync(long id, int number)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/chapter/{id}/page/{number}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadAsByteArrayAsync();
    }

    public static async Task<byte[]?> GetChapterPagesAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/chapter/{id}/pages");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadAsByteArrayAsync();
    }

    public static async Task<Chapter?> PostChapterAsync(Chapter chapter)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("api/chapter", chapter);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<Chapter>();
    }

    public static async Task<bool> PutChapterAsync(long id, Chapter chapter)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PutAsJsonAsync($"api/chapter/{id}", chapter);
        return response.IsSuccessStatusCode;
    }

    public static async Task<bool> DeleteChapterAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.DeleteAsync($"api/chapter/{id}");
        return response.IsSuccessStatusCode;
    }
}