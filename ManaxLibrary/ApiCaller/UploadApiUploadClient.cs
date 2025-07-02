using System.Net.Http.Headers;

namespace ManaxLibrary.ApiCaller;

public static class UploadApiUploadClient
{
    public static async Task<HttpResponseMessage> UploadSerieAsync(ByteArrayContent file, string fileName, long libraryId)
    {
        using MultipartFormDataContent content = new();
        file.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");
        content.Add(file, "file", fileName);
        content.Add(new StringContent(libraryId.ToString()), "libraryId");
        return await ManaxApiClient.Client.PostAsync("api/upload/serie", content);
    }

    public static async Task<HttpResponseMessage> UploadChapterAsync(ByteArrayContent file, string fileName, long serieId)
    {
        using MultipartFormDataContent content = new();
        file.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");
        content.Add(file, "file", fileName);
        content.Add(new StringContent(serieId.ToString()), "serieId");
        return await ManaxApiClient.Client.PostAsync("api/upload/serie", content);
    }

    public static async Task<HttpResponseMessage> ReplaceChapterAsync(ByteArrayContent file, string fileName, long serieId)
    {
        using MultipartFormDataContent content = new();
        file.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");
        content.Add(file, "file", fileName);
        content.Add(new StringContent(serieId.ToString()), "serieId");
        return await ManaxApiClient.Client.PostAsync("api/upload/serie", content);
    }
}

