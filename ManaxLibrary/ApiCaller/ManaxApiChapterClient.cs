using ManaxLibrary.DTO.Chapter;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiChapterClient
{
    public static async Task<Optional<List<long>>> GetChapterIdsAsync()
    {
        return await ManaxApiClient.GetAsync<List<long>>("api/chapters");
    }

    public static async Task<Optional<ChapterDto>> GetChapterAsync(long id)
    {
        return await ManaxApiClient.GetAsync<ChapterDto>($"api/chapter/{id}");
    }

    public static async Task<Optional<byte[]>> GetChapterPageAsync(long id, int number)
    {
        return await ManaxApiClient.GetBytesAsync($"api/chapter/{id}/page/{number}");
    }

    public static async Task<Optional<byte[]>> GetChapterPagesAsync(long id)
    {
        return await ManaxApiClient.GetBytesAsync($"api/chapter/{id}/pages");
    }

    public static async Task<Optional<bool>> DeleteChapterAsync(long id)
    {
        return await ManaxApiClient.DeleteAsync($"api/chapter/{id}");
    }
}