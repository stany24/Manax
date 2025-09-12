using System.Text;
using System.Text.Json;
using ManaxLibrary.DTO.Tag;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiTagClient
{
    public static async Task<List<TagDto>> GetTagsAsync()
    {
        HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("/api/tags");
        response.EnsureSuccessStatusCode();
        string json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<TagDto>>(json) ?? [];
    }

    public static async Task<long> CreateTagAsync(TagCreateDto tagCreate)
    {
        string json = JsonSerializer.Serialize(tagCreate);
        StringContent content = new(json, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsync("/api/tag", content);
        response.EnsureSuccessStatusCode();
        string responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<long>(responseJson);
    }

    public static async Task UpdateTagAsync(TagDto tag)
    {
        string json = JsonSerializer.Serialize(tag);
        StringContent content = new(json, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await ManaxApiClient.Client.PutAsync("/api/tag", content);
        response.EnsureSuccessStatusCode();
    }

    public static async Task DeleteTagAsync(long id)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.DeleteAsync($"/api/tag/{id}");
        response.EnsureSuccessStatusCode();
    }
}
