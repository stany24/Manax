using System.Net.Http.Json;
using ManaxLibrary.DTO.Issue.Automatic;
using ManaxLibrary.DTO.Issue.Reported;

namespace ManaxLibrary.ApiCaller;

public static class ManaxApiIssueClient
{
    public static async Task<Optional<List<IssueChapterAutomaticDto>>> GetAllAutomaticChapterIssuesAsync()
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/issue/chapter/automatic");
            if (!response.IsSuccessStatusCode) return new Optional<List<IssueChapterAutomaticDto>>(response);
            List<IssueChapterAutomaticDto>? issues =
                await response.Content.ReadFromJsonAsync<List<IssueChapterAutomaticDto>>();
            return issues == null
                ? new Optional<List<IssueChapterAutomaticDto>>("Failed to read automatic chapter issues from response.")
                : new Optional<List<IssueChapterAutomaticDto>>(issues);
        });
    }

    public static async Task<Optional<List<IssueSerieAutomaticDto>>> GetAllAutomaticSerieIssuesAsync()
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/issue/serie/automatic");
            if (!response.IsSuccessStatusCode) return new Optional<List<IssueSerieAutomaticDto>>(response);
            List<IssueSerieAutomaticDto>? issues =
                await response.Content.ReadFromJsonAsync<List<IssueSerieAutomaticDto>>();
            return issues == null
                ? new Optional<List<IssueSerieAutomaticDto>>("Failed to read automatic serie issues from response.")
                : new Optional<List<IssueSerieAutomaticDto>>(issues);
        });
    }

    public static async Task<Optional<List<IssueChapterReportedDto>>> GetAllReportedChapterIssuesAsync()
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/issue/chapter/reported");
            if (!response.IsSuccessStatusCode) return new Optional<List<IssueChapterReportedDto>>(response);
            List<IssueChapterReportedDto>? issues =
                await response.Content.ReadFromJsonAsync<List<IssueChapterReportedDto>>();
            return issues == null
                ? new Optional<List<IssueChapterReportedDto>>("Failed to read reported chapter issues from response.")
                : new Optional<List<IssueChapterReportedDto>>(issues);
        });
    }

    public static async Task<Optional<List<IssueChapterReportedTypeDto>>> GetAllReportedChapterIssueTypesAsync()
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/issue/chapter/reported/types");
            if (!response.IsSuccessStatusCode) return new Optional<List<IssueChapterReportedTypeDto>>(response);
            List<IssueChapterReportedTypeDto>? issues =
                await response.Content.ReadFromJsonAsync<List<IssueChapterReportedTypeDto>>();
            return issues == null
                ? new Optional<List<IssueChapterReportedTypeDto>>(
                    "Failed to read reported chapter issue type from response.")
                : new Optional<List<IssueChapterReportedTypeDto>>(issues);
        });
    }

    public static async Task<Optional<List<IssueSerieReportedDto>>> GetAllReportedSerieIssuesAsync()
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/issue/serie/reported");
            if (!response.IsSuccessStatusCode) return new Optional<List<IssueSerieReportedDto>>(response);
            List<IssueSerieReportedDto>? issues =
                await response.Content.ReadFromJsonAsync<List<IssueSerieReportedDto>>();
            return issues == null
                ? new Optional<List<IssueSerieReportedDto>>("Failed to read reported serie issues from response.")
                : new Optional<List<IssueSerieReportedDto>>(issues);
        });
    }

    public static async Task<Optional<List<IssueSerieReportedTypeDto>>> GetAllReportedSerieIssueTypesAsync()
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.GetAsync("api/issue/serie/reported/types");
            if (!response.IsSuccessStatusCode) return new Optional<List<IssueSerieReportedTypeDto>>(response);
            List<IssueSerieReportedTypeDto>? issues =
                await response.Content.ReadFromJsonAsync<List<IssueSerieReportedTypeDto>>();
            return issues == null
                ? new Optional<List<IssueSerieReportedTypeDto>>(
                    "Failed to read reported serie issue type from response.")
                : new Optional<List<IssueSerieReportedTypeDto>>(issues);
        });
    }

    public static async Task<Optional<bool>> CreateChapterIssueAsync(IssueChapterReportedCreateDto issueChapterReported)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response =
                await ManaxApiClient.Client.PostAsJsonAsync("api/issue/chapter", issueChapterReported);
            return response.IsSuccessStatusCode
                ? new Optional<bool>(true)
                : new Optional<bool>(response);
        });
    }

    public static async Task<Optional<bool>> CreateSerieIssueAsync(IssueSerieReportedCreateDto issueSerieReported)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response =
                await ManaxApiClient.Client.PostAsJsonAsync("api/issue/serie", issueSerieReported);
            return response.IsSuccessStatusCode
                ? new Optional<bool>(true)
                : new Optional<bool>(response);
        });
    }

    public static async Task<Optional<bool>> CloseChapterIssueAsync(long id)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.PutAsync($"api/issue/chapter/{id}/close", null);
            return response.IsSuccessStatusCode
                ? new Optional<bool>(true)
                : new Optional<bool>(response);
        });
    }

    public static async Task<Optional<bool>> CloseSerieIssueAsync(long id)
    {
        return await ManaxApiClient.ExecuteWithErrorHandlingAsync(async () =>
        {
            HttpResponseMessage response = await ManaxApiClient.Client.PutAsync($"api/issue/serie/{id}/close", null);
            return response.IsSuccessStatusCode
                ? new Optional<bool>(true)
                : new Optional<bool>(response);
        });
    }
}