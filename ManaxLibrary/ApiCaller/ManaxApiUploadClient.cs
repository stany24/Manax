using System.Globalization;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using ManaxLibrary.DTO.Chapter;
using ManaxLibrary.DTO.Serie;

namespace ManaxLibrary.ApiCaller;

public static partial class ManaxApiUploadClient
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

    [GeneratedRegex(@"[^\d]")]
    private static partial Regex RegexNotNumber();

    private static uint ExtractChapterNumber(string fileName)
    {
        foreach (string pattern in ChapterNumberPatterns)
        {
            Regex regex = new(pattern);
            Match match = regex.Match(fileName);
            if (!match.Success) continue;
            string numberStr = RegexNotNumber().Replace(match.Value, "");
            if (uint.TryParse(numberStr, out uint number)) return number;
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
                return Optional<bool>.Failure(serieCreateResponse.Error);

            long serieId = serieCreateResponse.GetValue();

            string? poster = Directory.GetFiles(directory, "*poster.*").FirstOrDefault();
            string? posterError = null;
            if (poster != null)
            {
                Optional<bool> posterResult = await UploadPosterAsync(poster, Path.GetFileName(poster), serieId);
                if (posterResult.Failed)
                    posterError = posterResult.Error;
            }

            string[] chapters = Directory.GetFiles(directory, "*.cbz");
            Array.Sort(chapters);

            foreach (string filePath in chapters)
            {
                NewChapterDto newChapterDto = new()
                {
                    Data = await File.ReadAllBytesAsync(filePath),
                    SerieId = (int)serieId,
                    Number = ExtractChapterNumber(Path.GetFileName(filePath))
                };
                Optional<bool> uploadChapterResponse = await UploadChapterAsync(newChapterDto);
                if (uploadChapterResponse.Failed)
                    return Optional<bool>.Failure(uploadChapterResponse.Error);
            }

            return posterError == null
                ? Optional<bool>.Success(true)
                : Optional<bool>.Failure(posterError);
        });
    }

    public static async Task<Optional<bool>> UploadChapterAsync(NewChapterDto dto)
    {
        return await ManaxApiClient.PostAsync<bool,NewChapterDto>("api/upload/chapter", dto);
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
            HttpResponseMessage response =
                await ManaxApiClient.UploadClient.PostAsync("api/upload/chapter/replace", content);
            return response.IsSuccessStatusCode
                ? Optional<bool>.Success(true)
                : Optional<bool>.Failure(response);
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
            HttpResponseMessage response = await ManaxApiClient.UploadClient.PostAsync("api/upload/poster", content);
            return response.IsSuccessStatusCode
                ? Optional<bool>.Success(true)
                : Optional<bool>.Failure(response);
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
            HttpResponseMessage response =
                await ManaxApiClient.UploadClient.PostAsync("api/upload/poster/replace", content);
            return response.IsSuccessStatusCode
                ? Optional<bool>.Success(true)
                : Optional<bool>.Failure(response);
        });
    }
}