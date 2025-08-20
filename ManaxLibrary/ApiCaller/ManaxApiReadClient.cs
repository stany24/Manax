using System.Net.Http.Json;
using ManaxLibrary.DTO.Read;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiReadClient
{
    public static async Task<Optional<bool>> MarkAsRead(ReadCreateDto readCreateDto)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PutAsJsonAsync("api/read/read", readCreateDto);
        return response.IsSuccessStatusCode
            ? new Optional<bool>(true)
            : new Optional<bool>(response);
    }

    public static async Task<Optional<bool>> MarkAsUnread(long chapterId)
    {
        HttpResponseMessage response = await ManaxApiClient.Client.PutAsJsonAsync("api/read/unread", chapterId);
        return response.IsSuccessStatusCode
            ? new Optional<bool>(true)
            : new Optional<bool>(response);
    }
}