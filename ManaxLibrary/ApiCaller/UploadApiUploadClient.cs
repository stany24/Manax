using System.Net.Http.Headers;
using ManaxLibrary.DTOs.Serie;

namespace ManaxLibrary.ApiCaller;

public static class UploadApiUploadClient
{
    public static async Task<bool> UploadSerieAsync(string directory, long libraryId)
    {
        SerieCreateDTO serieCreate = new()
        {
            Title = Path.GetFileName(directory[..directory.LastIndexOf(Path.DirectorySeparatorChar)]),
            LibraryId = libraryId
        };
        long? serieId = await ManaxApiSerieClient.PostSerieAsync(serieCreate);
        if (serieId == null)
            return false;

        foreach (string filePath in Directory.GetFiles(directory, "*.cbz"))
        {
            await using FileStream fileStream = File.OpenRead(filePath);
            ByteArrayContent fileContent = new(await File.ReadAllBytesAsync(filePath));
            string fileName = Path.GetFileName(filePath);
            HttpResponseMessage chapterResponse = await UploadChapterAsync(fileContent, fileName, (long)serieId);
            if (!chapterResponse.IsSuccessStatusCode)
                return false;
        }

        string? poster = Directory.GetFiles(directory, "*poster.*").FirstOrDefault();
        if (poster != null)
        {
            HttpResponseMessage chapterResponse = await UploadPosterAsync(poster, Path.GetFileName(poster), (long)serieId);
            if (!chapterResponse.IsSuccessStatusCode)
                return false;
        }
        
        return true;
    }

    public static async Task<HttpResponseMessage> UploadChapterAsync(ByteArrayContent file, string fileName, long serieId)
    {
        using MultipartFormDataContent content = new();
        file.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");
        content.Add(file, "file", fileName);
        content.Add(new StringContent(serieId.ToString()), "serieId");
        return await ManaxApiClient.Client.PostAsync("api/upload/chapter", content);
    }

    public static async Task<HttpResponseMessage> ReplaceChapterAsync(ByteArrayContent file, string fileName, long serieId)
    {
        using MultipartFormDataContent content = new();
        file.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");
        content.Add(file, "file", fileName);
        content.Add(new StringContent(serieId.ToString()), "serieId");
        return await ManaxApiClient.Client.PostAsync("api/upload/chapter/replace", content);
    }
    
    public static async Task<HttpResponseMessage> UploadPosterAsync(string file, string fileName, long serieId)
    {
        using MultipartFormDataContent content = new();
        ByteArrayContent img = new(await File.ReadAllBytesAsync(file));
        img.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");
        content.Add(img, "file", fileName);
        content.Add(new StringContent(serieId.ToString()), "serieId");
        return await ManaxApiClient.Client.PostAsync("api/upload/poster", content);
    }

    public static async Task<HttpResponseMessage> ReplacePosterAsync(string file, string fileName, long serieId)
    {
        using MultipartFormDataContent content = new();
        ByteArrayContent img = new(await File.ReadAllBytesAsync(file));
        img.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");
        content.Add(img, "file", fileName);
        content.Add(new StringContent(serieId.ToString()), "serieId");
        return await ManaxApiClient.Client.PostAsync("api/upload/poster/replace", content);
    }
}
