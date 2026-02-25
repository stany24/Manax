using ManaxLibrary.DTO.Tag;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiTagClient
{
    public static async Task<Optional<List<TagDto>>> GetTagsAsync()
    {
        return await ManaxApiClient.GetAsync<List<TagDto>>("api/tags");
    }

    public static async Task<Optional<bool>> CreateTagAsync(TagCreateDto tagCreate)
    {
        return await ManaxApiClient.PostSuccessAsync("api/tag", tagCreate);
    }

    public static async Task<Optional<bool>> UpdateTagAsync(TagUpdateDto tag)
    {
        return await ManaxApiClient.PutSuccessAsync("api/tag", tag);
    }

    public static async Task<Optional<bool>> DeleteTagAsync(long id)
    {
        return await ManaxApiClient.DeleteAsync($"api/tag/{id}");
    }
}