using ManaxLibrary.DTO.Read;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiReadClient
{
    public static async Task<Optional<bool>> MarkAsRead(ReadCreateDto readCreateDto)
    {
        return await ManaxApiClient.PutSuccessAsync("api/read/read", readCreateDto);
    }

    public static async Task<Optional<bool>> MarkAsUnread(long chapterId)
    {
        return await ManaxApiClient.PutSuccessAsync("api/read/unread", chapterId);
    }
}