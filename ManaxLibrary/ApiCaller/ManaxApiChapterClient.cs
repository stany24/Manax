using System.Net.Http.Json;
using ManaxLibrary.DTO.Chapter;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiChapterClient
{
    public static async Task<Optional<List<long>>> GetChapterIdsAsync()
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/chapters");
            if (!response.IsSuccessStatusCode) return new Optional<List<long>>(response);
            List<long>? ids = await response.Content.ReadFromJsonAsync<List<long>>();
            return ids == null
                ? new Optional<List<long>>("Failed to read chapter IDs from response.")
                : new Optional<List<long>>(ids);
        });
    }

    public static async Task<Optional<ChapterDto>> GetChapterAsync(long id)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/chapter/{id}");
            if (!response.IsSuccessStatusCode) return new Optional<ChapterDto>(response);
            ChapterDto? chapter = await response.Content.ReadFromJsonAsync<ChapterDto>();
            return chapter == null
                ? new Optional<ChapterDto>($"Failed to read chapter with ID {id} from response.")
                : new Optional<ChapterDto>(chapter);
        });
    }

    public static async Task<Optional<byte[]>> GetChapterPageAsync(long id, int number)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/chapter/{id}/page/{number}");
            if (!response.IsSuccessStatusCode) return new Optional<byte[]>(response);
            byte[] data = await response.Content.ReadAsByteArrayAsync();
            return data.Length == 0
                ? new Optional<byte[]>($"Empty page data received for chapter ID {id}, page {number}.")
                : new Optional<byte[]>(data);
        });
    }

    public static async Task<Optional<byte[]>> GetChapterPagesAsync(long id)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/chapter/{id}/pages");
            if (!response.IsSuccessStatusCode) return new Optional<byte[]>(response);
            byte[] data = await response.Content.ReadAsByteArrayAsync();
            return data.Length == 0
                ? new Optional<byte[]>($"Empty pages data received for chapter ID {id}.")
                : new Optional<byte[]>(data);
        });
    }
    
    public static async Task<Optional<bool>> DeleteChapterAsync(long id)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.DeleteAsync($"api/chapter/{id}");
            return response.IsSuccessStatusCode
                ? new Optional<bool>(true)
                : new Optional<bool>(response);
        });
    }
}