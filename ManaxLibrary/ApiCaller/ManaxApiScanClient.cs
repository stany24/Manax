using System.Net.Http.Json;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiScanClient
{
    public static async Task<bool> ScanLibraryAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/scan/library/{id}");
        return response.IsSuccessStatusCode;
    }

    public static async Task<bool> ScanSerieAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/scan/serie/{id}");
        return response.IsSuccessStatusCode;
    }

    public static async Task<bool> ScanChapterAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/scan/chapter/{id}");
        return response.IsSuccessStatusCode;
    }

    public static async Task<Dictionary<string, int>?> GetTasksAsync()
    {
        try
        {
            HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/scan/tasks");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<Dictionary<string, int>>();
        }
        catch (Exception)
        {
            return null;
        }
    }
}