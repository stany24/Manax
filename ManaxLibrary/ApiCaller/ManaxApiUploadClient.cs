using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Serie;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiUploadClient
{
    private static readonly string[] ChapterNumberPatterns =
    [
        "CH\\d{1,4}",
        "(?i)chapter[-_ ]\\d{1,4}",
        "(?i)episode[-_ ][-_ ]\\d{1,4}",
        "(?i)episode[-_ ]\\d{1,4}",
        "(?i)chap[-_ ]\\d{1,4}",
        "(?i)ch.[-_ ]*\\d{1,4}",
        "(?i)ep.[-_ ]*\\d{1,4}",
        "(?i)Flight[-_ ]\\d{1,4}",
        "\\d{1,4}"
    ];
    
    private static int ExtractChapterNumber(string fileName)
    {
        foreach (string pattern in ChapterNumberPatterns)
        {
            Regex regex = new(pattern);
            Match match = regex.Match(fileName);
            if (!match.Success) continue;
            string numberStr = Regex.Replace(match.Value, @"[^\d]", "");
            if (int.TryParse(numberStr, out int number))
            {
                return number;
            }
        }
        return 0;
    }
    
    public static async Task<Optional<bool>> UploadSerieAsync(string directory)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            SerieCreateDto serieCreate = new()
            {
                Title = Path.GetFileName(directory[..directory.LastIndexOf(Path.DirectorySeparatorChar)])
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
                NewChapterDto newChapterDto = new()
                {
                    Data = await File.ReadAllBytesAsync(filePath),
                    SerieId = (int)serieId,
                    Number = ExtractChapterNumber(Path.GetFileName(filePath))
                };
                Optional<bool> uploadChapterResponse = await UploadChapterAsync(newChapterDto);
                if (uploadChapterResponse.Failed)
                    return new Optional<bool>(uploadChapterResponse.Error);
            }

            return new Optional<bool>(posterError == null);
        });
    }

    public static async Task<Optional<bool>> UploadChapterAsync(NewChapterDto dto)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.PostAsJsonAsync("api/upload/chapter", dto);
            return new Optional<bool>(response.IsSuccessStatusCode);
        });
    }

    public static async Task<Optional<bool>> ReplaceChapterAsync(ByteArrayContent file, string fileName,
        long serieId)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            using MultipartFormDataContent content = new();
            file.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");
            content.Add(file, "file", fileName);
            content.Add(new StringContent(serieId.ToString(CultureInfo.InvariantCulture)), "serieId");
            HttpResponseMessage response = await ManaxApiClient.Client.PostAsync("api/upload/chapter/replace", content);
            return new Optional<bool>(response.IsSuccessStatusCode);
        });
    }

    public static async Task<Optional<bool>> UploadPosterAsync(string file, string fileName, long serieId)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            using MultipartFormDataContent content = new();
            ByteArrayContent img = new(await File.ReadAllBytesAsync(file));
            img.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");
            content.Add(img, "file", fileName);
            content.Add(new StringContent(serieId.ToString(CultureInfo.InvariantCulture)), "serieId");
            HttpResponseMessage response = await ManaxApiClient.Client.PostAsync("api/upload/poster", content);
            return new Optional<bool>(response.IsSuccessStatusCode);
        });
    }

    public static async Task<Optional<bool>> ReplacePosterAsync(string file, string fileName, long serieId)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            using MultipartFormDataContent content = new();
            ByteArrayContent img = new(await File.ReadAllBytesAsync(file));
            img.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");
            content.Add(img, "file", fileName);
            content.Add(new StringContent(serieId.ToString(CultureInfo.InvariantCulture)), "serieId");
            HttpResponseMessage response = await ManaxApiClient.Client.PostAsync("api/upload/poster/replace", content);
            return new Optional<bool>(response.IsSuccessStatusCode);
        });
    }
}