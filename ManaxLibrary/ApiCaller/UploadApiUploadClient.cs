using System.Net.Http.Headers;
using ManaxLibrary.DTO.Serie;

namespace ManaxLibrary.ApiCaller;

public static class UploadApiUploadClient
{
    public static async Task<Optional<bool>> UploadSerieAsync(string directory, long libraryId)
    {
        SerieCreateDto serieCreate = new()
        {
            Title = Path.GetFileName(directory[..directory.LastIndexOf(Path.DirectorySeparatorChar)]),
            LibraryId = libraryId
        };

        Optional<long> serieCreateResponse = await ManaxApiSerieClient.PostSerieAsync(serieCreate);
        if (serieCreateResponse.Failed)
            return new Optional<bool>(serieCreateResponse.Error);

        long serieId = serieCreateResponse.GetValue();

        string? poster = Directory.GetFiles(directory, "*poster.*").FirstOrDefault();
        string? posterError = null;
        if (poster != null)
        {
            Optional<bool> posterResult = await UploadPosterAsync(poster, Path.GetFileName(poster), serieId);
            if (posterResult.Failed)
                posterError = posterResult.Error;
        }

        foreach (string filePath in Directory.GetFiles(directory, "*.cbz"))
        {
            await using FileStream fileStream = File.OpenRead(filePath);
            ByteArrayContent fileContent = new(await File.ReadAllBytesAsync(filePath));
            string fileName = Path.GetFileName(filePath);
            Optional<bool> uploadChapterResponse = await UploadChapterAsync(fileContent, fileName, serieId);
            if (uploadChapterResponse.Failed)
                return new Optional<bool>(uploadChapterResponse.Error);
        }

        return new Optional<bool>(posterError == null);
    }

    public static async Task<Optional<bool>> UploadChapterAsync(ByteArrayContent file, string fileName,
        long serieId)
    {
        using MultipartFormDataContent content = new();
        file.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");
        content.Add(file, "file", fileName);
        content.Add(new StringContent(serieId.ToString()), "serieId");
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsync("api/upload/chapter", content);
        return new Optional<bool>(response.IsSuccessStatusCode);
    }

    public static async Task<Optional<bool>> ReplaceChapterAsync(ByteArrayContent file, string fileName,
        long serieId)
    {
        using MultipartFormDataContent content = new();
        file.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");
        content.Add(file, "file", fileName);
        content.Add(new StringContent(serieId.ToString()), "serieId");
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsync("api/upload/chapter/replace", content);
        return new Optional<bool>(response.IsSuccessStatusCode);
    }

    public static async Task<Optional<bool>> UploadPosterAsync(string file, string fileName, long serieId)
    {
        using MultipartFormDataContent content = new();
        ByteArrayContent img = new(await File.ReadAllBytesAsync(file));
        img.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");
        content.Add(img, "file", fileName);
        content.Add(new StringContent(serieId.ToString()), "serieId");
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsync("api/upload/poster", content);
        return new Optional<bool>(response.IsSuccessStatusCode);
    }

    public static async Task<Optional<bool>> ReplacePosterAsync(string file, string fileName, long serieId)
    {
        using MultipartFormDataContent content = new();
        ByteArrayContent img = new(await File.ReadAllBytesAsync(file));
        img.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");
        content.Add(img, "file", fileName);
        content.Add(new StringContent(serieId.ToString()), "serieId");
        HttpResponseMessage response = await ManaxApiClient.Client.PostAsync("api/upload/poster/replace", content);
        return new Optional<bool>(response.IsSuccessStatusCode);
    }
}