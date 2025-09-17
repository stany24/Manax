using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ManaxLibrary.DTO.Tag;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiTagClient
{
    public static async Task<Optional<List<TagDto>>> GetTagsAsync()
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/tags");
            if (!response.IsSuccessStatusCode) return new Optional<List<TagDto>>(response);
            List<TagDto>? issues =
                await response.Content.ReadFromJsonAsync<List<TagDto>>();
            return issues == null
                ? new Optional<List<TagDto>>("Failed to read automatic chapter issues from response.")
                : new Optional<List<TagDto>>(issues);
        });
    }

    public static async Task<Optional<bool>> CreateTagAsync(TagCreateDto tagCreate)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("api/tag", tagCreate);
        return response.IsSuccessStatusCode ? new Optional<bool>(true) : new Optional<bool>(response);
    }

    public static async Task UpdateTagAsync(TagUpdateDto tag)
    {
        string json = JsonSerializer.Serialize(tag);
        StringContent content = new(json, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await ManaxApiClient.Client.PutAsync("api/tag", content);
        response.EnsureSuccessStatusCode();
    }

    public static async Task DeleteTagAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.DeleteAsync($"api/tag/{id}");
        response.EnsureSuccessStatusCode();
    }
}