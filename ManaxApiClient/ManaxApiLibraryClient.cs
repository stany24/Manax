using System.Net.Http.Json;
using ManaxApi.Models.Library;

namespace ManaxApiClient;

public static class ManaxApiLibraryClient
{
    public static async Task<List<long>?> GetLibraryIdsAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/libraries");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<List<long>>();
    }

    public static async Task<LibraryInfo?> GetLibraryInfoAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/library/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<LibraryInfo>();
    }

    public static async Task<List<long>?> GetLibrarySeriesAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/library/{id}/series");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<List<long>>();
    }

    public static async Task<long?> PostLibraryAsync(Library library)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("api/library/create", library);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<long>();
    }

    public static async Task<bool> PutLibraryAsync(long id, Library library)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PutAsJsonAsync($"api/library/{id}", library);
        return response.IsSuccessStatusCode;
    }

    public static async Task<bool> DeleteLibraryAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.DeleteAsync($"api/library/{id}");
        return response.IsSuccessStatusCode;
    }
}