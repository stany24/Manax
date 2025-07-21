using System.Net.Http.Json;
using ManaxLibrary.DTOs;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiChapterClient
{
    public static async Task<Optional<List<long>>> GetChapterIdsAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/chapters");
        if (!response.IsSuccessStatusCode) return new Optional<List<long>>(response);
        List<long>? ids = await response.Content.ReadFromJsonAsync<List<long>>();
        return ids == null
            ? new Optional<List<long>>("Failed to read chapter IDs from response.")
            : new Optional<List<long>>(ids);
    }

    public static async Task<Optional<ChapterDTO>> GetChapterAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/chapter/{id}");
        if (!response.IsSuccessStatusCode) return new Optional<ChapterDTO>(response);
        ChapterDTO? chapter = await response.Content.ReadFromJsonAsync<ChapterDTO>();
        return chapter == null
            ? new Optional<ChapterDTO>($"Failed to read chapter with ID {id} from response.")
            : new Optional<ChapterDTO>(chapter);
    }

    public static async Task<Optional<byte[]>> GetChapterPageAsync(long id, int number)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/chapter/{id}/page/{number}");
        if (!response.IsSuccessStatusCode) return new Optional<byte[]>(response);
        byte[] data = await response.Content.ReadAsByteArrayAsync();
        return data.Length == 0
            ? new Optional<byte[]>($"Empty page data received for chapter ID {id}, page {number}.")
            : new Optional<byte[]>(data);
    }

    public static async Task<Optional<byte[]>> GetChapterPagesAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync($"api/chapter/{id}/pages");
        if (!response.IsSuccessStatusCode) return new Optional<byte[]>(response);
        byte[] data = await response.Content.ReadAsByteArrayAsync();
        return data.Length == 0
            ? new Optional<byte[]>($"Empty pages data received for chapter ID {id}.")
            : new Optional<byte[]>(data);
    }

    public static async Task<Optional<ChapterDTO>> PostChapterAsync(ChapterDTO chapter)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("api/chapter", chapter);
        if (!response.IsSuccessStatusCode) return new Optional<ChapterDTO>(response);
        ChapterDTO? createdChapter = await response.Content.ReadFromJsonAsync<ChapterDTO>();
        return createdChapter == null
            ? new Optional<ChapterDTO>("Failed to read created chapter from response.")
            : new Optional<ChapterDTO>(createdChapter);
    }

    public static async Task<Optional<bool>> PutChapterAsync(long id, ChapterDTO chapter)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PutAsJsonAsync($"api/chapter/{id}", chapter);
        return response.IsSuccessStatusCode
            ? new Optional<bool>(true)
            : new Optional<bool>(response);
    }

    public static async Task<Optional<bool>> DeleteChapterAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.DeleteAsync($"api/chapter/{id}");
        return response.IsSuccessStatusCode
            ? new Optional<bool>(true)
            : new Optional<bool>(response);
    }
}