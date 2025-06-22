using System.Net.Http.Json;
using ManaxLibrary.DTOs;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiLibraryClient
{
    public static async Task<List<long>?> GetLibraryIdsAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/libraries");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<List<long>>();
    }

    public static async Task<LibraryDTO?> GetLibraryAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/library/{id}");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<LibraryDTO>();
    }

    public static async Task<List<long>?> GetLibrarySeriesAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/library/{id}/series");
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<List<long>>();
    }

    public static async Task<long?> PostLibraryAsync(LibraryCreateDTO library)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("api/library/create", library);
        if (!response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<long>();
    }

    public static async Task<bool> PutLibraryAsync(long id, LibraryCreateDTO library)
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